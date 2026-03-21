using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Тип платформы.
    /// </summary>
    public enum PlatformType
    {
        Ground,      // Земля
        Grass,       // Трава
        Dirt,        // Грязь
        Stone,       // Камень
        Brick,       // Кирпич
        Question,    // Блок с вопросом
        Crate,       // Ящик
        Pipe,        // Труба
        Lava,        // Лава
        Water,       // Вода
        Spikes,      // Шипы
        Ladder,      // Лестница
        Fence,       // Забор
        Bush,        // Куст
        Snow,        // Снег
        Sand,        // Песок
        Planet,      // Планета (космическая тема)
    }

    /// <summary>
    /// Платформа с использованием спрайтов.
    /// </summary>
    public class Platform : IDisposable
    {
        protected readonly GraphicsDevice _graphicsDevice;
        protected Rectangle _bounds;
        protected PlatformType _type;
        protected Texture2D _sprite;
        protected bool _isSolid;
        protected bool _isUsed;
        protected bool _isBroken;
        protected float _scale;

        public Rectangle Bounds => _bounds;
        public PlatformType Type => _type;
        public bool IsSolid => _isSolid;
        public bool IsUsed => _isUsed;
        public bool IsBroken => _isBroken;

        public Platform(GraphicsDevice graphicsDevice, Rectangle bounds, PlatformType type, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _bounds = bounds;
            _type = type;
            _scale = 1f;
            _isSolid = true;
            _isUsed = false;
            _isBroken = false;

            LoadSprite(content);
        }

        protected virtual void LoadSprite(ContentManager content)
        {
            switch (_type)
            {
                case PlatformType.Ground:
                case PlatformType.Grass:
                    _sprite = content.Load<Texture2D>("Sprites/Ground/Grass/grassMid");
                    break;

                case PlatformType.Dirt:
                    _sprite = content.Load<Texture2D>("Sprites/Ground/Dirt/dirtMid");
                    break;

                case PlatformType.Stone:
                    _sprite = content.Load<Texture2D>("Sprites/Ground/Stone/stoneMid");
                    break;

                case PlatformType.Brick:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/brickBrown");
                    break;

                case PlatformType.Question:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/boxCoin");
                    break;

                case PlatformType.Crate:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/boxCrate");
                    break;

                case PlatformType.Pipe:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/boxCrate_single");
                    break;

                case PlatformType.Lava:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/lava");
                    _isSolid = false;
                    break;

                case PlatformType.Water:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/water");
                    _isSolid = false;
                    break;

                case PlatformType.Spikes:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/spikes");
                    _isSolid = false;
                    break;

                case PlatformType.Ladder:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/ladderMid");
                    _isSolid = false;
                    break;

                case PlatformType.Fence:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/fence");
                    break;

                case PlatformType.Bush:
                    _sprite = content.Load<Texture2D>("Sprites/Tiles/bush");
                    _isSolid = false;
                    break;

                case PlatformType.Snow:
                    _sprite = content.Load<Texture2D>("Sprites/Ground/Snow/snowMid");
                    break;

                case PlatformType.Sand:
                    _sprite = content.Load<Texture2D>("Sprites/Ground/Sand/sandMid");
                    break;

                case PlatformType.Planet:
                    _sprite = content.Load<Texture2D>("Sprites/Ground/Planet/planetMid");
                    break;

                default:
                    _sprite = content.Load<Texture2D>("Sprites/Ground/Grass/grassMid");
                    break;
            }
        }

        public virtual void Use()
        {
            _isUsed = true;
        }

        public virtual void Break()
        {
            _isBroken = true;
        }

        public virtual void Update(GameTime gameTime)
        {
            // Базовая платформа не обновляется
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isBroken) return;

            // Рисуем платформу тайлами
            int tileWidth = _sprite.Width;
            int tileHeight = _sprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y);

            for (int x = startX; x < startX + _bounds.Width; x += tileWidth)
            {
                for (int y = startY; y < startY + _bounds.Height; y += tileHeight)
                {
                    spriteBatch.Draw(
                        _sprite,
                        new Rectangle(x, y, tileWidth, tileHeight),
                        Color.White
                    );
                }
            }
        }

        public void Dispose()
        {
            // Текстуры освобождаются ContentManager
        }
    }

    /// <summary>
    /// Платформа-земля с травой.
    /// </summary>
    public class GroundPlatform : Platform
    {
        private Texture2D _grassTopSprite;
        private Texture2D _grassLeftSprite;
        private Texture2D _grassRightSprite;
        private Texture2D _grassCenterSprite;
        private Texture2D _dirtSprite;

        public GroundPlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Ground, content)
        {
            LoadGroundSprites(content);
        }

        private void LoadGroundSprites(ContentManager content)
        {
            _grassTopSprite = content.Load<Texture2D>("Sprites/Ground/Grass/grass");
            _grassLeftSprite = content.Load<Texture2D>("Sprites/Ground/Grass/grassLeft");
            _grassRightSprite = content.Load<Texture2D>("Sprites/Ground/Grass/grassRight");
            _grassCenterSprite = content.Load<Texture2D>("Sprites/Ground/Grass/grassMid");
            _dirtSprite = content.Load<Texture2D>("Sprites/Ground/Dirt/dirtMid");
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            int tileWidth = _grassCenterSprite.Width;
            int tileHeight = _grassCenterSprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y);

            int tilesX = _bounds.Width / tileWidth + 1;
            int tilesY = _bounds.Height / tileHeight + 1;

            for (int y = 0; y < tilesY; y++)
            {
                for (int x = 0; x < tilesX; x++)
                {
                    Texture2D sprite;
                    int drawX = startX + x * tileWidth;
                    int drawY = startY + y * tileHeight;

                    if (y == 0)
                    {
                        // Верхний ряд - трава
                        if (x == 0)
                            sprite = _grassLeftSprite;
                        else if (x == tilesX - 1)
                            sprite = _grassRightSprite;
                        else
                            sprite = _grassTopSprite;
                    }
                    else
                    {
                        // Остальные ряды - земля
                        sprite = _dirtSprite;
                    }

                    spriteBatch.Draw(
                        sprite,
                        new Rectangle(drawX, drawY, tileWidth, tileHeight),
                        Color.White
                    );
                }
            }
        }
    }

    /// <summary>
    /// Платформа-кирпич.
    /// </summary>
    public class BrickPlatform : Platform
    {
        private float _shakeTimer;
        private Vector2 _shakeOffset;

        public BrickPlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Brick, content)
        {
            _shakeTimer = 0f;
            _shakeOffset = Vector2.Zero;
        }

        public override void Use()
        {
            _shakeTimer = 0.2f;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= elapsed;
                _shakeOffset = new Vector2(
                    (float)(Random.Shared.NextDouble() * 4 - 2),
                    (float)(Random.Shared.NextDouble() * 4 - 2)
                );
            }
            else
            {
                _shakeOffset = Vector2.Zero;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (_isBroken) return;

            int tileWidth = _sprite.Width;
            int tileHeight = _sprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X + _shakeOffset.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y + _shakeOffset.Y);

            spriteBatch.Draw(
                _sprite,
                new Rectangle(startX, startY, tileWidth, tileHeight),
                _isUsed ? new Color(0.7f, 0.7f, 0.7f) : Color.White
            );
        }
    }

    /// <summary>
    /// Блок с вопросом.
    /// </summary>
    public class QuestionPlatform : Platform
    {
        private Texture2D _usedSprite;
        private float _bounceOffset;
        private float _bounceTimer;

        public QuestionPlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Question, content)
        {
            _usedSprite = content.Load<Texture2D>("Sprites/Tiles/boxCoin_disabled");
            _bounceOffset = 0f;
            _bounceTimer = 0f;
        }

        public override void Use()
        {
            if (!_isUsed)
            {
                _isUsed = true;
                _bounceTimer = 0.15f;
            }
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_bounceTimer > 0f)
            {
                _bounceTimer -= elapsed;
                _bounceOffset = (float)Math.Sin(_bounceTimer * Math.PI * 10) * 10f;
            }
            else
            {
                _bounceOffset = 0f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            int tileWidth = _sprite.Width;
            int tileHeight = _sprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y + _bounceOffset);

            spriteBatch.Draw(
                _isUsed ? _usedSprite : _sprite,
                new Rectangle(startX, startY, tileWidth, tileHeight),
                Color.White
            );
        }
    }

    /// <summary>
    /// Труба.
    /// </summary>
    public class PipePlatform : Platform
    {
        private Texture2D _pipeTopSprite;
        private Texture2D _pipeBodySprite;

        public PipePlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Pipe, content)
        {
            _pipeTopSprite = content.Load<Texture2D>("Sprites/Tiles/boxCrate_single");
            _pipeBodySprite = content.Load<Texture2D>("Sprites/Tiles/boxCrate");
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            int tileWidth = _pipeBodySprite.Width;
            int tileHeight = _pipeBodySprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y);

            // Рисуем верх трубы
            spriteBatch.Draw(
                _pipeTopSprite,
                new Rectangle(startX - 5, startY, tileWidth + 10, tileHeight),
                Color.White
            );

            // Рисуем тело трубы
            for (int y = tileHeight; y < _bounds.Height; y += tileHeight)
            {
                spriteBatch.Draw(
                    _pipeBodySprite,
                    new Rectangle(startX, startY + y, tileWidth, tileHeight),
                    Color.White
                );
            }
        }
    }

    /// <summary>
    /// Шипы (опасная платформа).
    /// </summary>
    public class SpikesPlatform : Platform
    {
        private float _glowTimer;

        public SpikesPlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Spikes, content)
        {
            _glowTimer = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _glowTimer += elapsed * 5f;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            int tileWidth = _sprite.Width;
            int tileHeight = _sprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y);

            float alpha = 0.8f + (float)Math.Sin(_glowTimer) * 0.2f;

            spriteBatch.Draw(
                _sprite,
                new Rectangle(startX, startY, tileWidth, tileHeight),
                new Color(Color.White, alpha)
            );
        }
    }

    /// <summary>
    /// Лава (опасная платформа).
    /// </summary>
    public class LavaPlatform : Platform
    {
        private float _bubbleTimer;
        private int _currentFrame;

        public LavaPlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Lava, content)
        {
            _bubbleTimer = 0f;
            _currentFrame = 0;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _bubbleTimer += elapsed;

            if (_bubbleTimer > 0.2f)
            {
                _bubbleTimer = 0f;
                _currentFrame = (_currentFrame + 1) % 3;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            int tileWidth = _sprite.Width;
            int tileHeight = _sprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y);

            // Анимация лавы
            Color lavaColor = _currentFrame switch
            {
                0 => new Color(255, 100, 0),
                1 => new Color(255, 150, 50),
                _ => new Color(255, 80, 0)
            };

            spriteBatch.Draw(
                _sprite,
                new Rectangle(startX, startY, tileWidth, tileHeight),
                lavaColor
            );
        }
    }

    /// <summary>
    /// Вода.
    /// </summary>
    public class WaterPlatform : Platform
    {
        private float _waveTimer;

        public WaterPlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Water, content)
        {
            _waveTimer = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _waveTimer += elapsed * 3f;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            int tileWidth = _sprite.Width;
            int tileHeight = _sprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y);

            float waveOffset = (float)Math.Sin(_waveTimer) * 3f;

            spriteBatch.Draw(
                _sprite,
                new Rectangle(startX, (int)(startY + waveOffset), tileWidth, tileHeight),
                new Color(100, 150, 255, 180)
            );
        }
    }

    /// <summary>
    /// Декоративный куст.
    /// </summary>
    public class BushPlatform : Platform
    {
        public BushPlatform(GraphicsDevice graphicsDevice, Rectangle bounds, ContentManager content)
            : base(graphicsDevice, bounds, PlatformType.Bush, content)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            int tileWidth = _sprite.Width;
            int tileHeight = _sprite.Height;

            int startX = (int)(_bounds.X - cameraPosition.X);
            int startY = (int)(_bounds.Y - cameraPosition.Y);

            spriteBatch.Draw(
                _sprite,
                new Rectangle(startX, startY, tileWidth * 2, tileHeight),
                Color.White
            );
        }
    }
}
