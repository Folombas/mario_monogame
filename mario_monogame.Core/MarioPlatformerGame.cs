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
        private Rectangle _playerRect;
        
        // Прокачка
        private int _playerLevel = 1;
        private int _playerExp = 0;
        private int _expToNextLevel = 100;
        private WeaponType _currentWeapon = WeaponType.Sword;
        private int _damage = 10;
        private float _moveSpeed = 400f;

        // Камера
        private Vector2 _cameraPos;

        // Платформы и тайлы
        private List<Platform> _platforms;
        private int _tileSize = 48;
        
        // Биомы
        private BiomeType _currentBiome;
        private Dictionary<BiomeType, BiomeTiles> _biomeTiles;

        // Враги
        private List<Enemy> _enemies;
        
        // Сундуки
        private List<Chest> _chests;
        
        // Монеты
        private List<Coin> _coins;
        
        // Оружие на карте
        private List<WeaponPickup> _weapons;

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
        private SpriteFont _titleFont;
        private SpriteFont _levelCompleteFont;

        // Флаг финиша
        private Rectangle _finishLine;

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
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            try { _font = Content.Load<SpriteFont>("Fonts/Hud"); } catch { }
            try { _titleFont = Content.Load<SpriteFont>("Fonts/Smilen"); } catch { }
            try { _levelCompleteFont = Content.Load<SpriteFont>("Fonts/BloodyModes"); } catch { }

            // Загружаем спрайты игрока
            try { _playerSprite = Content.Load<Texture2D>("Sprites/Players/128x256/Green/alienGreen_stand"); }
            catch { _playerSprite = CreatePlaceholder(64, 64, Color.Lime); }

            // Загружаем биомы
            LoadBiomes();

            // Загружаем врагов
            LoadEnemySprites();

            // Загружаем предметы
            try { _coinSprite = Content.Load<Texture2D>("Sprites/Items/coinGold"); }
            catch { _coinSprite = CreatePlaceholder(32, 32, Color.Gold); }

            try { _chestSprite = Content.Load<Texture2D>("Sprites/Tiles/boxCrate"); }
            catch { _chestSprite = CreatePlaceholder(48, 48, Color.Brown); }

            // Загружаем фон
            try { _background = Content.Load<Texture2D>("Sprites/Backgrounds/blue_grass"); }
            catch { _background = CreatePlaceholder(1280, 720, new Color(100, 149, 237)); }

            CreateLevel();
            ResetPlayer();
        }

        private void LoadBiomes()
        {
            _biomeTiles = new Dictionary<BiomeType, BiomeTiles>();

            // Grassland
            var grass = new BiomeTiles();
            try { grass.Ground = Content.Load<Texture2D>("Sprites/Ground/Grass/grassMid"); }
            catch { grass.Ground = CreatePlaceholder(48, 48, Color.Green); }
            try { grass.Dirt = Content.Load<Texture2D>("Sprites/Ground/Dirt/dirtMid"); }
            catch { grass.Dirt = CreatePlaceholder(48, 48, Color.Brown); }
            try { grass.Brick = Content.Load<Texture2D>("Sprites/Tiles/brickBrown"); }
            catch { grass.Brick = CreatePlaceholder(48, 48, Color.Red); }
            try { grass.Background = Content.Load<Texture2D>("Sprites/Backgrounds/blue_grass"); }
            catch { }
            _biomeTiles[BiomeType.Grassland] = grass;

            // Desert
            var desert = new BiomeTiles();
            try { desert.Ground = Content.Load<Texture2D>("Sprites/Ground/Sand/sandMid"); }
            catch { desert.Ground = CreatePlaceholder(48, 48, Color.SandyBrown); }
            try { desert.Dirt = Content.Load<Texture2D>("Sprites/Ground/Sand/sand"); }
            catch { desert.Dirt = CreatePlaceholder(48, 48, Color.SandyBrown); }
            try { desert.Brick = Content.Load<Texture2D>("Sprites/Tiles/brickBrown"); }
            catch { desert.Brick = CreatePlaceholder(48, 48, Color.Orange); }
            try { desert.Background = Content.Load<Texture2D>("Sprites/Backgrounds/blue_desert"); }
            catch { }
            _biomeTiles[BiomeType.Desert] = desert;

            // Snow
            var snow = new BiomeTiles();
            try { snow.Ground = Content.Load<Texture2D>("Sprites/Ground/Snow/snowMid"); }
            catch { snow.Ground = CreatePlaceholder(48, 48, Color.White); }
            try { snow.Dirt = Content.Load<Texture2D>("Sprites/Ground/Snow/snow"); }
            catch { snow.Dirt = CreatePlaceholder(48, 48, Color.LightBlue); }
            try { snow.Brick = Content.Load<Texture2D>("Sprites/Tiles/brickGrey"); }
            catch { snow.Brick = CreatePlaceholder(48, 48, Color.Gray); }
            try { snow.Background = Content.Load<Texture2D>("Sprites/Backgrounds/blue_shroom"); }
            catch { }
            _biomeTiles[BiomeType.Snow] = snow;

            // Cave
            var cave = new BiomeTiles();
            try { cave.Ground = Content.Load<Texture2D>("Sprites/Ground/Stone/stoneMid"); }
            catch { cave.Ground = CreatePlaceholder(48, 48, Color.DarkGray); }
            try { cave.Dirt = Content.Load<Texture2D>("Sprites/Ground/Stone/stone"); }
            catch { cave.Dirt = CreatePlaceholder(48, 48, Color.Gray); }
            try { cave.Brick = Content.Load<Texture2D>("Sprites/Tiles/boxCrate"); }
            catch { cave.Brick = CreatePlaceholder(48, 48, Color.Brown); }
            try { cave.Background = Content.Load<Texture2D>("Sprites/Backgrounds/colored_land"); }
            catch { }
            _biomeTiles[BiomeType.Cave] = cave;

            // Castle
            var castle = new BiomeTiles();
            try { castle.Ground = Content.Load<Texture2D>("Sprites/Ground/Stone/stoneMid"); }
            catch { castle.Ground = CreatePlaceholder(48, 48, Color.Purple); }
            try { castle.Dirt = Content.Load<Texture2D>("Sprites/Ground/Stone/stone"); }
            catch { castle.Dirt = CreatePlaceholder(48, 48, Color.Purple); }
            try { castle.Brick = Content.Load<Texture2D>("Sprites/Tiles/brickGrey"); }
            catch { castle.Brick = CreatePlaceholder(48, 48, Color.Indigo); }
            try { castle.Background = Content.Load<Texture2D>("Sprites/Backgrounds/colored_shroom"); }
            catch { }
            _biomeTiles[BiomeType.Castle] = castle;

            // Forest
            var forest = new BiomeTiles();
            try { forest.Ground = Content.Load<Texture2D>("Sprites/Ground/Dirt/dirtMid"); }
            catch { forest.Ground = CreatePlaceholder(48, 48, Color.Green); }
            try { forest.Dirt = Content.Load<Texture2D>("Sprites/Ground/Dirt/dirt"); }
            catch { forest.Dirt = CreatePlaceholder(48, 48, Color.Brown); }
            try { forest.Brick = Content.Load<Texture2D>("Sprites/Tiles/boxCrate"); }
            catch { forest.Brick = CreatePlaceholder(48, 48, Color.DarkGreen); }
            try { forest.Background = Content.Load<Texture2D>("Sprites/Backgrounds/colored_grass"); }
            catch { }
            _biomeTiles[BiomeType.Forest] = forest;

            // Industrial
            var industrial = new BiomeTiles();
            try { industrial.Ground = Content.Load<Texture2D>("Sprites/Ground/Stone/stoneMid"); }
            catch { industrial.Ground = CreatePlaceholder(48, 48, Color.Gray); }
            try { industrial.Dirt = Content.Load<Texture2D>("Sprites/Ground/Stone/stone"); }
            catch { industrial.Dirt = CreatePlaceholder(48, 48, Color.DarkGray); }
            try { industrial.Brick = Content.Load<Texture2D>("Sprites/Tiles/boxCrate_warning"); }
            catch { industrial.Brick = CreatePlaceholder(48, 48, Color.Orange); }
            try { industrial.Background = Content.Load<Texture2D>("Sprites/Backgrounds/colored_desert"); }
            catch { }
            _biomeTiles[BiomeType.Industrial] = industrial;
        }

        private void LoadEnemySprites()
        {
            _enemySprites = new Dictionary<string, Texture2D>();
            try { _enemySprites["goomba"] = Content.Load<Texture2D>("Sprites/Enemies/slimeGreen"); }
            catch { _enemySprites["goomba"] = CreatePlaceholder(48, 32, Color.Purple); }
            try { _enemySprites["slime"] = Content.Load<Texture2D>("Sprites/Enemies/slimeBlue"); }
            catch { _enemySprites["slime"] = CreatePlaceholder(48, 32, Color.Blue); }
            try { _enemySprites["bee"] = Content.Load<Texture2D>("Sprites/Enemies/bee"); }
            catch { _enemySprites["bee"] = CreatePlaceholder(48, 48, Color.Yellow); }
            try { _enemySprites["mouse"] = Content.Load<Texture2D>("Sprites/Enemies/mouse"); }
            catch { _enemySprites["mouse"] = CreatePlaceholder(48, 32, Color.Gray); }
        }

        private Texture2D CreatePlaceholder(int w, int h, Color c)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, w, h);
            Color[] data = new Color[w * h];
            for (int i = 0; i < data.Length; i++) data[i] = c;
            tex.SetData(data);
            return tex;
        }

        private BiomeType GetBiomeForLevel(int level)
        {
            return (BiomeType)((level - 1) % 7);
        }

        private void CreateLevel()
        {
            _platforms = new List<Platform>();
            _enemies = new List<Enemy>();
            _coins = new List<Coin>();
            _chests = new List<Chest>();
            _weapons = new List<WeaponPickup>();

            _currentBiome = GetBiomeForLevel(_currentLevel);
            BiomeTiles tiles = _biomeTiles[_currentBiome];
            _background = tiles.Background ?? _background;

            int levelLength = 50 + _currentLevel * 10;

            // Земля
            for (int x = 0; x < levelLength; x++)
            {
                _platforms.Add(new Platform(x * _tileSize, 650, _tileSize, _tileSize, tiles.Ground, true));
                _platforms.Add(new Platform(x * _tileSize, 698, _tileSize, _tileSize, tiles.Dirt, true));
            }

            // Платформы
            int platformCount = 5 + _currentLevel * 2;
            for (int i = 0; i < platformCount; i++)
            {
                int x = 8 + i * 7 + Random.Shared.Next(2);
                int y = 7 + Random.Shared.Next(5);
                int length = 3 + Random.Shared.Next(3);
                CreatePlatform(x, y, length, tiles.Brick);
            }

            // Вопрос блоки
            int questionCount = 2 + _currentLevel;
            for (int i = 0; i < questionCount; i++)
            {
                int x = 10 + i * 15;
                int y = 6 + Random.Shared.Next(3);
                _platforms.Add(new Platform(x * _tileSize, y * _tileSize - 48, _tileSize, _tileSize, tiles.Brick, true) { IsQuestion = true });
            }

            // Трубы
            int pipeCount = 2 + _currentLevel / 2;
            for (int i = 0; i < pipeCount; i++)
            {
                int x = 12 + i * 20;
                int height = 2 + Random.Shared.Next(2);
                CreatePipe(x, 13, height);
            }

            // Сундуки
            int chestCount = 3 + _currentLevel / 2;
            for (int i = 0; i < chestCount; i++)
            {
                int x = 15 + i * 20 + Random.Shared.Next(5);
                int y = 600;
                _chests.Add(new Chest(x * _tileSize, y, _chestSprite));
            }

            // Оружие на карте
            if (Random.Shared.Next(3) == 0)
            {
                int x = 20 + Random.Shared.Next(30);
                _weapons.Add(new WeaponPickup(x * _tileSize, 500, WeaponType.Spear));
            }
            if (Random.Shared.Next(4) == 0)
            {
                int x = 35 + Random.Shared.Next(20);
                _weapons.Add(new WeaponPickup(x * _tileSize, 500, WeaponType.Axe));
            }

            // Враги (разные типы)
            int enemyCount = 3 + _currentLevel * 2;
            for (int i = 0; i < enemyCount; i++)
            {
                float x = 300 + i * 400 + Random.Shared.Next(100);
                string type = i % 3 == 0 ? "slime" : (i % 3 == 1 ? "bee" : "mouse");
                _enemies.Add(new Enemy(x, 600, _enemySprites.ContainsKey(type) ? _enemySprites[type] : _enemySprites["goomba"], type));
            }

            // Монеты
            for (int i = 0; i < 30 + _currentLevel * 10; i++)
            {
                float x = 200 + i * 60 + Random.Shared.Next(30);
                float y = 400 + (float)Math.Sin(i * 0.5) * 150 + Random.Shared.Next(50);
                _coins.Add(new Coin(x, y, _coinSprite));
            }

            // Флаг финиша
            _finishLine = new Rectangle((levelLength - 3) * _tileSize, 400, 10, 250);
        }

        private void CreatePlatform(int x, int y, int length, Texture2D tile)
        {
            for (int i = 0; i < length; i++)
            {
                _platforms.Add(new Platform((x + i) * _tileSize, y * _tileSize - _tileSize, _tileSize, _tileSize, tile, true));
            }
        }

        private void CreatePipe(int x, int y, int height)
        {
            for (int i = 0; i < height; i++)
            {
                _platforms.Add(new Platform(x * _tileSize + _tileSize / 4, (y + i) * _tileSize, _tileSize / 2, _tileSize, _biomeTiles[_currentBiome].Ground, true));
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
                    UpdateChests();
                    UpdateWeapons();
                    CheckCollisions();
                    CheckLevelComplete();
                }
                else
                {
                    UpdateTransition(dt);
                }

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
                    _currentLevel = 1;
                    _playerLevel = 1;
                    _playerExp = 0;
                    _lives = 3;
                    _score = 0;
                    _coinsCollected = 0;
                    _currentWeapon = WeaponType.Sword;
                    ResetPlayer();
                    CreateLevel();
                    _state = GameState.Playing;
                }
            }

            _prevKeys = keys;
            base.Update(gameTime);
        }

        private void UpdatePlayer(float dt, KeyboardState keys)
        {
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

            _playerVel.X = MathHelper.Clamp(_playerVel.X, -_moveSpeed, _moveSpeed);

            if ((keys.IsKeyDown(Keys.Space) || keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up)) && _playerGrounded)
            {
                _playerVel.Y = -700f;
                _playerGrounded = false;
            }

            _playerVel.Y += 2000f * dt;
            _playerVel.Y = MathHelper.Clamp(_playerVel.Y, -1000f, 1000f);

            _playerPos += _playerVel * dt;
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

                    if (GetPlayerRect().Intersects(enemy.Rect))
                    {
                        if (_playerVel.Y > 0 && _playerPos.Y < enemy.Pos.Y)
                        {
                            enemy.Kill();
                            _playerVel.Y = -400f;
                            AddExp(50);
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
                if (!coin.Collected && GetPlayerRect().Intersects(coin.Rect))
                {
                    coin.Collected = true;
                    _score += 50;
                    _coinsCollected++;
                    AddExp(10);
                }
            }
        }

        private void UpdateChests()
        {
            foreach (var chest in _chests)
            {
                if (!chest.Opened && GetPlayerRect().Intersects(chest.Rect))
                {
                    chest.Open();
                    LootChest(chest);
                }
            }
        }

        private void UpdateWeapons()
        {
            foreach (var weapon in _weapons)
            {
                if (!weapon.Collected && GetPlayerRect().Intersects(weapon.Rect))
                {
                    weapon.Collected = true;
                    _currentWeapon = weapon.WeaponType;
                    _damage += 5;
                    _score += 100;
                }
            }
        }

        private void LootChest(Chest chest)
        {
            var reward = chest.GetLoot();
            _score += reward.Score;
            _coinsCollected += reward.Coins;
            AddExp(reward.Exp);
            
            if (reward.Weapon != WeaponType.None)
            {
                _currentWeapon = reward.Weapon;
                _damage += 10;
            }
        }

        private void AddExp(int exp)
        {
            _playerExp += exp;
            if (_playerExp >= _expToNextLevel)
            {
                _playerLevel++;
                _playerExp -= _expToNextLevel;
                _expToNextLevel = (int)(_expToNextLevel * 1.5f);
                _damage += 2;
                _moveSpeed += 20;
                _lives++;
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
                    float overlapLeft = playerRect.Right - plat.Rect.Left;
                    float overlapRight = plat.Rect.Right - playerRect.Left;
                    float overlapTop = playerRect.Bottom - plat.Rect.Top;
                    float overlapBottom = plat.Rect.Bottom - playerRect.Top;

                    float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                    if (minOverlap == overlapTop && _playerVel.Y >= 0)
                    {
                        _playerPos.Y = plat.Rect.Top - _playerRect.Height / 2f;
                        _playerVel.Y = 0;
                        _playerGrounded = true;

                        if (plat.IsQuestion && !plat.IsUsed)
                        {
                            plat.IsUsed = true;
                            AddExp(25);
                            _coinsCollected += 5;
                        }
                    }
                    else if (minOverlap == overlapBottom && _playerVel.Y < 0)
                    {
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

        private void CheckLevelComplete()
        {
            if (GetPlayerRect().Intersects(_finishLine))
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
                    AddExp(200);
                    CreateLevel();
                    ResetPlayer();
                    _transitionTimer = 0f;
                    _transitionType = TransitionType.FadeIn;
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

            // Фон
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
                    Color tint = plat.IsQuestion && !plat.IsUsed ? Color.Gold : Color.White;
                    _spriteBatch.Draw(plat.Texture, plat.Rect, tint);
                }
            }

            // Сундуки
            foreach (var chest in _chests)
            {
                if (!chest.Opened && chest.Rect.Right > _cameraPos.X && chest.Rect.Left < _cameraPos.X + 1280)
                {
                    _spriteBatch.Draw(chest.Texture, chest.Rect, Color.White);
                }
            }

            // Оружие
            foreach (var weapon in _weapons)
            {
                if (!weapon.Collected && weapon.Rect.Right > _cameraPos.X && weapon.Rect.Left < _cameraPos.X + 1280)
                {
                    _spriteBatch.Draw(_pixelTexture, weapon.Rect, weapon.WeaponType == WeaponType.Spear ? Color.Silver : Color.Orange);
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
            _spriteBatch.Draw(_pixelTexture, _finishLine, Color.Gold);

            _spriteBatch.End();

            // Переход
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
                _spriteBatch.DrawString(_font, $"Hero Level: {_playerLevel}", new Vector2(20, 140), Color.Purple);
                _spriteBatch.DrawString(_font, $"EXP: {_playerExp}/{_expToNextLevel}", new Vector2(20, 170), Color.Cyan);
                _spriteBatch.DrawString(_font, $"Weapon: {_currentWeapon}", new Vector2(20, 200), Color.Orange);
                _spriteBatch.DrawString(_font, $"Damage: {_damage}", new Vector2(20, 230), Color.Red);

                if (_isTransitioning && _transitionType == TransitionType.LevelComplete)
                {
                    SpriteFont lcFont = _levelCompleteFont ?? _font;
                    _spriteBatch.DrawString(lcFont, "LEVEL COMPLETE!", new Vector2(470, 300), Color.Gold);
                    _spriteBatch.DrawString(_font ?? lcFont, $"Loading Level {_currentLevel + 1}...", new Vector2(480, 380), Color.White);
                }
            }

            if (_state == GameState.Menu)
            {
                string title = "SUPER ALIEN PLATFORMER";
                SpriteFont useFont = _titleFont ?? _font;
                Vector2 titleSize = useFont?.MeasureString(title) ?? new Vector2(400, 80);
                _spriteBatch.DrawString(useFont, title, new Vector2(640 - titleSize.X / 2 + 3, 180 + 3), Color.Black);
                _spriteBatch.DrawString(useFont, title, new Vector2(640 - titleSize.X / 2, 180), new Color(100, 255, 100));
                _spriteBatch.DrawString(_font ?? useFont, "Press ENTER to Start", new Vector2(640 - 120, 300), Color.Yellow);
                _spriteBatch.DrawString(_font ?? useFont, "A/D - Move | SPACE - Jump", new Vector2(640 - 130, 380), Color.LightGray);
                _spriteBatch.DrawString(_font ?? useFont, "Open chests for loot!", new Vector2(640 - 100, 430), Color.Orange);
            }
            else if (_state == GameState.GameOver)
            {
                SpriteFont goFont = _levelCompleteFont ?? _font;
                _spriteBatch.DrawString(goFont, "GAME OVER", new Vector2(500, 300), Color.Red);
                _spriteBatch.DrawString(_font ?? goFont, "Press SPACE to Restart", new Vector2(520, 380), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawTransition(SpriteBatch sb)
        {
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

        private Texture2D _coinSprite;
        private Texture2D _chestSprite;
        private Dictionary<string, Texture2D> _enemySprites;
    }

    public enum BiomeType { Grassland, Desert, Snow, Cave, Castle, Forest, Industrial }

    public class BiomeTiles
    {
        public Texture2D Ground;
        public Texture2D Dirt;
        public Texture2D Brick;
        public Texture2D Background;
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
        public string Type;
        private float _animTimer;

        public Rectangle Rect => new Rectangle((int)Pos.X - 24, (int)Pos.Y - 16, 48, 32);

        public Enemy(float x, float y, Texture2D tex, string type)
        {
            Pos = new Vector2(x, y);
            Texture = tex;
            Type = type;
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

        public void Kill() => Alive = false;
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

        public void Update() => _animTimer += 0.05f;
    }

    public class Chest
    {
        public float X, Y;
        public Texture2D Texture;
        public bool Opened;

        public Rectangle Rect => new Rectangle((int)X, (int)Y, 48, 48);

        public Chest(float x, float y, Texture2D tex)
        {
            X = x;
            Y = y;
            Texture = tex;
        }

        public void Open() => Opened = true;

        public Loot GetLoot()
        {
            var loot = new Loot();
            int roll = Random.Shared.Next(100);
            
            if (roll < 30)
            {
                loot.Coins = 10 + Random.Shared.Next(20);
                loot.Score = 100;
                loot.Exp = 20;
            }
            else if (roll < 60)
            {
                loot.Coins = 5 + Random.Shared.Next(10);
                loot.Score = 50;
                loot.Exp = 30;
            }
            else if (roll < 80)
            {
                loot.Weapon = WeaponType.Spear;
                loot.Score = 200;
                loot.Exp = 50;
            }
            else if (roll < 95)
            {
                loot.Weapon = WeaponType.Axe;
                loot.Score = 300;
                loot.Exp = 75;
            }
            else
            {
                loot.Weapon = WeaponType.Sword;
                loot.Score = 500;
                loot.Exp = 100;
            }
            
            return loot;
        }
    }

    public class Loot
    {
        public int Coins;
        public int Score;
        public int Exp;
        public WeaponType Weapon;
    }

    public class WeaponPickup
    {
        public float X, Y;
        public WeaponType WeaponType;
        public bool Collected;

        public Rectangle Rect => new Rectangle((int)X, (int)Y, 48, 32);

        public WeaponPickup(float x, float y, WeaponType type)
        {
            X = x;
            Y = y;
            WeaponType = type;
        }
    }

    public enum WeaponType { None, Sword, Spear, Axe }

    public enum GameState { Menu, Playing, GameOver }
    public enum TransitionType { FadeOut, FadeIn, LevelComplete }
}
