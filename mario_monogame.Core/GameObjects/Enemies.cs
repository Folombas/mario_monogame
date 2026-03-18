using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Гумба (гриб-враг) в стиле Mario.
    /// </summary>
    public class Goomba : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private Texture2D _pixelTexture;
        
        // Состояние
        private bool _isAlive;
        private float _squishTimer;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        
        // Размеры
        private readonly float _width;
        private readonly float _height;
        
        // Анимация
        private float _walkAnimationTime;
        private bool _facingRight;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsAlive => _isAlive;

        public Goomba(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;
            
            _moveSpeed = 50f;
            _gravity = 1500f;
            
            _width = 32f;
            _height = 32f;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _isAlive = true;
            _squishTimer = 0f;
            _walkAnimationTime = 0f;
            _facingRight = Random.Shared.Next(2) == 0;
        }

        public void Update(GameTime gameTime, float groundY, Player player)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (!_isAlive)
            {
                _squishTimer -= elapsed;
                return;
            }
            
            // Движение
            float moveDir = _facingRight ? 1f : -1f;
            _velocity.X = moveDir * _moveSpeed;
            
            // Гравитация
            _velocity.Y += _gravity * elapsed;
            
            // Применение
            _position += _velocity * elapsed;
            
            // Проверка земли
            if (_position.Y >= groundY)
            {
                _position.Y = groundY;
                _velocity.Y = 0f;
            }
            
            // Разворот на краях
            _walkAnimationTime += elapsed * 5f;
            
            // Проверка столкновения с игроком
            if (player.Bounds.Intersects(Bounds))
            {
                // Игрок прыгнул сверху - убиваем гумбу
                if (player.Velocity.Y > 0 && player.Position.Y < _position.Y - 10f)
                {
                    Squish();
                    player.ApplyGravity(-500f); // Отскок
                }
                // Иначе игрок получает урон (обрабатывается в Player)
            }
        }

        public void Squish()
        {
            _isAlive = false;
            _squishTimer = 0.3f;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = _position - cameraPosition;
            
            if (!_isAlive)
            {
                // Сплюснутая версия
                DrawSquished(spriteBatch, drawPos);
                return;
            }
            
            // Тело (гриб)
            DrawCap(spriteBatch, drawPos);
            
            // Ножка
            DrawStem(spriteBatch, drawPos);
            
            // Глаза
            DrawEyes(spriteBatch, drawPos);
            
            // Ноги
            DrawFeet(spriteBatch, drawPos);
        }
        
        private void DrawCap(SpriteBatch spriteBatch, Vector2 position)
        {
            float capWidth = _width * 0.9f;
            float capHeight = _height * 0.5f;
            
            // Основная часть шляпки
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X - capWidth / 2),
                    (int)(position.Y - _height / 2),
                    (int)capWidth,
                    (int)capHeight
                ),
                new Color(140, 80, 40) // Коричневый
            );
            
            // Пятна на шляпке
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X - 8),
                    (int)(position.Y - _height / 2 + 8),
                    6,
                    6
                ),
                new Color(100, 50, 30)
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X + 2),
                    (int)(position.Y - _height / 2 + 5),
                    5,
                    5
                ),
                new Color(100, 50, 30)
            );
        }
        
        private void DrawStem(SpriteBatch spriteBatch, Vector2 position)
        {
            float stemWidth = _width * 0.5f;
            float stemHeight = _height * 0.35f;
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X - stemWidth / 2),
                    (int)(position.Y - stemHeight / 2 + 5),
                    (int)stemWidth,
                    (int)stemHeight
                ),
                new Color((byte)220, (byte)180, (byte)120, (byte)255) // Бежевый
            );
        }
        
        private void DrawEyes(SpriteBatch spriteBatch, Vector2 position)
        {
            // Белки глаз
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X - 8),
                    (int)(position.Y - 5),
                    6,
                    7
                ),
                Color.White
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X + 2),
                    (int)(position.Y - 5),
                    6,
                    7
                ),
                Color.White
            );
            
            // Зрачки
            float pupilOffset = _facingRight ? 2f : -2f;
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X - 8 + pupilOffset),
                    (int)(position.Y - 3),
                    3,
                    4
                ),
                Color.Black
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X + 2 + pupilOffset),
                    (int)(position.Y - 3),
                    3,
                    4
                ),
                Color.Black
            );
        }
        
        private void DrawFeet(SpriteBatch spriteBatch, Vector2 position)
        {
            float footWidth = _width * 0.35f;
            float footHeight = _height * 0.2f;
            float footOffset = (float)Math.Sin(_walkAnimationTime) * 3f;
            
            // Левая нога
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X - 10 - footOffset),
                    (int)(position.Y + _height / 2 - footHeight),
                    (int)footWidth,
                    (int)footHeight
                ),
                Color.Black
            );
            
            // Правая нога
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X + 10 + footOffset),
                    (int)(position.Y + _height / 2 - footHeight),
                    (int)footWidth,
                    (int)footHeight
                ),
                Color.Black
            );
        }
        
        private void DrawSquished(SpriteBatch spriteBatch, Vector2 position)
        {
            float alpha = _squishTimer / 0.3f;
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(position.X - _width / 2),
                    (int)(position.Y + _height / 2 - 8),
                    (int)_width,
                    8
                ),
                new Color((byte)140, (byte)80, (byte)40, (byte)(alpha * 255))
            );
        }
        
        public void ReverseDirection()
        {
            _facingRight = !_facingRight;
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }
    
    /// <summary>
    /// Ядовитый гриб (бонус).
    /// </summary>
    public class PoisonMushroom : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private Texture2D _pixelTexture;
        
        private bool _isCollected;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        private float _bounceTime;
        
        private readonly float _width;
        private readonly float _height;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsCollected => _isCollected;

        public PoisonMushroom(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;
            
            _moveSpeed = 80f;
            _gravity = 1200f;
            
            _width = 30f;
            _height = 30f;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _isCollected = false;
            _bounceTime = 0f;
        }

        public void Update(GameTime gameTime, float groundY)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_isCollected) return;
            
            // Движение
            _velocity.X = (float)Math.Sin(_bounceTime) * _moveSpeed;
            
            // Гравитация
            _velocity.Y += _gravity * elapsed;
            
            // Применение
            _position += _velocity * elapsed;
            
            // Проверка земли
            if (_position.Y >= groundY)
            {
                _position.Y = groundY;
                _velocity.Y = 0f;
            }
            
            _bounceTime += elapsed * 3f;
        }

        public void Collect()
        {
            _isCollected = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isCollected) return;
            
            Vector2 drawPos = _position - cameraPosition;
            
            // Шляпка (ядовитый зелёный цвет)
            float capWidth = _width * 0.9f;
            float capHeight = _height * 0.55f;
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - capWidth / 2),
                    (int)(drawPos.Y - _height / 2),
                    (int)capWidth,
                    (int)capHeight
                ),
                new Color(50, 180, 50) // Ядовито-зелёный
            );
            
            // Пятна (белые точки)
            for (int i = 0; i < 5; i++)
            {
                float px = drawPos.X - 10 + (i % 3) * 10;
                float py = drawPos.Y - _height / 2 + 5 + (i / 3) * 8f;
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(
                        (int)px,
                        (int)py,
                        4,
                        4
                    ),
                    Color.White
                );
            }
            
            // Ножка
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - _width * 0.25f),
                    (int)(drawPos.Y),
                    (int)(_width * 0.5f),
                    (int)(_height * 0.45f)
                ),
                new Color(240, 230, 200)
            );
            
            // Злые глаза
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - 8),
                    (int)(drawPos.Y - 2),
                    6,
                    6
                ),
                Color.White
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 2),
                    (int)(drawPos.Y - 2),
                    6,
                    6
                ),
                Color.White
            );
            
            // Зрачки
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - 6),
                    (int)(drawPos.Y),
                    3,
                    3
                ),
                Color.Red
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 4),
                    (int)(drawPos.Y),
                    3,
                    3
                ),
                Color.Red
            );
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }
}
