using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Базовый класс для врагов с использованием спрайтов.
    /// </summary>
    public class Enemy : IDisposable
    {
        protected readonly GraphicsDevice _graphicsDevice;
        protected Vector2 _position;
        protected Vector2 _velocity;

        // Спрайты
        protected Texture2D _sprite1;
        protected Texture2D _sprite2;
        protected Texture2D _deadSprite;

        // Состояние
        protected bool _isAlive;
        protected float _squishTimer;
        protected float _moveSpeed;
        protected float _gravity;
        protected bool _facingRight;

        // Размеры
        protected float _width;
        protected float _height;
        protected float _scale;

        // Анимация
        protected float _walkAnimationTime;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsAlive => _isAlive;

        public Enemy(GraphicsDevice graphicsDevice, Vector2 startPosition, float scale = 0.5f)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;
            _scale = scale;
            _moveSpeed = 50f;
            _gravity = 1500f;
            _isAlive = true;
            _squishTimer = 0f;
            _walkAnimationTime = 0f;
            _facingRight = Random.Shared.Next(2) == 0;
        }

        public virtual void Update(GameTime gameTime, float groundY, Player player)
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

            // Анимация
            _walkAnimationTime += elapsed * 5f;
        }

        public virtual void Squish()
        {
            _isAlive = false;
            _squishTimer = 0.3f;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = _position - cameraPosition;

            if (!_isAlive)
            {
                DrawSquished(spriteBatch, drawPos);
                return;
            }

            Texture2D currentSprite = GetCurrentSprite();
            SpriteEffects effects = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
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

        protected virtual Texture2D GetCurrentSprite()
        {
            int frame = (int)(_walkAnimationTime * 10) % 2;
            return frame == 0 ? _sprite1 : _sprite2;
        }

        protected virtual void DrawSquished(SpriteBatch spriteBatch, Vector2 position)
        {
            float alpha = _squishTimer / 0.3f;
            Texture2D deadSprite = _deadSprite ?? _sprite1;
            
            spriteBatch.Draw(
                deadSprite,
                position,
                null,
                new Color(Color.White, alpha),
                0f,
                new Vector2(deadSprite.Width / 2f, deadSprite.Height / 2f),
                _scale * 0.5f,
                SpriteEffects.None,
                0f
            );
        }

        public void ReverseDirection()
        {
            _facingRight = !_facingRight;
        }

        public virtual void Dispose()
        {
            // Текстуры освобождаются ContentManager
        }
    }

    /// <summary>
    /// Гумба (гриб-враг) в стиле Mario.
    /// </summary>
    public class Goomba : Enemy
    {
        public Goomba(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
            : base(graphicsDevice, startPosition, 0.5f)
        {
            // Загружаем спрайты слизня (похож на гумбу)
            _sprite1 = content.Load<Texture2D>("Sprites/Enemies/slimeGreen");
            _sprite2 = content.Load<Texture2D>("Sprites/Enemies/slimeGreen_move");
            _deadSprite = content.Load<Texture2D>("Sprites/Enemies/slimeGreen_dead");

            _width = 64f * _scale;
            _height = 32f * _scale;
            _moveSpeed = 60f;
        }

        public override void Update(GameTime gameTime, float groundY, Player player)
        {
            base.Update(gameTime, groundY, player);

            // Разворот при столкновении с игроком
            if (player.Bounds.Intersects(Bounds) && _isAlive)
            {
                if (player.Velocity.Y > 0 && player.Position.Y < _position.Y - 10f)
                {
                    Squish();
                    player.ApplyGravity(-500f);
                }
            }
        }
    }

    /// <summary>
    /// Пила-враг.
    /// </summary>
    public class SawEnemy : Enemy
    {
        public SawEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
            : base(graphicsDevice, startPosition, 0.6f)
        {
            _sprite1 = content.Load<Texture2D>("Sprites/Enemies/saw");
            _sprite2 = content.Load<Texture2D>("Sprites/Enemies/saw_move");
            _deadSprite = content.Load<Texture2D>("Sprites/Enemies/saw_dead");

            _width = 64f * _scale;
            _height = 64f * _scale;
            _moveSpeed = 80f;
            _gravity = 0f; // Пилы могут летать
        }

        public override void Update(GameTime gameTime, float groundY, Player player)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isAlive)
            {
                _squishTimer -= elapsed;
                return;
            }

            // Движение по синусоиде
            _velocity.X = (float)Math.Sin(_walkAnimationTime) * _moveSpeed;
            _velocity.Y = (float)Math.Cos(_walkAnimationTime * 2) * 30f;

            _position += _velocity * elapsed;
            _walkAnimationTime += elapsed * 3f;
        }
    }

    /// <summary>
    /// Рыба-враг (плавает в воде или прыгает).
    /// </summary>
    public class FishEnemy : Enemy
    {
        private readonly bool _isAquatic;
        private float _swimOffset;

        public FishEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, bool isAquatic = false)
            : base(graphicsDevice, startPosition, 0.4f)
        {
            _isAquatic = isAquatic;

            _sprite1 = content.Load<Texture2D>("Sprites/Enemies/fishGreen");
            _sprite2 = content.Load<Texture2D>("Sprites/Enemies/fishGreen_move");
            _deadSprite = content.Load<Texture2D>("Sprites/Enemies/fishGreen_dead");

            _width = 64f * _scale;
            _height = 32f * _scale;
            _moveSpeed = 100f;
            _facingRight = true;
            _swimOffset = 0f;
        }

        public override void Update(GameTime gameTime, float groundY, Player player)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isAlive)
            {
                _squishTimer -= elapsed;
                return;
            }

            if (_isAquatic)
            {
                // Плавание в воде
                _velocity.X = _facingRight ? _moveSpeed : -_moveSpeed;
                _swimOffset += elapsed * 5f;
                _velocity.Y = (float)Math.Sin(_swimOffset) * 50f;
            }
            else
            {
                // Прыгающая рыба
                _velocity.X = _facingRight ? _moveSpeed : -_moveSpeed;
                
                if (_position.Y >= groundY)
                {
                    _position.Y = groundY;
                    _velocity.Y = -300f; // Прыжок
                }
                else
                {
                    _velocity.Y += _gravity * elapsed;
                }
            }

            _position += _velocity * elapsed;
            _walkAnimationTime += elapsed * 5f;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = _position - cameraPosition;

            if (!_isAlive)
            {
                DrawSquished(spriteBatch, drawPos);
                return;
            }

            Texture2D currentSprite = GetCurrentSprite();
            // Рыбы всегда смотрят вправо по спрайту
            SpriteEffects effects = _facingRight ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
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
    }

    /// <summary>
    /// Слизень-враг.
    /// </summary>
    public class SlimeEnemy : Enemy
    {
        private readonly Color _slimeColor;

        public SlimeEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string color = "Green")
            : base(graphicsDevice, startPosition, 0.5f)
        {
            _slimeColor = color switch
            {
                "Blue" => Color.Blue,
                "Purple" => Color.Purple,
                _ => Color.Green
            };

            _sprite1 = content.Load<Texture2D>($"Sprites/Enemies/slime{color}");
            _sprite2 = content.Load<Texture2D>($"Sprites/Enemies/slime{color}_move");
            _deadSprite = content.Load<Texture2D>($"Sprites/Enemies/slime{color}_dead");

            _width = 64f * _scale;
            _height = 40f * _scale;
            _moveSpeed = 40f;
        }

        public override void Update(GameTime gameTime, float groundY, Player player)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isAlive)
            {
                _squishTimer -= elapsed;
                return;
            }

            // Медленное движение с подпрыгиванием
            float moveDir = _facingRight ? 1f : -1f;
            _velocity.X = moveDir * _moveSpeed;
            _velocity.Y += _gravity * elapsed;

            if (_position.Y >= groundY)
            {
                _position.Y = groundY;
                _velocity.Y = 0f;
                
                // Подпрыгивание
                if (Random.Shared.Next(100) < 2)
                {
                    _velocity.Y = -200f;
                }
            }

            _position += _velocity * elapsed;
            _walkAnimationTime += elapsed * 3f;
        }
    }

    /// <summary>
    /// Улитка-враг (медленная, с панцирем).
    /// </summary>
    public class SnailEnemy : Enemy
    {
        public SnailEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
            : base(graphicsDevice, startPosition, 0.5f)
        {
            _sprite1 = content.Load<Texture2D>("Sprites/Enemies/snail");
            _sprite2 = content.Load<Texture2D>("Sprites/Enemies/snail_move");
            _deadSprite = content.Load<Texture2D>("Sprites/Enemies/snail_shell");

            _width = 64f * _scale;
            _height = 48f * _scale;
            _moveSpeed = 30f;
        }
    }

    /// <summary>
    /// Летающий враг (муха/пчела).
    /// </summary>
    public class FlyingEnemy : Enemy
    {
        private float _flyOffset;
        private readonly float _flyHeight;

        public FlyingEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string type = "bee")
            : base(graphicsDevice, startPosition, 0.4f)
        {
            _sprite1 = content.Load<Texture2D>($"Sprites/Enemies/{type}");
            _sprite2 = content.Load<Texture2D>($"Sprites/Enemies/{type}_move");
            _deadSprite = content.Load<Texture2D>($"Sprites/Enemies/{type}_dead");

            _width = 48f * _scale;
            _height = 48f * _scale;
            _moveSpeed = 70f;
            _gravity = 200f;
            _flyOffset = 0f;
            _flyHeight = 50f;
        }

        public override void Update(GameTime gameTime, float groundY, Player player)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isAlive)
            {
                _squishTimer -= elapsed;
                return;
            }

            // Полёт по синусоиде
            float moveDir = _facingRight ? 1f : -1f;
            _velocity.X = moveDir * _moveSpeed;
            
            _flyOffset += elapsed * 8f;
            _velocity.Y = (float)Math.Sin(_flyOffset) * 60f;

            _position += _velocity * elapsed;
            _walkAnimationTime += elapsed * 10f;

            // Ограничение высоты
            if (_position.Y < groundY - _flyHeight)
            {
                _position.Y = groundY - _flyHeight;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = _position - cameraPosition;

            if (!_isAlive)
            {
                DrawSquished(spriteBatch, drawPos);
                return;
            }

            Texture2D currentSprite = GetCurrentSprite();
            SpriteEffects effects = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new Vector2(currentSprite.Width / 2f, currentSprite.Height / 2f);

            // Быстрая анимация крыльев
            int wingFrame = (int)(_walkAnimationTime * 20) % 2;
            Texture2D spriteToDraw = wingFrame == 0 ? _sprite1 : _sprite2;

            spriteBatch.Draw(
                spriteToDraw,
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
    }

    /// <summary>
    /// Мышь-враг (быстрая, маленькая).
    /// </summary>
    public class MouseEnemy : Enemy
    {
        public MouseEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
            : base(graphicsDevice, startPosition, 0.4f)
        {
            _sprite1 = content.Load<Texture2D>("Sprites/Enemies/mouse");
            _sprite2 = content.Load<Texture2D>("Sprites/Enemies/mouse_move");
            _deadSprite = content.Load<Texture2D>("Sprites/Enemies/mouse_dead");

            _width = 48f * _scale;
            _height = 32f * _scale;
            _moveSpeed = 120f; // Быстрая
        }
    }

    /// <summary>
    /// Червяк-враг.
    /// </summary>
    public class WormEnemy : Enemy
    {
        public WormEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string color = "Green")
            : base(graphicsDevice, startPosition, 0.5f)
        {
            _sprite1 = content.Load<Texture2D>($"Sprites/Enemies/worm{color}");
            _sprite2 = content.Load<Texture2D>($"Sprites/Enemies/worm{color}_move");
            _deadSprite = content.Load<Texture2D>($"Sprites/Enemies/worm{color}_dead");

            _width = 64f * _scale;
            _height = 32f * _scale;
            _moveSpeed = 50f;
        }
    }

    /// <summary>
    /// Лягушка-враг (прыгает).
    /// </summary>
    public class FrogEnemy : Enemy
    {
        private float _jumpTimer;
        private bool _isJumping;

        public FrogEnemy(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
            : base(graphicsDevice, startPosition, 0.5f)
        {
            _sprite1 = content.Load<Texture2D>("Sprites/Enemies/frog");
            _sprite2 = content.Load<Texture2D>("Sprites/Enemies/frog_move");
            _deadSprite = content.Load<Texture2D>("Sprites/Enemies/frog_dead");

            _width = 64f * _scale;
            _height = 48f * _scale;
            _moveSpeed = 40f;
            _jumpTimer = 0f;
            _isJumping = false;
        }

        public override void Update(GameTime gameTime, float groundY, Player player)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isAlive)
            {
                _squishTimer -= elapsed;
                return;
            }

            _jumpTimer += elapsed;

            // Прыжок каждые 1-2 секунды
            if (_jumpTimer > 1.5f && _isGrounded)
            {
                _isJumping = true;
                _velocity.Y = -350f;
                float moveDir = _facingRight ? 1f : -1f;
                _velocity.X = moveDir * 150f;
                _jumpTimer = 0f;
            }
            else
            {
                _velocity.Y += _gravity * elapsed;
                _velocity.X = 0f;
            }

            if (_position.Y >= groundY)
            {
                _position.Y = groundY;
                _velocity.Y = 0f;
                _isJumping = false;
            }

            _position += _velocity * elapsed;
            _walkAnimationTime += elapsed * 5f;
        }

        private bool _isGrounded => _position.Y >= 0; // Упрощённо
    }
}
