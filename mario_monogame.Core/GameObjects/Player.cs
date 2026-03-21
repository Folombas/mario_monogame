using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Игрок в стиле платформера с использованием спрайтов.
    /// </summary>
    public class Player : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;

        // Спрайты
        private Texture2D _standSprite;
        private Texture2D _walk1Sprite;
        private Texture2D _walk2Sprite;
        private Texture2D _jumpSprite;
        private Texture2D _hitSprite;
        private Texture2D _climb1Sprite;
        private Texture2D _climb2Sprite;
        private Texture2D _duckSprite;
        private Texture2D _frontSprite;
        private Texture2D _swim1Sprite;
        private Texture2D _swim2Sprite;

        // Физика
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
        private float _hitTimer;

        // Размеры спрайта
        private float _width;
        private float _height;
        private float _originalHeight;
        private float _scale;

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
        public bool IsFacingRight => _facingRight;

        public Player(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;

            // Загрузка спрайтов
            _standSprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_stand");
            _walk1Sprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_walk1");
            _walk2Sprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_walk2");
            _jumpSprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_jump");
            _hitSprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_hit");
            _climb1Sprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_climb1");
            _climb2Sprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_climb2");
            _duckSprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_duck");
            _frontSprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_front");
            _swim1Sprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_swim1");
            _swim2Sprite = content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_swim2");

            // Физика
            _gravity = 2500f;
            _maxFallSpeed = 800f;
            _moveSpeed = 400f;
            _acceleration = 2000f;
            _deceleration = 1500f;
            _maxSpeed = 350f;
            _jumpForce = 850f;
            _jumpCharge = 0f;

            // Размеры (масштабируем спрайт 128x256 до 64x128)
            _scale = 0.5f;
            _width = 128f * _scale;
            _height = 128f * _scale; // Уменьшенная высота для хитбокса
            _originalHeight = 128f * _scale;

            _isGrounded = false;
            _isJumping = false;
            _facingRight = true;
            _walkAnimationTime = 0f;
            _isWalking = false;
            _state = PlayerState.Small;
            _invincibilityTimer = 0f;
            _hitTimer = 0f;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, float groundY)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Сброс состояния
            _isWalking = false;

            // Таймер попадания
            if (_hitTimer > 0f)
            {
                _hitTimer -= elapsed;
            }

            // Управление
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
                _walkAnimationTime += elapsed * Math.Abs(_velocity.X) * 0.1f;
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
                _height = _originalHeight * 1.2f;
                _position.Y -= _originalHeight * 0.2f;
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
                _height = _originalHeight * 1.2f;
            }
            else if (_state == PlayerState.Big)
            {
                _state = PlayerState.Small;
                _height = _originalHeight;
                _position.Y += _originalHeight * 0.2f;
                _invincibilityTimer = 2f;
            }
        }

        public void Hit()
        {
            if (!IsInvincible && _hitTimer <= 0f)
            {
                _hitTimer = 0.3f;
                Shrink();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            // Мерцание если неуязвим
            if (IsInvincible && (int)(_invincibilityTimer * 10) % 2 == 0)
                return;

            Vector2 drawPos = _position - cameraPosition;
            Texture2D currentSprite = GetCurrentSprite();

            // Отражение по горизонтали если смотрим влево
            SpriteEffects effects = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Рисуем спрайт с центрированием
            Vector2 origin = new Vector2(currentSprite.Width / 2f, currentSprite.Height / 2f);
            
            spriteBatch.Draw(
                currentSprite,
                drawPos,
                null,
                Color.White,
                0f,
                origin,
                _scale,
                effects,
                0f
            );
        }

        private Texture2D GetCurrentSprite()
        {
            // При попадании показываем спрайт удара
            if (_hitTimer > 0f)
            {
                return _hitSprite;
            }

            // В прыжке
            if (!_isGrounded)
            {
                return _jumpSprite;
            }

            // Идём
            if (_isWalking)
            {
                int frame = (int)(_walkAnimationTime * 10) % 2;
                return frame == 0 ? _walk1Sprite : _walk2Sprite;
            }

            // Стоим
            return _standSprite;
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
            // Текстуры загружаются через ContentManager и освобождаются им
        }
    }

    public enum PlayerState
    {
        Small,
        Big,
        Fire
    }
}
