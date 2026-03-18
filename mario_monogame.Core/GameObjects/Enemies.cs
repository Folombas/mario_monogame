using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Лиса - враг зайчика, который бегает по земле и патрулирует территорию.
    /// </summary>
    public class Fox : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private readonly float _groundY;
        private Texture2D _pixelTexture;
        
        // Патрулирование
        private float _patrolStart;
        private float _patrolEnd;
        private float _moveSpeed;
        private bool _facingRight;
        
        // Анимация
        private float _walkAnimationTime;
        private float _tailWagTime;
        
        // Размеры
        private readonly float _width;
        private readonly float _height;
        
        // Цвета
        private readonly Color _furColor;
        private readonly Color _bellyColor;
        private readonly Color _tailTipColor;

        public Vector2 Position => _position;
        public float InteractionRadius => 40f;
        public bool IsDead { get; private set; }

        public Fox(GraphicsDevice graphicsDevice, Vector2 startPosition, float groundY, float patrolStart, float patrolEnd)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _groundY = groundY;
            _patrolStart = patrolStart;
            _patrolEnd = patrolEnd;
            _moveSpeed = 80f;
            _facingRight = true;
            
            _width = 60f;
            _height = 35f;
            
            _furColor = new Color(220, 100, 50);
            _bellyColor = new Color(255, 200, 150);
            _tailTipColor = Color.White;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _walkAnimationTime = 0f;
            _tailWagTime = 0f;
            IsDead = false;
        }

        public void Update(GameTime gameTime, Vector2 rabbitPosition)
        {
            if (IsDead) return;
            
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Движение патрулирования
            if (_facingRight)
            {
                _position.X += _moveSpeed * elapsed;
                if (_position.X >= _patrolEnd)
                {
                    _facingRight = false;
                }
            }
            else
            {
                _position.X -= _moveSpeed * elapsed;
                if (_position.X <= _patrolStart)
                {
                    _facingRight = true;
                }
            }
            
            // Анимация ходьбы
            _walkAnimationTime += elapsed * 8f;
            _tailWagTime += elapsed * 5f;
            
            // Проверка столкновения с зайчиком
            float distance = Vector2.Distance(_position, rabbitPosition);
            if (distance < InteractionRadius)
            {
                // Зайчик пойман!
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead) return;
            
            float legOffset = (float)Math.Sin(_walkAnimationTime) * 5f;
            float tailAngle = (float)Math.Sin(_tailWagTime) * 0.3f;
            
            DrawBody(spriteBatch, legOffset);
            DrawTail(spriteBatch, tailAngle);
            DrawHead(spriteBatch);
            DrawLegs(spriteBatch, legOffset);
        }
        
        private void DrawBody(SpriteBatch spriteBatch, float legOffset)
        {
            float bodyWidth = _width * 0.7f;
            float bodyHeight = _height * 0.6f;
            
            // Основное тело (овал)
            DrawEllipse(spriteBatch, _position, bodyWidth, bodyHeight, _furColor, 0f);
            
            // Брюшко (светлее)
            DrawEllipse(spriteBatch, _position + new Vector2(0, 5f), bodyWidth * 0.6f, bodyHeight * 0.4f, _bellyColor, 0f);
        }
        
        private void DrawTail(SpriteBatch spriteBatch, float angle)
        {
            float tailLength = 35f;
            float tailWidth = 12f;
            float tailX = _facingRight ? -_width * 0.3f : _width * 0.3f;
            float direction = _facingRight ? -1f : 1f;
            
            // Хвост
            Vector2 tailStart = _position + new Vector2(tailX, 0);
            Vector2 tailEnd = tailStart + new Vector2(
                (float)Math.Sin(angle) * tailLength * direction,
                -(float)Math.Cos(angle) * tailLength * 0.5f
            );
            
            DrawLine(spriteBatch, tailStart, tailEnd, _furColor, tailWidth);
            
            // Белый кончик хвоста
            Vector2 tipPos = tailEnd;
            DrawCircle(spriteBatch, tipPos, 6f, _tailTipColor);
        }
        
        private void DrawHead(SpriteBatch spriteBatch)
        {
            float headSize = _height * 0.5f;
            float headOffset = _facingRight ? headSize * 0.7f : -headSize * 0.7f;
            
            // Голова
            DrawCircle(spriteBatch, _position + new Vector2(headOffset, -headSize * 0.3f), headSize, _furColor);
            
            // Уши
            float earSize = 10f;
            float earOffset = _facingRight ? 5f : -5f;
            DrawTriangle(spriteBatch, 
                _position + new Vector2(earOffset - 5f, -headSize * 0.8f),
                _position + new Vector2(earOffset + 5f, -headSize * 0.8f),
                _position + new Vector2(earOffset, -headSize * 1.2f),
                _furColor);
            
            // Мордочка
            float snoutX = _facingRight ? headOffset + headSize * 0.5f : headOffset - headSize * 0.5f;
            DrawCircle(spriteBatch, _position + new Vector2(snoutX, -headSize * 0.1f), 8f, _bellyColor);
            
            // Нос
            Color noseColor = _facingRight ? 
                new Color((byte)(_position.X + snoutX), 50, 50) : 
                new Color(50, 50, 50);
            DrawCircle(spriteBatch, _position + new Vector2(snoutX + (_facingRight ? 6f : -6f), -headSize * 0.2f), 3f, Color.Black);
            
            // Глаза
            float eyeX = _facingRight ? headOffset + 3f : headOffset - 3f;
            DrawCircle(spriteBatch, _position + new Vector2(eyeX, -headSize * 0.4f), 3f, Color.Black);
        }
        
        private void DrawLegs(SpriteBatch spriteBatch, float legOffset)
        {
            float legWidth = 6f;
            float legLength = 15f;
            
            // Передние лапы
            DrawLine(spriteBatch,
                _position + new Vector2(-10f, _height * 0.2f),
                _position + new Vector2(-10f + legOffset, _height * 0.5f),
                _furColor, legWidth);
            
            DrawLine(spriteBatch,
                _position + new Vector2(10f, _height * 0.2f),
                _position + new Vector2(10f - legOffset, _height * 0.5f),
                _furColor, legWidth);
        }

        public void Scare()
        {
            // Лиса убегает
            _moveSpeed = 200f;
        }
        
        public void Kill()
        {
            IsDead = true;
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            DrawEllipse(spriteBatch, center, radius * 2, radius * 2, color, 0f);
        }

        private void DrawEllipse(SpriteBatch spriteBatch, Vector2 center, float width, float height, Color color, float rotation)
        {
            int segments = 24;
            
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

    /// <summary>
    /// Ворона - летающий враг, который кружит над определённой зоной.
    /// </summary>
    public class Crow : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private readonly Vector2 _center;
        private readonly float _orbitRadius;
        private float _orbitAngle;
        private float _orbitSpeed;
        private Texture2D _pixelTexture;
        
        // Анимация крыльев
        private float _wingFlapTime;
        private float _wingAngle;
        
        // Размеры
        private readonly float _bodyWidth;
        private readonly float _bodyHeight;
        private readonly float _wingSpan;

        public Vector2 Position => _position;
        public float InteractionRadius => 30f;

        public Crow(GraphicsDevice graphicsDevice, Vector2 center, float orbitRadius, float height)
        {
            _graphicsDevice = graphicsDevice;
            _center = center;
            _orbitRadius = orbitRadius;
            _position = new Vector2(center.X + orbitRadius, height);
            _orbitAngle = 0f;
            _orbitSpeed = 0.5f;
            
            _bodyWidth = 30f;
            _bodyHeight = 20f;
            _wingSpan = 40f;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _wingFlapTime = 0f;
            _wingAngle = 0f;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Орбитальное движение
            _orbitAngle += _orbitSpeed * elapsed;
            _position.X = _center.X + (float)Math.Cos(_orbitAngle) * _orbitRadius;
            _position.Y = _center.Y + (float)Math.Sin(_orbitAngle * 0.5f) * 30f;
            
            // Анимация крыльев
            _wingFlapTime += elapsed * 8f;
            _wingAngle = (float)Math.Sin(_wingFlapTime) * 0.5f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawWings(spriteBatch);
            DrawBody(spriteBatch);
            DrawHead(spriteBatch);
        }
        
        private void DrawWings(SpriteBatch spriteBatch)
        {
            float wingWidth = _wingSpan * 0.3f;
            float wingLength = _wingSpan * 0.5f;
            
            // Левое крыло
            spriteBatch.Draw(
                _pixelTexture,
                _position + new Vector2(-10f, -5f),
                null,
                Color.Black,
                _wingAngle,
                new Vector2(wingWidth / 2, 0),
                new Vector2(wingWidth, wingLength),
                SpriteEffects.None,
                0f
            );
            
            // Правое крыло
            spriteBatch.Draw(
                _pixelTexture,
                _position + new Vector2(10f, -5f),
                null,
                Color.Black,
                -_wingAngle,
                new Vector2(wingWidth / 2, 0),
                new Vector2(wingWidth, wingLength),
                SpriteEffects.None,
                0f
            );
        }
        
        private void DrawBody(SpriteBatch spriteBatch)
        {
            // Тело (овал)
            DrawEllipse(spriteBatch, _position, _bodyWidth, _bodyHeight, Color.Black, 0f);
        }
        
        private void DrawHead(SpriteBatch spriteBatch)
        {
            float headSize = 10f;
            float headOffset = _bodyWidth * 0.4f;
            
            // Голова
            DrawCircle(spriteBatch, _position + new Vector2(headOffset, -5f), headSize, Color.Black);
            
            // Клюв
            float beakLength = 8f;
            Vector2 beakStart = _position + new Vector2(headOffset + headSize * 0.5f, -5f);
            Vector2 beakEnd = beakStart + new Vector2(beakLength, 0);
            DrawLine(spriteBatch, beakStart, beakEnd, new Color(255, 200, 50), 4f);
            
            // Глаз
            DrawCircle(spriteBatch, _position + new Vector2(headOffset + 3f, -7f), 2f, Color.White);
            DrawCircle(spriteBatch, _position + new Vector2(headOffset + 4f, -7f), 1f, Color.Black);
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            DrawEllipse(spriteBatch, center, radius * 2, radius * 2, color, 0f);
        }

        private void DrawEllipse(SpriteBatch spriteBatch, Vector2 center, float width, float height, Color color, float rotation)
        {
            int segments = 24;
            
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
