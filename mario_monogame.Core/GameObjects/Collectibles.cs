using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Монета в стиле платформера с использованием спрайтов.
    /// </summary>
    public class Coin : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Texture2D _sprite;

        private bool _isCollected;
        private float _spinAngle;
        private float _bobOffset;
        private readonly float _bobSpeed;
        private readonly float _width;
        private readonly float _height;
        private readonly float _scale;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsCollected => _isCollected;

        public Coin(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string type = "Gold")
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;

            _scale = 0.6f;
            _sprite = type switch
            {
                "Bronze" => content.Load<Texture2D>("Sprites/Items/coinBronze"),
                "Silver" => content.Load<Texture2D>("Sprites/Items/coinSilver"),
                _ => content.Load<Texture2D>("Sprites/Items/coinGold")
            };

            _width = _sprite.Width * _scale;
            _height = _sprite.Height * _scale;

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

            Vector2 origin = new Vector2(_sprite.Width / 2f, _sprite.Height / 2f);

            if (scaleX < 0.2f)
            {
                // Почти не видно - рисуем как линию
                spriteBatch.Draw(
                    _sprite,
                    drawPos,
                    null,
                    new Color(1f, 1f, 1f, scaleX),
                    0f,
                    origin,
                    _scale,
                    SpriteEffects.FlipHorizontally,
                    0f
                );
            }
            else
            {
                spriteBatch.Draw(
                    _sprite,
                    drawPos,
                    null,
                    Color.White,
                    0f,
                    origin,
                    new Vector2(scaleX * _scale, _scale),
                    SpriteEffects.None,
                    0f
                );
            }
        }

        public void Dispose()
        {
            // Текстура освобождается ContentManager
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
        private Texture2D _sprite;

        private bool _isCollected;
        private bool _isEmerging;
        private float _emergeProgress;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        private bool _facingRight;
        private readonly float _width;
        private readonly float _height;
        private readonly float _scale;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsCollected => _isCollected;

        public PowerMushroom(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string type = "Red")
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;

            _scale = 0.5f;
            _sprite = type switch
            {
                "Brown" => content.Load<Texture2D>("Sprites/Tiles/mushroomBrown"),
                _ => content.Load<Texture2D>("Sprites/Tiles/mushroomRed")
            };

            _width = _sprite.Width * _scale;
            _height = _sprite.Height * _scale;

            _moveSpeed = 100f;
            _gravity = 1500f;

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
                    _sprite,
                    drawPos,
                    null,
                    new Color(1f, 1f, 1f, _emergeProgress),
                    0f,
                    new Vector2(_sprite.Width / 2f, _sprite.Height / 2f),
                    new Vector2(_scale, _scale * _emergeProgress),
                    SpriteEffects.None,
                    0f
                );
                return;
            }

            Vector2 origin = new Vector2(_sprite.Width / 2f, _sprite.Height / 2f);
            SpriteEffects effects = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(
                _sprite,
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

        public void Dispose()
        {
            // Текстура освобождается ContentManager
        }
    }

    /// <summary>
    /// Ядовитый гриб (бонус-ловушка).
    /// </summary>
    public class PoisonMushroom : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private Texture2D _sprite;

        private bool _isCollected;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        private float _bounceTime;
        private readonly float _width;
        private readonly float _height;
        private readonly float _scale;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsCollected => _isCollected;

        public PoisonMushroom(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;

            _scale = 0.5f;
            _sprite = content.Load<Texture2D>("Sprites/Tiles/mushroomBrown"); // Коричневый = ядовитый

            _width = _sprite.Width * _scale;
            _height = _sprite.Height * _scale;

            _moveSpeed = 80f;
            _gravity = 1200f;

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
            Vector2 origin = new Vector2(_sprite.Width / 2f, _sprite.Height / 2f);
            SpriteEffects effects = (int)_bounceTime % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(
                _sprite,
                drawPos,
                null,
                new Color(0.8f, 1f, 0.8f, 1f), // Зеленоватый оттенок
                0f,
                origin,
                _scale,
                effects,
                0f
            );
        }

        public void Dispose()
        {
            // Текстура освобождается ContentManager
        }
    }

    /// <summary>
    /// Звезда-бонус (неуязвимость).
    /// </summary>
    public class StarPower : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Vector2 _velocity;
        private Texture2D _sprite;

        private bool _isCollected;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        private float _glowTimer;
        private readonly float _width;
        private readonly float _height;
        private readonly float _scale;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsCollected => _isCollected;

        public StarPower(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _velocity = Vector2.Zero;

            _scale = 0.5f;
            _sprite = content.Load<Texture2D>("Sprites/Items/star");

            _width = _sprite.Width * _scale;
            _height = _sprite.Height * _scale;

            _moveSpeed = 120f;
            _gravity = 1000f;
            _isCollected = false;
            _glowTimer = 0f;
        }

        public void Update(GameTime gameTime, float groundY)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isCollected) return;

            // Движение с подпрыгиванием
            _velocity.X = _moveSpeed;
            _velocity.Y += _gravity * elapsed;

            _position += _velocity * elapsed;

            if (_position.Y >= groundY)
            {
                _position.Y = groundY;
                _velocity.Y = -400f; // Прыжок
            }

            _glowTimer += elapsed * 10f;
        }

        public void Collect()
        {
            _isCollected = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isCollected) return;

            Vector2 drawPos = _position - cameraPosition;
            Vector2 origin = new Vector2(_sprite.Width / 2f, _sprite.Height / 2f);

            // Мерцание
            float alpha = 0.7f + (float)Math.Sin(_glowTimer) * 0.3f;

            spriteBatch.Draw(
                _sprite,
                drawPos,
                null,
                new Color(1f, 1f, 1f, alpha),
                0f,
                origin,
                _scale,
                SpriteEffects.None,
                0f
            );
        }

        public void Dispose()
        {
            // Текстура освобождается ContentManager
        }
    }

    /// <summary>
    /// Драгоценный камень (бонусные очки).
    /// </summary>
    public class Gem : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Texture2D _sprite;

        private bool _isCollected;
        private float _sparkleTimer;
        private readonly float _width;
        private readonly float _height;
        private readonly float _scale;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsCollected => _isCollected;

        public Gem(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string color = "Red")
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;

            _scale = 0.5f;
            _sprite = color switch
            {
                "Blue" => content.Load<Texture2D>("Sprites/Items/gemBlue"),
                "Green" => content.Load<Texture2D>("Sprites/Items/gemGreen"),
                "Yellow" => content.Load<Texture2D>("Sprites/Items/gemYellow"),
                _ => content.Load<Texture2D>("Sprites/Items/gemRed")
            };

            _width = _sprite.Width * _scale;
            _height = _sprite.Height * _scale;

            _isCollected = false;
            _sparkleTimer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _sparkleTimer += elapsed * 8f;
        }

        public void Collect()
        {
            _isCollected = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isCollected) return;

            Vector2 drawPos = _position - cameraPosition;
            Vector2 origin = new Vector2(_sprite.Width / 2f, _sprite.Height / 2f);

            // Мерцание
            float alpha = 0.8f + (float)Math.Sin(_sparkleTimer) * 0.2f;

            spriteBatch.Draw(
                _sprite,
                drawPos,
                null,
                new Color(1f, 1f, 1f, alpha),
                0f,
                origin,
                _scale,
                SpriteEffects.None,
                0f
            );
        }

        public void Dispose()
        {
            // Текстура освобождается ContentManager
        }
    }

    /// <summary>
    /// Ключ (открывает двери/уровни).
    /// </summary>
    public class Key : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Texture2D _sprite;

        private bool _isCollected;
        private float _floatOffset;
        private readonly float _width;
        private readonly float _height;
        private readonly float _scale;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsCollected => _isCollected;

        public Key(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string color = "Red")
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;

            _scale = 0.5f;
            _sprite = color switch
            {
                "Blue" => content.Load<Texture2D>("Sprites/Items/keyBlue"),
                "Green" => content.Load<Texture2D>("Sprites/Items/keyGreen"),
                "Yellow" => content.Load<Texture2D>("Sprites/Items/keyYellow"),
                _ => content.Load<Texture2D>("Sprites/Items/keyRed")
            };

            _width = _sprite.Width * _scale;
            _height = _sprite.Height * _scale;

            _isCollected = false;
            _floatOffset = 0f;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _floatOffset = (float)Math.Sin(elapsed * 5f) * 5f;
        }

        public void Collect()
        {
            _isCollected = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isCollected) return;

            Vector2 drawPos = _position - cameraPosition + new Vector2(0, _floatOffset);
            Vector2 origin = new Vector2(_sprite.Width / 2f, _sprite.Height / 2f);

            spriteBatch.Draw(
                _sprite,
                drawPos,
                null,
                Color.White,
                0f,
                origin,
                _scale,
                SpriteEffects.None,
                0f
            );
        }

        public void Dispose()
        {
            // Текстура освобождается ContentManager
        }
    }

    /// <summary>
    /// Флаг финиша уровня.
    /// </summary>
    public class Flag : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private Texture2D _flag1Sprite;
        private Texture2D _flag2Sprite;
        private Texture2D _poleSprite;

        private bool _isReached;
        private float _waveTimer;
        private readonly float _width;
        private readonly float _height;
        private readonly float _scale;

        public Vector2 Position => _position;
        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _width / 2),
            (int)(_position.Y - _height / 2),
            (int)_width,
            (int)_height
        );
        public bool IsReached => _isReached;

        public Flag(GraphicsDevice graphicsDevice, Vector2 startPosition, ContentManager content, string color = "Red")
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;

            _scale = 0.6f;

            _flag1Sprite = content.Load<Texture2D>($"Sprites/Items/flag{color}1");
            _flag2Sprite = content.Load<Texture2D>($"Sprites/Items/flag{color}2");
            _poleSprite = content.Load<Texture2D>($"Sprites/Items/flag{color}_down");

            _width = 64f * _scale;
            _height = 128f * _scale;

            _isReached = false;
            _waveTimer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _waveTimer += elapsed * 5f;
        }

        public void Reach()
        {
            _isReached = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = _position - cameraPosition;

            // Рисуем флагшток
            spriteBatch.Draw(
                _poleSprite,
                drawPos + new Vector2(0, 32),
                null,
                Color.White,
                0f,
                new Vector2(_poleSprite.Width / 2f, 0),
                new Vector2(_scale * 0.3f, _scale * 2f),
                SpriteEffects.None,
                0f
            );

            // Развевающийся флаг
            Texture2D currentFlag = (int)_waveTimer % 2 == 0 ? _flag1Sprite : _flag2Sprite;
            spriteBatch.Draw(
                currentFlag,
                drawPos,
                null,
                _isReached ? new Color(0.7f, 1f, 0.7f) : Color.White,
                0f,
                new Vector2(0, currentFlag.Height / 2f),
                _scale,
                SpriteEffects.None,
                0f
            );
        }

        public void Dispose()
        {
            // Текстуры освобождаются ContentManager
        }
    }
}
