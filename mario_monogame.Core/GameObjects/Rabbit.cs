using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Улучшенный зайчик с плавной анимацией и улучшенной физикой.
    /// </summary>
    public class Rabbit : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private readonly float _groundY;
        private Texture2D _pixelTexture;
        
        // Улучшенная физика
        private bool _isJumping;
        private bool _isGrounded;
        private float _jumpForce;
        private readonly float _gravity;
        private readonly float _maxSpeed;
        private readonly float _acceleration;
        private readonly float _friction;
        private readonly float _jumpStrength;
        private float _facingDirection;
        
        // Состояние
        private bool _facingRight;
        private float _walkAnimationTime;
        private bool _isWalking;
        private float _carrotCarryAngle;
        
        // Анимация моргания
        private float _blinkTimer;
        private float _blinkDuration;
        private bool _isBlinking;
        
        // Дыхание/пульсация
        private float _breathTimer;
        
        // Размеры
        private readonly float _width;
        private readonly float _height;
        
        // Цвета с градиентами
        private readonly Color _furColor;
        private readonly Color _furShadowColor;
        private readonly Color _earInnerColor;
        private readonly Color _eyeColor;
        private readonly Color _noseColor;
        private readonly Color _cheekColor;

        public Vector2 Position => _position;
        public bool IsGrounded => _isGrounded;
        public int CarrotsCollected { get; private set; }
        public bool HasCarrot { get; private set; }
        public float Velocity => Math.Abs(_velocity.X);

        public Rabbit(GraphicsDevice graphicsDevice, Vector2 startPosition, float groundY)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _groundY = groundY;
            _velocity = Vector2.Zero;
            
            // Улучшенные физические параметры
            _gravity = 2000f;              // Более сильная гравитация
            _maxSpeed = 400f;              // Максимальная скорость
            _acceleration = 1500f;         // Ускорение
            _friction = 1200f;             // Трение
            _jumpStrength = 750f;          // Сила прыжка
            _jumpForce = 0f;
            
            // Размеры зайчика
            _width = 60f;
            _height = 70f;
            
            // Улучшенные цвета
            _furColor = new Color(245, 245, 250);
            _furShadowColor = new Color(200, 200, 210);
            _earInnerColor = new Color(255, 190, 190);
            _eyeColor = new Color(30, 30, 40);
            _noseColor = new Color(255, 120, 120);
            _cheekColor = new Color(255, 180, 180, 150);
            
            // Создаём белый пиксель для отрисовки
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _isGrounded = true;
            _facingRight = true;
            _facingDirection = 1f;
            _walkAnimationTime = 0f;
            _isWalking = false;
            _carrotCarryAngle = 0f;
            CarrotsCollected = 0;
            HasCarrot = false;
            
            // Анимация моргания
            _blinkTimer = 0f;
            _blinkDuration = 0f;
            _isBlinking = false;
            
            // Дыхание
            _breathTimer = 0f;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, int screenWidth)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Сброс состояния ходьбы
            _isWalking = false;
            
            // Улучшенное управление с инерцией
            float targetVelocity = 0f;
            
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            {
                targetVelocity = -_maxSpeed;
                _facingRight = false;
                _facingDirection = -1f;
                _isWalking = true;
            }
            else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            {
                targetVelocity = _maxSpeed;
                _facingRight = true;
                _facingDirection = 1f;
                _isWalking = true;
            }
            
            // Плавное ускорение/замедление
            if (targetVelocity != 0f)
            {
                if (_velocity.X < targetVelocity)
                {
                    _velocity.X = Math.Min(_velocity.X + _acceleration * elapsed, targetVelocity);
                }
                else if (_velocity.X > targetVelocity)
                {
                    _velocity.X = Math.Max(_velocity.X - _acceleration * elapsed, targetVelocity);
                }
            }
            else
            {
                // Применение трения при остановке
                if (Math.Abs(_velocity.X) > 10f)
                {
                    _velocity.X -= Math.Sign(_velocity.X) * _friction * elapsed;
                }
                else
                {
                    _velocity.X = 0f;
                }
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
            {
                _position.X = _width / 2;
                _velocity.X = 0f;
            }
            if (_position.X > screenWidth - _width / 2)
            {
                _position.X = screenWidth - _width / 2;
                _velocity.X = 0f;
            }
            
            // Проверка земли
            if (_position.Y >= _groundY)
            {
                _position.Y = _groundY;
                _isGrounded = true;
                _isJumping = false;
                _jumpForce = 0f;
            }
            
            // Анимация ходьбы (более плавная)
            if (_isWalking && _isGrounded)
            {
                _walkAnimationTime += elapsed * (8f + Math.Abs(_velocity.X) * 0.02f);
            }
            else
            {
                // Плавное затухание анимации
                _walkAnimationTime = 0f;
            }
            
            // Анимация морковки
            if (HasCarrot)
            {
                _carrotCarryAngle += elapsed * 3f;
            }
            
            // Анимация моргания
            UpdateBlinking(elapsed);
            
            // Анимация дыхания
            _breathTimer += elapsed * 2f;
        }
        
        private void UpdateBlinking(float elapsed)
        {
            if (_isBlinking)
            {
                _blinkDuration -= elapsed;
                if (_blinkDuration <= 0f)
                {
                    _isBlinking = false;
                    _blinkTimer = 2f + (float)(new Random().NextDouble() * 3f);
                }
            }
            else
            {
                _blinkTimer -= elapsed;
                if (_blinkTimer <= 0f)
                {
                    _isBlinking = true;
                    _blinkDuration = 0.1f + (float)(new Random().NextDouble() * 0.1f);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float legOffset = (float)Math.Sin(_walkAnimationTime) * 6f;
            float earWiggle = (float)Math.Sin(_walkAnimationTime * 0.5f) * 0.15f;
            float bodyBob = (float)Math.Sin(_walkAnimationTime * 0.5f) * 2f;
            float breathScale = 1f + (float)Math.Sin(_breathTimer) * 0.02f;
            
            // Тень под зайчиком
            DrawShadow(spriteBatch);
            
            // Рисуем зайчика
            DrawBody(spriteBatch, legOffset, bodyBob, breathScale);
            DrawHead(spriteBatch, earWiggle, bodyBob, breathScale);
            DrawEars(spriteBatch, earWiggle, bodyBob);
            DrawFace(spriteBatch, breathScale);
            
            // Если несёт морковку
            if (HasCarrot)
            {
                DrawCarriedCarrot(spriteBatch);
            }
        }
        
        private void DrawShadow(SpriteBatch spriteBatch)
        {
            float shadowWidth = _width * 0.8f;
            float shadowHeight = 15f;
            float shadowAlpha = _isGrounded ? 0.3f : 0.15f;
            float shadowScale = _isGrounded ? 1f : 0.7f;
            
            DrawEllipse(spriteBatch,
                _position + new Vector2(0, _height * 0.45f),
                shadowWidth * shadowScale,
                shadowHeight * shadowScale,
                new Color((byte)0, (byte)0, (byte)0, (byte)(shadowAlpha * 255)),
                0f);
        }

        private void DrawBody(SpriteBatch spriteBatch, float legOffset, float bodyBob, float breathScale)
        {
            float bodyWidth = _width * 0.75f * breathScale;
            float bodyHeight = _height * 0.55f * breathScale;
            
            // Тело с градиентом (основная часть)
            DrawGradientEllipse(spriteBatch, _position + new Vector2(0, bodyBob), 
                bodyWidth, bodyHeight, _furColor, _furShadowColor, 0f);
            
            // Брюшко (светлее)
            DrawEllipse(spriteBatch, _position + new Vector2(0, 5f + bodyBob), 
                bodyWidth * 0.5f, bodyHeight * 0.4f, new Color(255, 255, 255), 0f);
            
            // Передние лапки
            DrawLeg(spriteBatch, _position + new Vector2(-12f, bodyHeight * 0.3f + bodyBob), legOffset, true);
            DrawLeg(spriteBatch, _position + new Vector2(12f, bodyHeight * 0.3f + bodyBob), -legOffset, true);
            
            // Задние лапки
            DrawFoot(spriteBatch, _position + new Vector2(-18f, bodyHeight * 0.2f + bodyBob), -legOffset * 0.3f);
            DrawFoot(spriteBatch, _position + new Vector2(18f, bodyHeight * 0.2f + bodyBob), legOffset * 0.3f);
            
            // Хвостик (помпон)
            float tailX = _facingRight ? -bodyWidth * 0.45f : bodyWidth * 0.45f;
            float tailWag = (float)Math.Sin(_walkAnimationTime * 2f) * 3f;
            DrawCircle(spriteBatch, _position + new Vector2(tailX, tailWag + bodyBob), 10f, _furColor);
            DrawCircle(spriteBatch, _position + new Vector2(tailX, tailWag + bodyBob), 6f, Color.White);
        }

        private void DrawLeg(SpriteBatch spriteBatch, Vector2 position, float offset, bool isFront)
        {
            float legWidth = 10f;
            float legLength = 22f;
            
            // Основная часть лапки
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
            
            // Подушечка лапки
            DrawEllipse(spriteBatch, position + new Vector2(0, offset + legLength), 
                legWidth * 0.9f, 5f, new Color(255, 180, 180), 0f);
        }

        private void DrawFoot(SpriteBatch spriteBatch, Vector2 position, float offset)
        {
            float footWidth = 14f;
            float footHeight = 8f;
            
            DrawEllipse(spriteBatch, position + new Vector2(0, offset + 10f), footWidth, footHeight, _furColor, 0f);
            
            // Пальчики
            for (int i = -1; i <= 1; i++)
            {
                DrawCircle(spriteBatch, position + new Vector2(i * 5f, offset + 6f), 3f, Color.White);
            }
        }

        private void DrawHead(SpriteBatch spriteBatch, float earWiggle, float bodyBob, float breathScale)
        {
            float headSize = _height * 0.5f * breathScale;
            float headOffset = _facingRight ? headSize * 0.65f : -headSize * 0.65f;
            
            // Голова (круг с градиентом)
            DrawGradientCircle(spriteBatch, _position + new Vector2(headOffset, -headSize * 0.35f + bodyBob), 
                headSize, _furColor, _furShadowColor);
            
            // Щёчки
            float cheekX = _facingRight ? headOffset + 8f : headOffset - 8f;
            DrawCircle(spriteBatch, _position + new Vector2(cheekX, -headSize * 0.15f + bodyBob), 6f, _cheekColor);
        }

        private void DrawEars(SpriteBatch spriteBatch, float earWiggle, float bodyBob)
        {
            float earWidth = 10f;
            float earLength = 40f;
            float headOffset = _facingRight ? _height * 0.5f * 0.65f : -_height * 0.5f * 0.65f;
            Vector2 headPos = _position + new Vector2(headOffset, -_height * 0.35f + bodyBob);
            
            // Левое ухо
            float leftEarAngle = -0.2f + earWiggle;
            DrawEar(spriteBatch, headPos + new Vector2(-12f, -earLength * 0.5f), earWidth, earLength, leftEarAngle);
            
            // Правое ухо
            float rightEarAngle = 0.2f - earWiggle;
            DrawEar(spriteBatch, headPos + new Vector2(12f, -earLength * 0.5f), earWidth, earLength, rightEarAngle);
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
            float innerLength = length * 0.65f;
            spriteBatch.Draw(
                _pixelTexture,
                position + new Vector2(width * 0.25f, length * 0.15f),
                null,
                _earInnerColor,
                angle,
                new Vector2(innerWidth / 2, 0),
                new Vector2(innerWidth, innerLength),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawFace(SpriteBatch spriteBatch, float breathScale)
        {
            float headOffset = _facingRight ? _height * 0.5f * 0.65f * breathScale : -_height * 0.5f * 0.65f * breathScale;
            Vector2 headPos = _position + new Vector2(headOffset, -_height * 0.35f);
            float eyeOffset = _facingRight ? 10f : -10f;
            
            // Глаза
            if (!_isBlinking)
            {
                // Открытые глаза
                DrawCircle(spriteBatch, headPos + new Vector2(eyeOffset, -5f), 6f, _eyeColor);
                
                // Блик в глазах
                DrawCircle(spriteBatch, headPos + new Vector2(eyeOffset + 2f, -7f), 2.5f, Color.White);
            }
            else
            {
                // Закрытые глаза (линия)
                DrawLine(spriteBatch, 
                    headPos + new Vector2(eyeOffset - 5f, -5f),
                    headPos + new Vector2(eyeOffset + 5f, -5f),
                    _eyeColor, 2f);
            }
            
            // Нос
            float noseX = _facingRight ? headPos.X + 14f : headPos.X - 14f;
            DrawCircle(spriteBatch, new Vector2(noseX, headPos.Y + 8f), 5f, _noseColor);
            DrawCircle(spriteBatch, new Vector2(noseX, headPos.Y + 8f), 2f, Color.White);
            
            // Усы
            DrawWhiskers(spriteBatch, new Vector2(noseX, headPos.Y + 8f));
        }

        private void DrawWhiskers(SpriteBatch spriteBatch, Vector2 nosePosition)
        {
            float whiskerLength = 18f;
            float direction = _facingRight ? 1f : -1f;
            
            for (int i = -1; i <= 1; i++)
            {
                float angle = i * 0.25f + (float)Math.Sin(_walkAnimationTime * 0.5f) * 0.1f;
                float endX = nosePosition.X + (float)Math.Cos(angle) * whiskerLength * direction;
                float endY = nosePosition.Y + (float)Math.Sin(angle) * whiskerLength;
                
                DrawLine(spriteBatch, nosePosition, new Vector2(endX, endY), Color.Gray, 1.5f);
            }
        }

        private void DrawCarriedCarrot(SpriteBatch spriteBatch)
        {
            float carrotAngle = (float)Math.Sin(_carrotCarryAngle) * 0.15f + 0.35f;
            float handOffset = _facingRight ? 28f : -28f;
            Vector2 handPos = _position + new Vector2(handOffset, 10f);
            
            // Морковка в лапках
            float carrotLength = 24f;
            float carrotWidth = 10f;
            
            Vector2 carrotEnd = handPos + new Vector2(
                (float)Math.Sin(carrotAngle) * carrotLength * (_facingRight ? 1 : -1),
                -(float)Math.Cos(carrotAngle) * carrotLength
            );
            
            // Тело морковки (градиент)
            DrawLine(spriteBatch, handPos, carrotEnd, new Color(255, 150, 0), carrotWidth);
            DrawLine(spriteBatch, handPos, carrotEnd, new Color(255, 180, 50), carrotWidth * 0.5f);
            
            // Ботва
            DrawCarrotTop(spriteBatch, carrotEnd, carrotAngle);
        }

        private void DrawCarrotTop(SpriteBatch spriteBatch, Vector2 position, float angle)
        {
            for (int i = 0; i < 3; i++)
            {
                float leafAngle = angle + (i - 1) * 0.5f;
                float leafLength = 10f;
                Vector2 leafEnd = position + new Vector2(
                    (float)Math.Sin(leafAngle) * leafLength,
                    -(float)Math.Cos(leafAngle) * leafLength
                );
                DrawLine(spriteBatch, position, leafEnd, new Color(50, 160, 50), 3f);
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
            int segments = 32;
            
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
        
        private void DrawGradientEllipse(SpriteBatch spriteBatch, Vector2 center, float width, float height, Color lightColor, Color darkColor, float rotation)
        {
            int segments = 32;
            
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
                
                // Градиент от светлого к тёмному
                Color color = i < segments / 2 ? lightColor : darkColor;
                DrawTriangle(spriteBatch, center, point1, point2, color);
            }
        }
        
        private void DrawGradientCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color lightColor, Color darkColor)
        {
            DrawGradientEllipse(spriteBatch, center, radius * 2, radius * 2, lightColor, darkColor, 0f);
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
