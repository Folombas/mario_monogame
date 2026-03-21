using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core
{
    public class MarioPlatformerGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Игрок
        private Vector2 _playerPos;
        private Vector2 _playerVel;
        private Texture2D _playerSprite;
        private bool _playerGrounded;
        private bool _playerFacingRight = true;
        private float _playerAnimTimer;
        private Rectangle _playerRect;

        // Камера
        private Vector2 _cameraPos;

        // Платформы
        private List<Platform> _platforms;
        private Texture2D _grassTile;
        private Texture2D _dirtTile;
        private Texture2D _brickTile;

        // Враги
        private List<Enemy> _enemies;
        private Texture2D _goombaSprite;

        // Монеты
        private List<Coin> _coins;
        private Texture2D _coinSprite;

        // Фон
        private Texture2D _background;
        private Texture2D _pixelTexture;

        // Состояние
        private int _score;
        private int _coinsCollected;
        private int _lives = 3;
        private int _currentLevel = 1;
        private GameState _state = GameState.Menu;
        private bool _isTransitioning;
        private float _transitionTimer;
        private TransitionType _transitionType;

        // Ввод
        private KeyboardState _prevKeys;

        // Шрифт
        private SpriteFont _font;

        public MarioPlatformerGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Загружаем шрифт
            try { _font = Content.Load<SpriteFont>("Fonts/Hud"); } catch { }

            // Загружаем спрайты игрока
            try
            {
                _playerSprite = Content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_stand");
            }
            catch { _playerSprite = CreatePlaceholder(64, 64, Color.Lime); }

            // Загружаем тайлы
            try { _grassTile = Content.Load<Texture2D>("Sprites/Ground/Grass/grassMid"); }
            catch { _grassTile = CreatePlaceholder(48, 48, Color.Green); }

            try { _dirtTile = Content.Load<Texture2D>("Sprites/Ground/Dirt/dirtMid"); }
            catch { _dirtTile = CreatePlaceholder(48, 48, Color.Brown); }

            try { _brickTile = Content.Load<Texture2D>("Sprites/Tiles/brickBrown"); }
            catch { _brickTile = CreatePlaceholder(48, 48, Color.Red); }

            // Загружаем врагов
            try { _goombaSprite = Content.Load<Texture2D>("Sprites/Enemies/slimeGreen"); }
            catch { _goombaSprite = CreatePlaceholder(48, 32, Color.Purple); }

            // Загружаем монеты
            try { _coinSprite = Content.Load<Texture2D>("Sprites/Items/coinGold"); }
            catch { _coinSprite = CreatePlaceholder(32, 32, Color.Gold); }

            // Загружаем фон
            try { _background = Content.Load<Texture2D>("Sprites/Backgrounds/blue_grass"); }
            catch { _background = CreatePlaceholder(1280, 720, new Color(100, 149, 237)); }

            CreateLevel();
            ResetPlayer();
        }

        private Texture2D CreatePlaceholder(int w, int h, Color c)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, w, h);
            Color[] data = new Color[w * h];
            for (int i = 0; i < data.Length; i++) data[i] = c;
            tex.SetData(data);
            return tex;
        }

        private void CreateLevel()
        {
            _platforms = new List<Platform>();
            _enemies = new List<Enemy>();
            _coins = new List<Coin>();

            int tileSize = 48;
            int levelLength = 50 + _currentLevel * 10; // Уровни становятся длиннее

            // Земля
            for (int x = 0; x < levelLength; x++)
            {
                _platforms.Add(new Platform(x * tileSize, 650, tileSize, tileSize, _grassTile, true));
                _platforms.Add(new Platform(x * tileSize, 698, tileSize, tileSize, _dirtTile, true));
            }

            // Платформы (разная сложность для каждого уровня)
            int platformCount = 5 + _currentLevel * 2;
            for (int i = 0; i < platformCount; i++)
            {
                int x = 8 + i * 7 + Random.Shared.Next(2);
                int y = 7 + Random.Shared.Next(5);
                int length = 3 + Random.Shared.Next(3);
                CreatePlatform(x, y, length, _brickTile);
            }

            // Вопрос блоки
            int questionCount = 2 + _currentLevel;
            for (int i = 0; i < questionCount; i++)
            {
                int x = 10 + i * 15;
                int y = 6 + Random.Shared.Next(3);
                _platforms.Add(new Platform(x * tileSize, y * tileSize - 48, tileSize, tileSize, _brickTile, true) { IsQuestion = true });
            }

            // Трубы
            int pipeCount = 2 + _currentLevel / 2;
            for (int i = 0; i < pipeCount; i++)
            {
                int x = 12 + i * 20;
                int height = 2 + Random.Shared.Next(2);
                CreatePipe(x, 13, height, tileSize);
            }

            // Враги (больше с каждым уровнем)
            int enemyCount = 3 + _currentLevel * 2;
            for (int i = 0; i < enemyCount; i++)
            {
                float x = 300 + i * 400 + Random.Shared.Next(100);
                _enemies.Add(new Enemy(x, 600, _goombaSprite));
            }

            // Монеты
            for (int i = 0; i < 30 + _currentLevel * 10; i++)
            {
                float x = 200 + i * 60 + Random.Shared.Next(30);
                float y = 400 + (float)Math.Sin(i * 0.5) * 150 + Random.Shared.Next(50);
                _coins.Add(new Coin(x, y, _coinSprite));
            }

            // Флаг финиша
            _finishLine = new Rectangle((levelLength - 3) * tileSize, 400, 10, 250);
        }

        private Rectangle _finishLine;

        private void CreatePlatform(int x, int y, int length, Texture2D tile)
        {
            int tileSize = 48;
            for (int i = 0; i < length; i++)
            {
                _platforms.Add(new Platform((x + i) * tileSize, y * tileSize - tileSize, tileSize, tileSize, tile, true));
            }
        }

        private void CreatePipe(int x, int y, int height, int tileSize)
        {
            for (int i = 0; i < height; i++)
            {
                _platforms.Add(new Platform(x * tileSize + tileSize / 4, (y + i) * tileSize, tileSize / 2, tileSize, _grassTile, true));
            }
        }

        private void ResetPlayer()
        {
            _playerPos = new Vector2(100, 500);
            _playerVel = Vector2.Zero;
            _playerGrounded = false;
            _playerRect = new Rectangle(0, 0, 48, 48);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keys = Keyboard.GetState();

            // Выход
            if (keys.IsKeyDown(Keys.Escape)) Exit();

            if (_state == GameState.Menu)
            {
                if (keys.IsKeyDown(Keys.Enter) && _prevKeys.IsKeyUp(Keys.Enter))
                    _state = GameState.Playing;
            }
            else if (_state == GameState.Playing)
            {
                if (!_isTransitioning)
                {
                    UpdatePlayer(dt, keys);
                    UpdateCamera();
                    UpdateEnemies(dt);
                    UpdateCoins();
                    CheckCollisions();
                    CheckLevelComplete();
                }
                else
                {
                    UpdateTransition(dt);
                }

                // Смерть от падения
                if (_playerPos.Y > 800)
                {
                    _lives--;
                    if (_lives <= 0) _state = GameState.GameOver;
                    else { ResetPlayer(); StartTransition(TransitionType.FadeOut); }
                }
            }
            else if (_state == GameState.GameOver)
            {
                if (keys.IsKeyDown(Keys.Space) && _prevKeys.IsKeyUp(Keys.Space))
                {
                    _lives = 3;
                    _score = 0;
                    _coinsCollected = 0;
                    ResetPlayer();
                    _state = GameState.Playing;
                }
            }

            _prevKeys = keys;
            base.Update(gameTime);
        }

        private void UpdatePlayer(float dt, KeyboardState keys)
        {
            // Движение
            float speed = 400f;
            float accel = 2000f;
            float friction = 0.85f;

            if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left))
            {
                _playerVel.X -= accel * dt;
                _playerFacingRight = false;
            }
            else if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right))
            {
                _playerVel.X += accel * dt;
                _playerFacingRight = true;
            }
            else
            {
                _playerVel.X *= friction;
            }

            _playerVel.X = MathHelper.Clamp(_playerVel.X, -speed, speed);

            // Прыжок
            if ((keys.IsKeyDown(Keys.Space) || keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up)) && _playerGrounded)
            {
                _playerVel.Y = -700f;
                _playerGrounded = false;
            }

            // Гравитация
            _playerVel.Y += 2000f * dt;
            _playerVel.Y = MathHelper.Clamp(_playerVel.Y, -1000f, 1000f);

            // Применяем скорость
            _playerPos += _playerVel * dt;

            // Анимация
            if (Math.Abs(_playerVel.X) > 10 && _playerGrounded)
                _playerAnimTimer += dt * 10f;
            else
                _playerAnimTimer = 0f;
        }

        private void UpdateCamera()
        {
            _cameraPos.X = MathHelper.Clamp(_playerPos.X - 640, 0, 4800 - 1280);
        }

        private void UpdateEnemies(float dt)
        {
            foreach (var enemy in _enemies)
            {
                if (enemy.Alive)
                {
                    enemy.Update(dt, 600f);

                    // Коллизия с игроком
                    if (GetPlayerRect().Intersects(enemy.Rect))
                    {
                        if (_playerVel.Y > 0 && _playerPos.Y < enemy.Pos.Y)
                        {
                            enemy.Kill();
                            _playerVel.Y = -400f;
                            _score += 200;
                        }
                        else
                        {
                            _lives--;
                            if (_lives <= 0) _state = GameState.GameOver;
                            else ResetPlayer();
                        }
                    }
                }
            }
        }

        private void UpdateCoins()
        {
            foreach (var coin in _coins)
            {
                if (!coin.Collected)
                {
                    coin.Update();

                    if (GetPlayerRect().Intersects(coin.Rect))
                    {
                        coin.Collected = true;
                        _score += 50;
                        _coinsCollected++;
                    }
                }
            }
        }

        private void CheckLevelComplete()
        {
            Rectangle playerRect = GetPlayerRect();
            if (playerRect.Intersects(_finishLine))
            {
                StartTransition(TransitionType.LevelComplete);
            }
        }

        private void StartTransition(TransitionType type)
        {
            _isTransitioning = true;
            _transitionTimer = 0f;
            _transitionType = type;
        }

        private void UpdateTransition(float dt)
        {
            _transitionTimer += dt;

            if (_transitionType == TransitionType.FadeOut)
            {
                if (_transitionTimer >= 1f)
                {
                    ResetPlayer();
                    _transitionTimer = 0f;
                    _transitionType = TransitionType.FadeIn;
                }
            }
            else if (_transitionType == TransitionType.FadeIn)
            {
                if (_transitionTimer >= 1f)
                {
                    _isTransitioning = false;
                }
            }
            else if (_transitionType == TransitionType.LevelComplete)
            {
                if (_transitionTimer >= 1.5f)
                {
                    _currentLevel++;
                    _score += 1000;
                    CreateLevel();
                    ResetPlayer();
                    _transitionTimer = 0f;
                    _transitionType = TransitionType.FadeIn;
                }
            }
        }
        private void CheckCollisions()
        {
            _playerGrounded = false;
            Rectangle playerRect = GetPlayerRect();

            foreach (var plat in _platforms)
            {
                if (playerRect.Intersects(plat.Rect))
                {
                    // Определяем направление коллизии
                    float overlapLeft = playerRect.Right - plat.Rect.Left;
                    float overlapRight = plat.Rect.Right - playerRect.Left;
                    float overlapTop = playerRect.Bottom - plat.Rect.Top;
                    float overlapBottom = plat.Rect.Bottom - playerRect.Top;

                    float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                    if (minOverlap == overlapTop && _playerVel.Y >= 0)
                    {
                        // Приземление
                        _playerPos.Y = plat.Rect.Top - _playerRect.Height / 2f;
                        _playerVel.Y = 0;
                        _playerGrounded = true;

                        if (plat.IsQuestion && !plat.IsUsed)
                        {
                            plat.IsUsed = true;
                            _score += 100;
                            _coinsCollected += 5;
                        }
                    }
                    else if (minOverlap == overlapBottom && _playerVel.Y < 0)
                    {
                        // Удар головой
                        _playerPos.Y = plat.Rect.Bottom + _playerRect.Height / 2f;
                        _playerVel.Y = 0;
                    }
                    else if (minOverlap == overlapLeft)
                    {
                        _playerPos.X = plat.Rect.Left - _playerRect.Width / 2f;
                        _playerVel.X = 0;
                    }
                    else if (minOverlap == overlapRight)
                    {
                        _playerPos.X = plat.Rect.Right + _playerRect.Width / 2f;
                        _playerVel.X = 0;
                    }
                }
            }
        }

        private Rectangle GetPlayerRect()
        {
            return new Rectangle((int)_playerPos.X - 24, (int)_playerPos.Y - 24, 48, 48);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(100, 149, 237));

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: Matrix.CreateTranslation(-_cameraPos.X, 0, 0));

            // Фон (параллакс)
            if (_background != null)
            {
                for (int x = -(int)(_cameraPos.X * 0.3f % _background.Width); x < 1280; x += _background.Width)
                {
                    _spriteBatch.Draw(_background, new Vector2(x - _cameraPos.X * 0.3f, 0), Color.White);
                }
            }

            // Платформы
            foreach (var plat in _platforms)
            {
                if (plat.Rect.Right > _cameraPos.X && plat.Rect.Left < _cameraPos.X + 1280)
                {
                    _spriteBatch.Draw(plat.Texture, plat.Rect, plat.IsQuestion && !plat.IsUsed ? Color.Gold : Color.White);
                }
            }

            // Монеты
            foreach (var coin in _coins)
            {
                if (!coin.Collected && coin.Rect.Right > _cameraPos.X && coin.Rect.Left < _cameraPos.X + 1280)
                {
                    _spriteBatch.Draw(coin.Texture, coin.Rect, Color.White);
                }
            }

            // Враги
            foreach (var enemy in _enemies)
            {
                if (enemy.Alive && enemy.Rect.Right > _cameraPos.X && enemy.Rect.Left < _cameraPos.X + 1280)
                {
                    SpriteEffects flip = enemy.FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    _spriteBatch.Draw(enemy.Texture, enemy.Rect, null, Color.White, 0f, Vector2.Zero, flip, 0f);
                }
            }

            // Игрок
            SpriteEffects playerFlip = _playerFacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            _spriteBatch.Draw(_playerSprite, GetPlayerRect(), null, Color.White, 0f, Vector2.Zero, playerFlip, 0f);

            // Флаг финиша
            _spriteBatch.Draw(_grassTile, _finishLine, Color.Gold);

            _spriteBatch.End();

            // Эффект перехода
            if (_isTransitioning)
            {
                _spriteBatch.Begin();
                DrawTransition(_spriteBatch);
                _spriteBatch.End();
            }

            // UI
            _spriteBatch.Begin();

            if (_font != null)
            {
                _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(20, 20), Color.White);
                _spriteBatch.DrawString(_font, $"Coins: {_coinsCollected}", new Vector2(20, 50), Color.Yellow);
                _spriteBatch.DrawString(_font, $"Lives: {_lives}", new Vector2(20, 80), Color.Red);
                _spriteBatch.DrawString(_font, $"Level: {_currentLevel}", new Vector2(20, 110), Color.LightGreen);

                // Сообщение о завершении уровня
                if (_isTransitioning && _transitionType == TransitionType.LevelComplete)
                {
                    _spriteBatch.DrawString(_font, "LEVEL COMPLETE!", new Vector2(520, 300), Color.Gold);
                    _spriteBatch.DrawString(_font, $"Loading Level {_currentLevel + 1}...", new Vector2(500, 360), Color.White);
                }
            }

            if (_state == GameState.Menu)
            {
                string title = "SUPER ALIEN PLATFORMER";
                Vector2 titleSize = _font?.MeasureString(title) ?? new Vector2(300, 50);
                _spriteBatch.DrawString(_font, title, new Vector2(640 - titleSize.X / 2, 250), Color.White);
                _spriteBatch.DrawString(_font, "Press ENTER to Start", new Vector2(640 - 120, 350), Color.Yellow);
                _spriteBatch.DrawString(_font, "A/D - Move | SPACE - Jump", new Vector2(640 - 130, 450), Color.LightGray);
            }
            else if (_state == GameState.GameOver)
            {
                _spriteBatch.DrawString(_font, "GAME OVER", new Vector2(550, 300), Color.Red);
                _spriteBatch.DrawString(_font, "Press SPACE to Restart", new Vector2(520, 380), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawTransition(SpriteBatch sb)
        {
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }

            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            if (_transitionType == TransitionType.FadeOut)
            {
                float alpha = MathHelper.Clamp(_transitionTimer, 0, 1f);
                sb.Draw(_pixelTexture, new Rectangle(0, 0, w, h), new Color((byte)0, (byte)0, (byte)0, (byte)(alpha * 200)));
            }
            else if (_transitionType == TransitionType.FadeIn)
            {
                float alpha = 1f - MathHelper.Clamp(_transitionTimer, 0, 1f);
                sb.Draw(_pixelTexture, new Rectangle(0, 0, w, h), new Color((byte)0, (byte)0, (byte)0, (byte)(alpha * 200)));
            }
            else if (_transitionType == TransitionType.LevelComplete)
            {
                float alpha = MathHelper.Clamp(_transitionTimer / 1.5f, 0, 1f);
                sb.Draw(_pixelTexture, new Rectangle(0, 0, w, h), new Color((byte)255, (byte)215, (byte)0, (byte)(alpha * 100)));
            }
        }
    }

    public class Platform
    {
        public Rectangle Rect;
        public Texture2D Texture;
        public bool Solid;
        public bool IsQuestion;
        public bool IsUsed;

        public Platform(int x, int y, int w, int h, Texture2D tex, bool solid)
        {
            Rect = new Rectangle(x, y, w, h);
            Texture = tex;
            Solid = solid;
        }
    }

    public class Enemy
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public Texture2D Texture;
        public bool Alive = true;
        public bool FacingRight;
        private float _animTimer;

        public Rectangle Rect => new Rectangle((int)Pos.X - 24, (int)Pos.Y - 16, 48, 32);

        public Enemy(float x, float y, Texture2D tex)
        {
            Pos = new Vector2(x, y);
            Texture = tex;
            FacingRight = Random.Shared.Next(2) == 0;
        }

        public void Update(float dt, float groundY)
        {
            Vel.X = FacingRight ? 50f : -50f;
            Vel.Y += 1500f * dt;

            Pos += Vel * dt;

            if (Pos.Y >= groundY)
            {
                Pos.Y = groundY;
                Vel.Y = 0;
            }

            _animTimer += dt;
            if (_animTimer > 2f)
            {
                FacingRight = !FacingRight;
                _animTimer = 0;
            }
        }

        public void Kill()
        {
            Alive = false;
        }
    }

    public class Coin
    {
        public float X, Y;
        public Texture2D Texture;
        public bool Collected;
        private float _animTimer;

        public Rectangle Rect => new Rectangle((int)X - 16, (int)Y - 16 + (int)(Math.Sin(_animTimer * 5) * 5), 32, 32);

        public Coin(float x, float y, Texture2D tex)
        {
            X = x;
            Y = y;
            Texture = tex;
        }

        public void Update()
        {
            _animTimer += 0.05f;
        }
    }

    public enum GameState { Menu, Playing, GameOver }
    public enum TransitionType { FadeOut, FadeIn, LevelComplete }
}
