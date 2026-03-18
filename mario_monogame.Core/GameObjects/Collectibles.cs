using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Монета в стиле Mario.
    /// </summary>
    public class Coin : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Texture2D _pixelTexture;
        
        private bool _isCollected;
        private float _spinAngle;
        private float _bobOffset;
        private readonly float _bobSpeed;
        
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

        public Coin(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            
            _width = 20f;
            _height = 24f;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _isCollected = false;
            _spinAngle = 0f;
            _bobOffset = 0f;
            _bobSpeed = 3f + (float)(Random.Shared.NextDouble() * 2f);
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_isCollected) return;
            
            // Анимация вращения
            _spinAngle += elapsed * 5f;
            
            // Покачивание
            _bobOffset = (float)Math.Sin(_bobSpeed * elapsed * 10f) * 3f;
        }

        public void Collect()
        {
            _isCollected = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isCollected) return;
            
            Vector2 drawPos = _position - cameraPosition + new Vector2(0, _bobOffset);
            
            // Сплющивание для эффекта 3D вращения
            float scaleX = Math.Abs((float)Math.Cos(_spinAngle));
            
            // Основная монета
            DrawCoin(spriteBatch, drawPos, scaleX);
        }
        
        private void DrawCoin(SpriteBatch spriteBatch, Vector2 position, float scaleX)
        {
            int coinWidth = (int)(_width * scaleX);
            int coinHeight = (int)_height;
            
            if (scaleX < 0.2f)
            {
                // Почти не видно - рисуем как линию
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(
                        (int)(position.X - 2),
                        (int)(position.Y - coinHeight / 2),
                        4,
                        coinHeight
                    ),
                    new Color(255, 215, 0) // Золотой
                );
            }
            else
            {
                // Рисуем овал монеты
                DrawEllipse(spriteBatch, position, coinWidth, coinHeight, new Color(255, 215, 0));
                
                // Блеск
                if (scaleX > 0.5f)
                {
                    spriteBatch.Draw(
                        _pixelTexture,
                        new Rectangle(
                            (int)(position.X - 3),
                            (int)(position.Y - 5),
                            6,
                            8
                        ),
                        new Color(255, 255, 200, 200)
                    );
                }
            }
        }
        
        private void DrawEllipse(SpriteBatch spriteBatch, Vector2 center, int width, int height, Color color)
        {
            if (width < 2) width = 2;
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(center.X - width / 2),
                    (int)(center.Y - height / 2),
                    width,
                    height
                ),
                color
            );
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }
    
    /// <summary>
    /// Бонусный гриб (увеличивает игрока).
    /// </summary>
    public class PowerMushroom : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private Texture2D _pixelTexture;
        
        private bool _isCollected;
        private bool _isEmerging;
        private float _emergeProgress;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        private bool _facingRight;
        
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

        public PowerMushroom(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;
            
            _moveSpeed = 100f;
            _gravity = 1500f;
            
            _width = 32f;
            _height = 32f;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _isCollected = false;
            _isEmerging = true;
            _emergeProgress = 0f;
            _facingRight = Random.Shared.Next(2) == 0;
        }

        public void Update(GameTime gameTime, float groundY)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_isCollected) return;
            
            if (_isEmerging)
            {
                _emergeProgress += elapsed * 2f;
                if (_emergeProgress >= 1f)
                {
                    _emergeProgress = 1f;
                    _isEmerging = false;
                }
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
        }

        public void Collect()
        {
            _isCollected = true;
        }

        public void ReverseDirection()
        {
            _facingRight = !_facingRight;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isCollected) return;
            
            Vector2 drawPos = _position - cameraPosition;
            
            if (_isEmerging)
            {
                // Анимация появления
                float emergeHeight = _height * _emergeProgress;
                drawPos.Y += (_height - emergeHeight) / 2f;
                
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(
                        (int)(drawPos.X - _width / 2),
                        (int)(drawPos.Y - emergeHeight / 2),
                        (int)_width,
                        (int)emergeHeight
                    ),
                    new Color((byte)230, (byte)0, (byte)0, (byte)(_emergeProgress * 255))
                );
                return;
            }
            
            // Шляпка (красная с белыми точками)
            float capWidth = _width * 0.95f;
            float capHeight = _height * 0.55f;
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - capWidth / 2),
                    (int)(drawPos.Y - _height / 2),
                    (int)capWidth,
                    (int)capHeight
                ),
                new Color(230, 0, 0) // Красный
            );
            
            // Белые точки
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - 10),
                    (int)(drawPos.Y - _height / 2 + 8),
                    6,
                    6
                ),
                Color.White
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 4),
                    (int)(drawPos.Y - _height / 2 + 5),
                    6,
                    6
                ),
                Color.White
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - 3),
                    (int)(drawPos.Y - _height / 2 + 12),
                    5,
                    5
                ),
                Color.White
            );
            
            // Ножка
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - _width * 0.25f),
                    (int)(drawPos.Y),
                    (int)(_width * 0.5f),
                    (int)(_height * 0.45f)
                ),
                new Color((byte)255, (byte)220, (byte)180, (byte)255) // Бежевый
            );
            
            // Глаза
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - 8),
                    (int)(drawPos.Y - 2),
                    6,
                    6
                ),
                Color.Black
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 2),
                    (int)(drawPos.Y - 2),
                    6,
                    6
                ),
                Color.Black
            );
            
            // Блик в глазах
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - 6),
                    (int)(drawPos.Y - 1),
                    2,
                    2
                ),
                Color.White
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 4),
                    (int)(drawPos.Y - 1),
                    2,
                    2
                ),
                Color.White
            );
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }
}
