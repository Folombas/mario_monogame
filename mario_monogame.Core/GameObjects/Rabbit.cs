using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Главный герой игры - зайчик, который может бегать, прыгать и собирать морковку.
    /// </summary>
    public class Rabbit : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private readonly float _groundY;
        private Texture2D _pixelTexture;
        
        // Физика
        private bool _isJumping;
        private bool _isGrounded;
        private float _jumpForce;
        private readonly float _gravity;
        private readonly float _moveSpeed;
        private readonly float _jumpStrength;
        
        // Состояние
        private bool _facingRight;
        private float _walkAnimationTime;
        private bool _isWalking;
        private float _carrotCarryAngle;
        
        // Размеры
        private readonly float _width;
        private readonly float _height;
        
        // Цвета
        private readonly Color _furColor;
        private readonly Color _earInnerColor;
        private readonly Color _eyeColor;
        private readonly Color _noseColor;

        public Vector2 Position => _position;
        public bool IsGrounded => _isGrounded;
        public int CarrotsCollected { get; private set; }
        public bool HasCarrot { get; private set; }

        public Rabbit(GraphicsDevice graphicsDevice, Vector2 startPosition, float groundY)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _groundY = groundY;
            _velocity = Vector2.Zero;
            
            // Физические параметры
            _gravity = 1500f;
            _moveSpeed = 300f;
            _jumpStrength = 650f;
            _jumpForce = 0f;
            
            // Размеры зайчика
            _width = 50f;
            _height = 60f;
            
            // Цвета
            _furColor = new Color(240, 240, 240);
            _earInnerColor = new Color(255, 180, 180);
            _eyeColor = Color.Black;
            _noseColor = new Color(255, 100, 100);
            
            // Создаём белый пиксель для отрисовки
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _isGrounded = true;
            _facingRight = true;
            _walkAnimationTime = 0f;
            _isWalking = false;
            _carrotCarryAngle = 0f;
            CarrotsCollected = 0;
            HasCarrot = false;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, int screenWidth)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Управление
            _isWalking = false;
            
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            {
                _velocity.X = -_moveSpeed;
                _facingRight = false;
                _isWalking = true;
            }
            else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            {
                _velocity.X = _moveSpeed;
                _facingRight = true;
                _isWalking = true;
            }
            else
            {
                _velocity.X = 0f;
            }
            
            // Прыжок
            if ((keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up)) && _isGrounded)
            {
                _isJumping = true;
                _isGrounded = false;
                _jumpForce = _jumpStrength;
            }
            
            // Применяем гравитацию
            if (!_isGrounded)
            {
                _jumpForce -= _gravity * elapsed;
                _velocity.Y = _jumpForce;
            }
            else
            {
                _velocity.Y = 0f;
            }
            
            // Обновляем позицию
            _position.X += _velocity.X * elapsed;
            _position.Y += _velocity.Y * elapsed;
            
            // Ограничение по экрану
            if (_position.X < _width / 2)
                _position.X = _width / 2;
            if (_position.X > screenWidth - _width / 2)
                _position.X = screenWidth - _width / 2;
            
            // Проверка земли
            if (_position.Y >= _groundY)
            {
                _position.Y = _groundY;
                _isGrounded = true;
                _isJumping = false;
                _jumpForce = 0f;
            }
            
            // Анимация ходьбы
            if (_isWalking && _isGrounded)
            {
                _walkAnimationTime += elapsed * 10f;
            }
            else
            {
                _walkAnimationTime = 0f;
            }
            
            // Анимация морковки
            if (HasCarrot)
            {
                _carrotCarryAngle += elapsed * 3f;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float legOffset = (float)Math.Sin(_walkAnimationTime) * 5f;
            float earWiggle = (float)Math.Sin(_walkAnimationTime * 0.5f) * 0.1f;
            
            // Рисуем зайчика
            DrawBody(spriteBatch, legOffset);
            DrawHead(spriteBatch, earWiggle);
            DrawEars(spriteBatch, earWiggle);
            DrawFace(spriteBatch);
            
            // Если несёт морковку
            if (HasCarrot)
            {
                DrawCarriedCarrot(spriteBatch);
            }
        }

        private void DrawBody(SpriteBatch spriteBatch, float legOffset)
        {
            float bodyWidth = _width * 0.7f;
            float bodyHeight = _height * 0.5f;
            
            // Тело (овал)
            DrawEllipse(spriteBatch, _position, bodyWidth, bodyHeight, _furColor, 0f);
            
            // Передние лапки
            DrawLeg(spriteBatch, _position + new Vector2(-10f, bodyHeight * 0.4f), legOffset, true);
            DrawLeg(spriteBatch, _position + new Vector2(10f, bodyHeight * 0.4f), -legOffset, true);
            
            // Задние лапки
            DrawFoot(spriteBatch, _position + new Vector2(-15f, bodyHeight * 0.3f), -legOffset * 0.5f);
            DrawFoot(spriteBatch, _position + new Vector2(15f, bodyHeight * 0.3f), legOffset * 0.5f);
            
            // Хвостик (помпон)
            float tailX = _facingRight ? -bodyWidth * 0.4f : bodyWidth * 0.4f;
            DrawCircle(spriteBatch, _position + new Vector2(tailX, 0), 8f, _furColor);
        }

        private void DrawLeg(SpriteBatch spriteBatch, Vector2 position, float offset, bool isFront)
        {
            float legWidth = 8f;
            float legLength = 20f;
            
            spriteBatch.Draw(
                _pixelTexture,
                position + new Vector2(0, offset),
                null,
                _furColor,
                0f,
                new Vector2(legWidth / 2, 0),
                new Vector2(legWidth, legLength),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawFoot(SpriteBatch spriteBatch, Vector2 position, float offset)
        {
            float footWidth = 12f;
            float footHeight = 6f;
            
            DrawEllipse(spriteBatch, position + new Vector2(0, offset + 10f), footWidth, footHeight, _furColor, 0f);
        }

        private void DrawHead(SpriteBatch spriteBatch, float earWiggle)
        {
            float headSize = _height * 0.45f;
            float headOffset = _facingRight ? headSize * 0.6f : -headSize * 0.6f;
            
            // Голова (круг)
            DrawCircle(spriteBatch, _position + new Vector2(headOffset, -headSize * 0.3f), headSize, _furColor);
        }

        private void DrawEars(SpriteBatch spriteBatch, float earWiggle)
        {
            float earWidth = 8f;
            float earLength = 35f;
            float headOffset = _facingRight ? _height * 0.45f * 0.6f : -_height * 0.45f * 0.6f;
            Vector2 headPos = _position + new Vector2(headOffset, -_height * 0.3f);
            
            // Левое ухо
            float leftEarAngle = -0.15f + earWiggle;
            DrawEar(spriteBatch, headPos + new Vector2(-10f, -earLength * 0.5f), earWidth, earLength, leftEarAngle);
            
            // Правое ухо
            float rightEarAngle = 0.15f - earWiggle;
            DrawEar(spriteBatch, headPos + new Vector2(10f, -earLength * 0.5f), earWidth, earLength, rightEarAngle);
        }

        private void DrawEar(SpriteBatch spriteBatch, Vector2 position, float width, float length, float angle)
        {
            // Внешняя часть уха
            spriteBatch.Draw(
                _pixelTexture,
                position,
                null,
                _furColor,
                angle,
                new Vector2(width / 2, 0),
                new Vector2(width, length),
                SpriteEffects.None,
                0f
            );
            
            // Внутренняя часть (розовая)
            float innerWidth = width * 0.5f;
            float innerLength = length * 0.6f;
            spriteBatch.Draw(
                _pixelTexture,
                position + new Vector2(width * 0.25f, length * 0.2f),
                null,
                _earInnerColor,
                angle,
                new Vector2(innerWidth / 2, 0),
                new Vector2(innerWidth, innerLength),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawFace(SpriteBatch spriteBatch)
        {
            float headOffset = _facingRight ? _height * 0.45f * 0.6f : -_height * 0.45f * 0.6f;
            Vector2 headPos = _position + new Vector2(headOffset, -_height * 0.3f);
            float eyeOffset = _facingRight ? 8f : -8f;
            
            // Глаза
            DrawCircle(spriteBatch, headPos + new Vector2(eyeOffset, -5f), 5f, _eyeColor);
            
            // Блик в глазах
            DrawCircle(spriteBatch, headPos + new Vector2(eyeOffset + 2f, -7f), 2f, Color.White);
            
            // Нос
            float noseX = _facingRight ? headPos.X + 12f : headPos.X - 12f;
            DrawCircle(spriteBatch, new Vector2(noseX, headPos.Y + 8f), 4f, _noseColor);
            
            // Усы
            DrawWhiskers(spriteBatch, new Vector2(noseX, headPos.Y + 8f));
        }

        private void DrawWhiskers(SpriteBatch spriteBatch, Vector2 nosePosition)
        {
            float whiskerLength = 15f;
            float direction = _facingRight ? 1f : -1f;
            
            for (int i = -1; i <= 1; i++)
            {
                float angle = i * 0.2f;
                float endX = nosePosition.X + (float)Math.Cos(angle) * whiskerLength * direction;
                float endY = nosePosition.Y + (float)Math.Sin(angle) * whiskerLength;
                
                DrawLine(spriteBatch, nosePosition, new Vector2(endX, endY), Color.Gray, 1f);
            }
        }

        private void DrawCarriedCarrot(SpriteBatch spriteBatch)
        {
            float carrotAngle = (float)Math.Sin(_carrotCarryAngle) * 0.1f + 0.3f;
            float handOffset = _facingRight ? 25f : -25f;
            Vector2 handPos = _position + new Vector2(handOffset, 10f);
            
            // Морковка в лапках
            float carrotLength = 20f;
            float carrotWidth = 8f;
            
            Vector2 carrotEnd = handPos + new Vector2(
                (float)Math.Sin(carrotAngle) * carrotLength * (_facingRight ? 1 : -1),
                -(float)Math.Cos(carrotAngle) * carrotLength
            );
            
            // Тело морковки
            DrawLine(spriteBatch, handPos, carrotEnd, new Color(255, 140, 0), carrotWidth);
            
            // Ботва
            DrawCarrotTop(spriteBatch, carrotEnd, carrotAngle);
        }

        private void DrawCarrotTop(SpriteBatch spriteBatch, Vector2 position, float angle)
        {
            for (int i = 0; i < 3; i++)
            {
                float leafAngle = angle + (i - 1) * 0.4f;
                float leafLength = 8f;
                Vector2 leafEnd = position + new Vector2(
                    (float)Math.Sin(leafAngle) * leafLength,
                    -(float)Math.Cos(leafAngle) * leafLength
                );
                DrawLine(spriteBatch, position, leafEnd, new Color(34, 139, 34), 2f);
            }
        }

        public void CollectCarrot()
        {
            if (!HasCarrot)
            {
                HasCarrot = true;
                CarrotsCollected++;
            }
        }

        public void DropCarrot()
        {
            HasCarrot = false;
        }

        public void Jump()
        {
            if (_isGrounded)
            {
                _isJumping = true;
                _isGrounded = false;
                _jumpForce = _jumpStrength;
            }
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            DrawEllipse(spriteBatch, center, radius * 2, radius * 2, color, 0f);
        }

        private void DrawEllipse(SpriteBatch spriteBatch, Vector2 center, float width, float height, Color color, float rotation)
        {
            int segments = 36;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(i * Math.PI * 2 / segments);
                float angle2 = (float)((i + 1) * Math.PI * 2 / segments);
                
                Vector2 point1 = new Vector2(
                    center.X + (float)Math.Cos(angle1) * width / 2,
                    center.Y + (float)Math.Sin(angle1) * height / 2
                );
                Vector2 point2 = new Vector2(
                    center.X + (float)Math.Cos(angle2) * width / 2,
                    center.Y + (float)Math.Sin(angle2) * height / 2
                );
                
                DrawTriangle(spriteBatch, center, point1, point2, color);
            }
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            float minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            float maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            float minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            float maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
            
            for (float x = minX; x <= maxX; x += 2f)
            {
                for (float y = minY; y <= maxY; y += 2f)
                {
                    if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                    {
                        spriteBatch.Draw(_pixelTexture, new Vector2(x, y), color);
                    }
                }
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);
            
            spriteBatch.Draw(
                _pixelTexture,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f
            );
        }

        private bool IsPointInTriangle(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float sign(Vector2 a, Vector2 b, Vector2 c)
            {
                return (a.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (a.Y - c.Y);
            }
            
            bool b1 = sign(point, p1, p2) < 0.0f;
            bool b2 = sign(point, p2, p3) < 0.0f;
            bool b3 = sign(point, p3, p1) < 0.0f;
            
            return (b1 == b2) && (b2 == b3);
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }
}
