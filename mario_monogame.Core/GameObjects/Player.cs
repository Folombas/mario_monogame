using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Игрок в стиле Mario с классической физикой платформера.
    /// </summary>
    public class Player : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private Texture2D _pixelTexture;
        
        // Физика Mario
        private bool _isGrounded;
        private bool _isJumping;
        private float _jumpCharge;
        private readonly float _gravity;
        private readonly float _maxFallSpeed;
        private readonly float _moveSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly float _maxSpeed;
        private readonly float _jumpForce;
        
        // Состояние
        private PlayerState _state;
        private bool _facingRight;
        private float _walkAnimationTime;
        private bool _isWalking;
        private float _invincibilityTimer;
        
        // Размеры
        private float _width;
        private float _height;
        private float _originalHeight;
        
        // Цвета
        private readonly Color _hatColor;
        private readonly Color _shirtColor;
        private readonly Color _overallsColor;
        private readonly Color _skinColor;
        private readonly Color _shoeColor;

        public Vector2 Position { get => _position; set => _position = value; }
        public Vector2 Velocity { get => _velocity; set => _velocity = value; }
        public bool IsGrounded => _isGrounded;
        public PlayerState State => _state;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsInvincible => _invincibilityTimer > 0;
        public bool IsBig => _state == PlayerState.Big || _state == PlayerState.Fire;

        public Player(GraphicsDevice graphicsDevice, Vector2 startPosition)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;
            
            // Физика в стиле Mario
            _gravity = 2500f;
            _maxFallSpeed = 800f;
            _moveSpeed = 400f;
            _acceleration = 2000f;
            _deceleration = 1500f;
            _maxSpeed = 350f;
            _jumpForce = 850f;
            _jumpCharge = 0f;
            
            // Размеры (маленький Марио)
            _width = 32f;
            _height = 32f;
            _originalHeight = 32f;
            
            // Цвета (классический Марио)
            _hatColor = new Color(230, 0, 0);      // Красная кепка
            _shirtColor = new Color(230, 0, 0);     // Красная рубашка
            _overallsColor = new Color(30, 30, 200); // Синий комбинезон
            _skinColor = new Color(255, 200, 150);   // Телесный
            _shoeColor = new Color(100, 60, 30);     // Коричневые ботинки
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _isGrounded = false;
            _isJumping = false;
            _facingRight = true;
            _walkAnimationTime = 0f;
            _isWalking = false;
            _state = PlayerState.Small;
            _invincibilityTimer = 0f;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, float groundY)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Сброс состояния
            _isWalking = false;
            
            // Управление (классическое Mario)
            float moveInput = 0f;
            
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            {
                moveInput = -1f;
                _facingRight = false;
                _isWalking = true;
            }
            else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            {
                moveInput = 1f;
                _facingRight = true;
                _isWalking = true;
            }
            
            // Ускорение/замедление
            if (moveInput != 0f)
            {
                _velocity.X += moveInput * _acceleration * elapsed;
                _velocity.X = MathHelper.Clamp(_velocity.X, -_maxSpeed, _maxSpeed);
            }
            else
            {
                // Торможение
                if (Math.Abs(_velocity.X) > 10f)
                {
                    _velocity.X -= Math.Sign(_velocity.X) * _deceleration * elapsed;
                }
                else
                {
                    _velocity.X = 0f;
                }
            }
            
            // Трение о землю
            if (_isGrounded)
            {
                _velocity.X *= 0.85f;
            }
            
            // Прыжок (с переменной высотой)
            if (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
            {
                if (_isGrounded && !_isJumping)
                {
                    _isJumping = true;
                    _isGrounded = false;
                    _velocity.Y = -_jumpForce;
                    _jumpCharge = 0f;
                }
                else if (!_isGrounded && _jumpCharge < 0.15f)
                {
                    // Дополнительный импульс при удержании (переменная высота прыжка)
                    _jumpCharge += elapsed;
                    _velocity.Y = Math.Max(_velocity.Y, -_jumpForce * 0.5f);
                }
            }
            else
            {
                // Отпускание кнопки обрезает прыжок
                if (_isJumping && _velocity.Y < -_jumpForce * 0.3f)
                {
                    _velocity.Y *= 0.5f;
                }
                _isJumping = false;
            }
            
            // Гравитация
            _velocity.Y += _gravity * elapsed;
            _velocity.Y = Math.Min(_velocity.Y, _maxFallSpeed);
            
            // Применение скорости
            _position += _velocity * elapsed;
            
            // Проверка земли
            if (_position.Y >= groundY)
            {
                _position.Y = groundY;
                _isGrounded = true;
                _isJumping = false;
                _velocity.Y = 0f;
            }
            
            // Анимация ходьбы
            if (_isWalking && _isGrounded)
            {
                _walkAnimationTime += elapsed * Math.Abs(_velocity.X) * 0.05f;
            }
            else
            {
                _walkAnimationTime = 0f;
            }
            
            // Таймер неуязвимости
            if (_invincibilityTimer > 0f)
            {
                _invincibilityTimer -= elapsed;
            }
        }
        
        public void Grow()
        {
            if (_state == PlayerState.Small)
            {
                _state = PlayerState.Big;
                _height = _originalHeight * 2f;
                _position.Y -= _originalHeight;
            }
            else if (_state == PlayerState.Big)
            {
                _state = PlayerState.Fire;
            }
        }
        
        public void Shrink()
        {
            if (_state == PlayerState.Fire)
            {
                _state = PlayerState.Big;
                _height = _originalHeight * 2f;
            }
            else if (_state == PlayerState.Big)
            {
                _state = PlayerState.Small;
                _height = _originalHeight;
                _position.Y += _originalHeight;
                _invincibilityTimer = 2f; // 2 секунды неуязвимости
            }
        }
        
        public void Hit()
        {
            if (!IsInvincible)
            {
                Shrink();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            // Мерцание если неуязвим
            if (IsInvincible && (int)(_invincibilityTimer * 10) % 2 == 0)
                return;
            
            float legOffset = (float)Math.Sin(_walkAnimationTime) * 4f;
            
            // Рисуем Марио
            DrawHead(spriteBatch, cameraPosition);
            DrawBody(spriteBatch, cameraPosition, legOffset);
            DrawLegs(spriteBatch, cameraPosition, legOffset);
        }
        
        private void DrawHead(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = _position - cameraPosition;
            float headWidth = _width * 0.9f;
            float headHeight = _height * 0.35f;
            
            // Кепка
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - headWidth / 2),
                    (int)(drawPos.Y - _height / 2 + 2),
                    (int)headWidth,
                    (int)(headHeight * 0.4f)
                ),
                _hatColor
            );
            
            // Поля кепки
            float brimX = _facingRight ? drawPos.X + headWidth * 0.1f : drawPos.X - headWidth * 0.5f;
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)brimX,
                    (int)(drawPos.Y - _height / 2 + headHeight * 0.25f),
                    (int)(headWidth * 0.4f),
                    (int)(headHeight * 0.2f)
                ),
                _hatColor
            );
            
            // Лицо
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - headWidth / 2 + 4),
                    (int)(drawPos.Y - _height / 2 + headHeight * 0.35f),
                    (int)(headWidth - 8),
                    (int)(headHeight * 0.65f)
                ),
                _skinColor
            );
            
            // Усы
            float mustacheX = _facingRight ? drawPos.X + 2f : drawPos.X - 2f;
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(mustacheX - 6),
                    (int)(drawPos.Y - _height / 2 + headHeight * 0.7f),
                    12,
                    4
                ),
                Color.Black
            );
            
            // Нос
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(mustacheX - 2),
                    (int)(drawPos.Y - _height / 2 + headHeight * 0.55f),
                    6,
                    5
                ),
                _skinColor
            );
        }
        
        private void DrawBody(SpriteBatch spriteBatch, Vector2 cameraPosition, float legOffset)
        {
            Vector2 drawPos = _position - cameraPosition;
            float bodyWidth = _width * 0.85f;
            float bodyHeight = _height * 0.35f;
            float bodyY = drawPos.Y - _height * 0.1f;
            
            // Рубашка
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - bodyWidth / 2),
                    (int)(bodyY - bodyHeight / 2),
                    (int)bodyWidth,
                    (int)bodyHeight
                ),
                _shirtColor
            );
            
            // Комбинезон
            float overallsHeight = bodyHeight * 0.6f;
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - bodyWidth / 2),
                    (int)(bodyY + bodyHeight * 0.2f),
                    (int)bodyWidth,
                    (int)overallsHeight
                ),
                _overallsColor
            );
            
            // Пуговицы
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - 4),
                    (int)(bodyY - 2),
                    3,
                    3
                ),
                Color.Gold
            );
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 1),
                    (int)(bodyY - 2),
                    3,
                    3
                ),
                Color.Gold
            );
        }
        
        private void DrawLegs(SpriteBatch spriteBatch, Vector2 cameraPosition, float legOffset)
        {
            Vector2 drawPos = _position - cameraPosition;
            float legWidth = _width * 0.35f;
            float legHeight = _height * 0.25f;
            float legY = drawPos.Y + _height * 0.35f;
            
            // Левая нога
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - legWidth - 2),
                    (int)(legY + legOffset),
                    (int)legWidth,
                    (int)legHeight
                ),
                _overallsColor
            );
            
            // Правая нога
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 2),
                    (int)(legY - legOffset),
                    (int)legWidth,
                    (int)legHeight
                ),
                _overallsColor
            );
            
            // Ботинки
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X - legWidth - 3),
                    (int)(legY + legHeight + legOffset),
                    (int)(legWidth + 2),
                    6
                ),
                _shoeColor
            );
            
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    (int)(drawPos.X + 1),
                    (int)(legY + legHeight - legOffset),
                    (int)(legWidth + 2),
                    6
                ),
                _shoeColor
            );
        }
        
        public void ApplyGravity(float amount)
        {
            _velocity.Y += amount;
        }
        
        public void SetInvincible(float duration)
        {
            _invincibilityTimer = duration;
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }
    
    public enum PlayerState
    {
        Small,
        Big,
        Fire
    }
}
