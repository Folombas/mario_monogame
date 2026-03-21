using mario_monogame.Core.GameObjects;
using mario_monogame.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core
{
    /// <summary>
    /// 2D платформер в стиле Mario с использованием спрайтов.
    /// </summary>
    public class MarioPlatformerGame : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

        // Игрок
        private Player player;

        // Платформы
        private List<Platform> platforms;
        private List<Platform> groundPlatforms;

        // Враги
        private List<Enemy> enemies;

        // Бонусы
        private List<Coin> coins;
        private List<PowerMushroom> powerMushrooms;
        private List<PoisonMushroom> poisonMushrooms;
        private List<StarPower> starPowers;
        private List<Gem> gems;
        private List<Flag> flags;

        // Камера
        private Vector2 cameraPosition;
        private float cameraTargetX;

        // Фон
        private Texture2D backgroundSprite;
        private Texture2D[] cloudSprites;
        private List<Cloud> clouds;

        // Уровень
        private float groundY;
        private int levelWidth;
        private int levelHeight;

        // UI
        private SpriteFont font;
        private Texture2D hudCoin;
        private Texture2D hudHeart;

        // Состояние
        private int score;
        private int coinsCollected;
        private int lives;
        private GameState gameState;

        // Для меню
        private KeyboardState previousKeyboardState;
        private int menuIndex;

        // Частицы
        private List<Particle> particles;

        public MarioPlatformerGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            graphicsDeviceManager.PreferredBackBufferHeight = 720;
            graphicsDeviceManager.IsFullScreen = false;

            groundY = 600f;
            levelWidth = 4000;
            levelHeight = 720;
            score = 0;
            coinsCollected = 0;
            lives = 3;
            gameState = GameState.Menu;
            menuIndex = 0;
            previousKeyboardState = Keyboard.GetState();

            clouds = new List<Cloud>();
            particles = new List<Particle>();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Загружаем шрифт
            try
            {
                font = Content.Load<SpriteFont>("Fonts/Hud");
            }
            catch
            {
                font = null;
            }

            // Загружаем фон
            try
            {
                backgroundSprite = Content.Load<Texture2D>("Sprites/Backgrounds/blue_grass");
            }
            catch
            {
                backgroundSprite = null;
            }

            // Загружаем HUD
            try
            {
                hudCoin = Content.Load<Texture2D>("Sprites/HUD/hudCoin");
                hudHeart = Content.Load<Texture2D>("Sprites/HUD/hudHeart_full");
            }
            catch { }

            // Создаём облака
            CreateClouds();

            // Создаём уровень
            CreateLevel();

            // Создаём игрока
            player = new Player(GraphicsDevice, new Vector2(100, groundY - 100), Content);

            // Камера
            cameraPosition = Vector2.Zero;
            cameraTargetX = 0f;
        }

        private void CreateClouds()
        {
            Random random = new Random(42);
            for (int i = 0; i < 10; i++)
            {
                clouds.Add(new Cloud(
                    random.Next(levelWidth),
                    random.Next(50, 250),
                    random.NextSingle() * 20 + 30f
                ));
            }
        }

        private void CreateLevel()
        {
            platforms = new List<Platform>();
            groundPlatforms = new List<Platform>();
            enemies = new List<Enemy>();
            coins = new List<Coin>();
            poisonMushrooms = new List<PoisonMushroom>();
            powerMushrooms = new List<PowerMushroom>();
            starPowers = new List<StarPower>();
            gems = new List<Gem>();
            flags = new List<Flag>();

            // Земля по всей длине уровня (используем тайлы)
            groundPlatforms.Add(new GroundPlatform(GraphicsDevice, new Rectangle(0, 650, levelWidth, 200), Content));

            // Создаём платформы из кирпичей и блоков
            CreateBrickPlatform(200, 520, 3);
            CreateBrickPlatform(400, 450, 5);
            CreateBrickPlatform(700, 520, 4);
            CreateBrickPlatform(950, 400, 3);
            CreateBrickPlatform(1200, 500, 6);
            CreateBrickPlatform(1500, 420, 4);
            CreateBrickPlatform(1800, 520, 5);
            CreateBrickPlatform(2100, 450, 3);
            CreateBrickPlatform(2400, 520, 4);
            CreateBrickPlatform(2700, 400, 5);

            // Блоки с вопросами
            CreateQuestionBlock(300, 400);
            CreateQuestionBlock(600, 350);
            CreateQuestionBlock(1100, 380);
            CreateQuestionBlock(1600, 350);
            CreateQuestionBlock(2000, 400);
            CreateQuestionBlock(2500, 350);

            // Трубы
            CreatePipe(350, 570, 80);
            CreatePipe(850, 550, 100);
            CreatePipe(1350, 570, 80);
            CreatePipe(1950, 550, 100);
            CreatePipe(2300, 570, 80);

            // Шипы
            CreateSpikes(500, 620, 3);
            CreateSpikes(1000, 620, 2);
            CreateSpikes(1500, 620, 4);
            CreateSpikes(2100, 620, 3);

            // Кусты (декор)
            CreateBush(150, 590);
            CreateBush(600, 590);
            CreateBush(1100, 590);
            CreateBush(1700, 590);
            CreateBush(2200, 590);

            // Монеты
            CreateCoin(220, 480);
            CreateCoin(250, 480);
            CreateCoin(280, 480);
            CreateCoin(420, 410);
            CreateCoin(450, 410);
            CreateCoin(480, 410);
            CreateCoin(510, 410);
            CreateCoin(720, 480);
            CreateCoin(750, 480);
            CreateCoin(780, 480);
            CreateCoin(970, 360);
            CreateCoin(1000, 360);
            CreateCoin(1220, 460);
            CreateCoin(1250, 460);
            CreateCoin(1280, 460);
            CreateCoin(1310, 460);
            CreateCoin(1520, 380);
            CreateCoin(1550, 380);
            CreateCoin(1820, 480);
            CreateCoin(1850, 480);
            CreateCoin(2120, 410);
            CreateCoin(2150, 410);
            CreateCoin(2420, 480);
            CreateCoin(2450, 480);
            CreateCoin(2720, 360);
            CreateCoin(2750, 360);
            CreateCoin(2780, 360);

            // Драгоценные камни
            CreateGem(450, 370, "Red");
            CreateGem(1250, 420, "Blue");
            CreateGem(2000, 370, "Green");
            CreateGem(2750, 320, "Yellow");

            // Звёзды
            CreateStar(1600, 300);
            CreateStar(2800, 300);

            // Грибы-бонусы (появляются из блоков)
            // Спавнятся при ударе по блоку

            // Ядовитые грибы
            poisonMushrooms.Add(new PoisonMushroom(GraphicsDevice, new Vector2(650, 400), Content));
            poisonMushrooms.Add(new PoisonMushroom(GraphicsDevice, new Vector2(1250, 350), Content));
            poisonMushrooms.Add(new PoisonMushroom(GraphicsDevice, new Vector2(1850, 400), Content));

            // Враги
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(500, groundY - 50), Content));
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(800, groundY - 50), Content));
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(1100, groundY - 50), Content));
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(1400, groundY - 50), Content));
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(1700, groundY - 50), Content));
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(2000, groundY - 50), Content));
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(2300, groundY - 50), Content));
            enemies.Add(new Goomba(GraphicsDevice, new Vector2(2600, groundY - 50), Content));

            enemies.Add(new SlimeEnemy(GraphicsDevice, new Vector2(600, groundY - 50), Content, "Blue"));
            enemies.Add(new SlimeEnemy(GraphicsDevice, new Vector2(1300, groundY - 50), Content, "Purple"));
            enemies.Add(new SlimeEnemy(GraphicsDevice, new Vector2(2100, groundY - 50), Content, "Green"));

            enemies.Add(new FlyingEnemy(GraphicsDevice, new Vector2(400, 400), Content, "bee"));
            enemies.Add(new FlyingEnemy(GraphicsDevice, new Vector2(1000, 350), Content, "fly"));
            enemies.Add(new FlyingEnemy(GraphicsDevice, new Vector2(1600, 400), Content, "bee"));

            enemies.Add(new MouseEnemy(GraphicsDevice, new Vector2(900, groundY - 50), Content));
            enemies.Add(new MouseEnemy(GraphicsDevice, new Vector2(1800, groundY - 50), Content));
            enemies.Add(new MouseEnemy(GraphicsDevice, new Vector2(2500, groundY - 50), Content));

            enemies.Add(new WormEnemy(GraphicsDevice, new Vector2(700, groundY - 50), Content, "Pink"));
            enemies.Add(new WormEnemy(GraphicsDevice, new Vector2(1500, groundY - 50), Content, "Pink"));

            // Флаг финиша
            flags.Add(new Flag(GraphicsDevice, new Vector2(levelWidth - 150, groundY - 150), Content, "Green"));
        }

        private void CreateBrickPlatform(int x, int y, int brickCount)
        {
            for (int i = 0; i < brickCount; i++)
            {
                platforms.Add(new BrickPlatform(GraphicsDevice, new Rectangle(x + i * 40, y, 40, 40), Content));
            }
        }

        private void CreateQuestionBlock(int x, int y)
        {
            platforms.Add(new QuestionPlatform(GraphicsDevice, new Rectangle(x, y, 40, 40), Content));
        }

        private void CreatePipe(int x, int y, int height)
        {
            platforms.Add(new PipePlatform(GraphicsDevice, new Rectangle(x, y, 50, height), Content));
        }

        private void CreateSpikes(int x, int y, int count)
        {
            for (int i = 0; i < count; i++)
            {
                platforms.Add(new SpikesPlatform(GraphicsDevice, new Rectangle(x + i * 40, y, 40, 30), Content));
            }
        }

        private void CreateBush(int x, int y)
        {
            platforms.Add(new BushPlatform(GraphicsDevice, new Rectangle(x, y, 80, 60), Content));
        }

        private void CreateCoin(int x, int y)
        {
            coins.Add(new Coin(GraphicsDevice, new Vector2(x, y), Content, "Gold"));
        }

        private void CreateGem(int x, int y, string color)
        {
            gems.Add(new Gem(GraphicsDevice, new Vector2(x, y), Content, color));
        }

        private void CreateStar(int x, int y)
        {
            starPowers.Add(new StarPower(GraphicsDevice, new Vector2(x, y), Content));
        }

        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (gameState == GameState.Menu)
            {
                UpdateMenu(keyboardState);
            }
            else if (gameState == GameState.Playing)
            {
                UpdateGame(gameTime, keyboardState, elapsed);
            }
            else if (gameState == GameState.GameOver)
            {
                if (keyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    ResetGame();
                    gameState = GameState.Playing;
                }
            }
            else if (gameState == GameState.LevelComplete)
            {
                if (keyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    ResetGame();
                    gameState = GameState.Playing;
                }
            }

            previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void UpdateMenu(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                if (previousKeyboardState.IsKeyUp(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.S))
                {
                    menuIndex = (menuIndex + 1) % 3;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                if (previousKeyboardState.IsKeyUp(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.W))
                {
                    menuIndex = (menuIndex - 1 + 3) % 3;
                }
            }

            if (keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space))
            {
                if (previousKeyboardState.IsKeyUp(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    if (menuIndex == 0)
                    {
                        gameState = GameState.Playing;
                        ResetGame();
                    }
                    else if (menuIndex == 2)
                    {
                        Exit();
                    }
                }
            }
        }

        private void UpdateGame(GameTime gameTime, KeyboardState keyboardState, float elapsed)
        {
            // Обновляем облака
            foreach (var cloud in clouds)
            {
                cloud.Update(elapsed);
            }

            // Игрок
            player.Update(gameTime, keyboardState, groundY - player.Bounds.Height / 2f);

            // Камера
            cameraTargetX = MathHelper.Clamp(player.Position.X - 640, 0, levelWidth - 1280);
            cameraPosition.X = MathHelper.Lerp(cameraPosition.X, cameraTargetX, elapsed * 5f);

            // Коллизии с платформами
            CheckPlatformCollisions();

            // Обновляем платформы
            foreach (var platform in platforms)
            {
                platform.Update(gameTime);
            }

            // Враги
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive)
                {
                    enemy.Update(gameTime, groundY - enemy.Bounds.Height / 2f, player);
                    CheckEnemyCollision(enemy);
                }
            }

            // Монеты
            foreach (var coin in coins)
            {
                if (!coin.IsCollected)
                {
                    coin.Update(gameTime);
                    CheckCoinCollection(coin);
                }
            }

            // Грибы
            foreach (var mushroom in powerMushrooms)
            {
                if (!mushroom.IsCollected)
                {
                    mushroom.Update(gameTime, groundY - mushroom.Bounds.Height / 2f);
                    CheckPowerupCollection(mushroom);
                }
            }

            foreach (var mushroom in poisonMushrooms)
            {
                if (!mushroom.IsCollected)
                {
                    mushroom.Update(gameTime, groundY - mushroom.Bounds.Height / 2f);
                    CheckPoisonCollision(mushroom);
                }
            }

            // Звёзды
            foreach (var star in starPowers)
            {
                if (!star.IsCollected)
                {
                    star.Update(gameTime, groundY - star.Bounds.Height / 2f);
                    CheckStarCollection(star);
                }
            }

            // Драгоценности
            foreach (var gem in gems)
            {
                if (!gem.IsCollected)
                {
                    gem.Update(gameTime);
                    CheckGemCollection(gem);
                }
            }

            // Флаги
            foreach (var flag in flags)
            {
                flag.Update(gameTime);
                CheckFlagReach(flag);
            }

            // Частицы
            UpdateParticles(elapsed);

            // Проверка падения в пропасть
            if (player.Position.Y > 800)
            {
                PlayerDie();
            }

            // Проверка победы
            if (player.Position.X > levelWidth - 100)
            {
                LevelComplete();
            }
        }

        private void CheckPlatformCollisions()
        {
            Rectangle playerBounds = player.Bounds;

            // Проверяем коллизии с наземными платформами
            foreach (var platform in groundPlatforms)
            {
                if (!platform.IsSolid) continue;

                Rectangle platBounds = platform.Bounds;

                if (playerBounds.Intersects(platBounds))
                {
                    float overlapTop = playerBounds.Bottom - platBounds.Top;
                    
                    if (overlapTop > 0 && overlapTop < 30 && player.Velocity.Y >= 0)
                    {
                        player.Position = new Vector2(player.Position.X, platBounds.Top - player.Bounds.Height / 2f);
                        player.ApplyGravity(0);
                    }
                }
            }

            // Проверяем коллизии с обычными платформами
            foreach (var platform in platforms)
            {
                if (!platform.IsSolid || platform.IsBroken) continue;

                Rectangle platBounds = platform.Bounds;

                if (playerBounds.Intersects(platBounds))
                {
                    float overlapLeft = playerBounds.Right - platBounds.Left;
                    float overlapRight = platBounds.Right - playerBounds.Left;
                    float overlapTop = playerBounds.Bottom - platBounds.Top;
                    float overlapBottom = platBounds.Bottom - playerBounds.Top;

                    float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                    if (minOverlap == overlapTop && player.Velocity.Y >= 0)
                    {
                        player.Position = new Vector2(player.Position.X, platBounds.Top - player.Bounds.Height / 2f);
                        player.ApplyGravity(0);

                        // Блок с вопросом
                        if (platform is QuestionPlatform qp && !qp.IsUsed)
                        {
                            qp.Use();
                            score += 100;
                            powerMushrooms.Add(new PowerMushroom(GraphicsDevice, platform.Bounds.Center.ToVector2(), Content));
                            CreateCoins(platform.Bounds.Center.ToVector2(), 5);
                        }
                        else if (platform is BrickPlatform bp && player.IsBig)
                        {
                            bp.Break();
                            score += 50;
                            CreateParticles(platform.Bounds.Center.ToVector2(), "brick");
                        }
                    }
                    else if (minOverlap == overlapBottom && player.Velocity.Y < 0)
                    {
                        player.Position = new Vector2(player.Position.X, platBounds.Bottom + player.Bounds.Height / 2f);
                        player.Velocity = new Vector2(player.Velocity.X, 0);

                        if (platform is BrickPlatform bp && player.IsBig)
                        {
                            bp.Break();
                            score += 50;
                            CreateParticles(platform.Bounds.Center.ToVector2(), "brick");
                        }
                    }
                    else if (minOverlap == overlapLeft)
                    {
                        player.Position = new Vector2(platBounds.Left - player.Bounds.Width / 2f, player.Position.Y);
                    }
                    else if (minOverlap == overlapRight)
                    {
                        player.Position = new Vector2(platBounds.Right + player.Bounds.Width / 2f, player.Position.Y);
                    }
                }
            }
        }

        private void CheckEnemyCollision(Enemy enemy)
        {
            if (!enemy.IsAlive) return;

            if (player.Bounds.Intersects(enemy.Bounds))
            {
                if (player.Velocity.Y > 0 && player.Position.Y < enemy.Position.Y - 10f)
                {
                    enemy.Squish();
                    score += 200;
                    player.ApplyGravity(-400f);
                    CreateParticles(enemy.Position, "enemy");
                }
                else if (!player.IsInvincible)
                {
                    player.Hit();
                    score -= 100;
                }
            }
        }

        private void CheckCoinCollection(Coin coin)
        {
            if (player.Bounds.Intersects(coin.Bounds))
            {
                coin.Collect();
                score += 50;
                coinsCollected++;
                CreateParticles(coin.Position, "coin");
            }
        }

        private void CheckPowerupCollection(PowerMushroom mushroom)
        {
            if (player.Bounds.Intersects(mushroom.Bounds))
            {
                mushroom.Collect();
                player.Grow();
                score += 500;
            }
        }

        private void CheckPoisonCollision(PoisonMushroom mushroom)
        {
            if (player.Bounds.Intersects(mushroom.Bounds))
            {
                mushroom.Collect();
                player.Hit();
                score -= 200;
            }
        }

        private void CheckStarCollection(StarPower star)
        {
            if (player.Bounds.Intersects(star.Bounds))
            {
                star.Collect();
                player.SetInvincible(10f);
                score += 1000;
            }
        }

        private void CheckGemCollection(Gem gem)
        {
            if (player.Bounds.Intersects(gem.Bounds))
            {
                gem.Collect();
                score += 300;
                CreateParticles(gem.Position, "gem");
            }
        }

        private void CheckFlagReach(Flag flag)
        {
            if (player.Bounds.Intersects(flag.Bounds))
            {
                flag.Reach();
                LevelComplete();
            }
        }

        private void CreateCoins(Vector2 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                coins.Add(new Coin(GraphicsDevice, new Vector2(position.X + (i - count/2) * 30, position.Y - 50), Content, "Gold"));
            }
        }

        private void CreateParticles(Vector2 position, string type)
        {
            for (int i = 0; i < 10; i++)
            {
                particles.Add(new Particle(
                    position,
                    new Vector2((float)(Random.Shared.NextDouble() * 200 - 100), (float)(Random.Shared.NextDouble() * -200 - 50)),
                    type
                ));
            }
        }

        private void UpdateParticles(float elapsed)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(elapsed);
                if (particles[i].IsDead)
                {
                    particles.RemoveAt(i);
                }
            }
        }

        private void PlayerDie()
        {
            lives--;
            if (lives <= 0)
            {
                gameState = GameState.GameOver;
            }
            else
            {
                ResetPlayer();
            }
        }

        private void LevelComplete()
        {
            score += 1000 + lives * 500;
            gameState = GameState.LevelComplete;
        }

        private void ResetGame()
        {
            score = 0;
            coinsCollected = 0;
            lives = 3;
            CreateLevel();
            ResetPlayer();
        }

        private void ResetPlayer()
        {
            player = new Player(GraphicsDevice, new Vector2(100, groundY - 100), Content);
            cameraPosition = Vector2.Zero;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(100, 149, 237));

            if (gameState == GameState.Menu)
            {
                DrawMenu();
            }
            else if (gameState == GameState.Playing || gameState == GameState.GameOver || gameState == GameState.LevelComplete)
            {
                DrawGame();
            }

            base.Draw(gameTime);
        }

        private void DrawMenu()
        {
            spriteBatch.Begin();

            // Фон меню
            if (backgroundSprite != null)
            {
                spriteBatch.Draw(backgroundSprite, GraphicsDevice.Viewport.Bounds, new Color(0.5f, 0.5f, 0.5f, 1f));
            }

            if (font != null)
            {
                string title = "SUPER ALIEN PLATFORMER";
                Vector2 titleSize = font.MeasureString(title);
                spriteBatch.DrawString(font, title, new Vector2(640 - titleSize.X / 2 + 2, 180 + 2), Color.Black);
                spriteBatch.DrawString(font, title, new Vector2(640 - titleSize.X / 2, 180), new Color(100, 200, 100));

                string[] menuItems = { "Start Game", "Controls", "Exit" };
                for (int i = 0; i < menuItems.Length; i++)
                {
                    Color color = i == menuIndex ? Color.Yellow : Color.White;
                    string marker = i == menuIndex ? "> " : "  ";
                    Vector2 itemSize = font.MeasureString(menuItems[i]);
                    spriteBatch.DrawString(font, marker + menuItems[i], new Vector2(640 - itemSize.X / 2, 320 + i * 60), color);
                }

                string controls = "A/D or Arrows - Move | Space - Jump";
                Vector2 controlsSize = font.MeasureString(controls);
                spriteBatch.DrawString(font, controls, new Vector2(640 - controlsSize.X / 2, 520), Color.LightGray);
            }

            spriteBatch.End();
        }

        private void DrawGame()
        {
            // Рисуем фон
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            
            if (backgroundSprite != null)
            {
                // Параллакс фон
                float parallaxX = cameraPosition.X * 0.3f;
                for (int x = -(int)(parallaxX % backgroundSprite.Width); x < 1280; x += backgroundSprite.Width)
                {
                    spriteBatch.Draw(backgroundSprite, new Vector2(x, 0), Color.White);
                }
            }

            // Облака
            foreach (var cloud in clouds)
            {
                cloud.Draw(spriteBatch, cameraPosition);
            }

            spriteBatch.End();

            // Игровые объекты
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: Matrix.CreateTranslation(-cameraPosition.X, 0, 0));

            // Платформы
            foreach (var platform in groundPlatforms)
            {
                if (platform.Bounds.Right > cameraPosition.X - 100 && platform.Bounds.Left < cameraPosition.X + 1380)
                {
                    platform.Draw(spriteBatch, cameraPosition);
                }
            }

            foreach (var platform in platforms)
            {
                if (platform.Bounds.Right > cameraPosition.X - 100 && platform.Bounds.Left < cameraPosition.X + 1380)
                {
                    platform.Draw(spriteBatch, cameraPosition);
                }
            }

            // Бонусы
            foreach (var mushroom in powerMushrooms)
            {
                mushroom.Draw(spriteBatch, cameraPosition);
            }

            foreach (var mushroom in poisonMushrooms)
            {
                mushroom.Draw(spriteBatch, cameraPosition);
            }

            foreach (var star in starPowers)
            {
                star.Draw(spriteBatch, cameraPosition);
            }

            foreach (var gem in gems)
            {
                gem.Draw(spriteBatch, cameraPosition);
            }

            foreach (var flag in flags)
            {
                flag.Draw(spriteBatch, cameraPosition);
            }

            // Монеты
            foreach (var coin in coins)
            {
                coin.Draw(spriteBatch, cameraPosition);
            }

            // Враги
            foreach (var enemy in enemies)
            {
                if (enemy.Bounds.Right > cameraPosition.X - 100 && enemy.Bounds.Left < cameraPosition.X + 1380)
                {
                    enemy.Draw(spriteBatch, cameraPosition);
                }
            }

            // Игрок
            player.Draw(spriteBatch, cameraPosition);

            // Частицы
            foreach (var particle in particles)
            {
                particle.Draw(spriteBatch, cameraPosition);
            }

            spriteBatch.End();

            // UI
            spriteBatch.Begin();
            DrawUI();
            spriteBatch.End();
        }

        private void DrawUI()
        {
            // Счёт
            if (font != null)
            {
                spriteBatch.DrawString(font, $"Score: {score}", new Vector2(20, 20), Color.White);
                spriteBatch.DrawString(font, $"x {coinsCollected}", new Vector2(120, 20), Color.Yellow);
                spriteBatch.DrawString(font, $"Lives: {lives}", new Vector2(20, 50), Color.Red);
            }

            // HUD иконки
            if (hudCoin != null)
            {
                spriteBatch.Draw(hudCoin, new Vector2(20, 80), new Rectangle(0, 0, 32, 32), Color.White);
            }

            // Game Over
            if (gameState == GameState.GameOver)
            {
                if (font != null)
                {
                    string gameOverText = "GAME OVER";
                    Vector2 gameOverSize = font.MeasureString(gameOverText);
                    spriteBatch.DrawString(font, gameOverText, new Vector2(640 - gameOverSize.X / 2 + 2, 360 + 2), Color.Black);
                    spriteBatch.DrawString(font, gameOverText, new Vector2(640 - gameOverSize.X / 2, 360), Color.Red);

                    string restartText = "Press Space to Restart";
                    Vector2 restartSize = font.MeasureString(restartText);
                    spriteBatch.DrawString(font, restartText, new Vector2(640 - restartSize.X / 2, 420), Color.White);
                }
            }

            // Level Complete
            if (gameState == GameState.LevelComplete)
            {
                if (font != null)
                {
                    string levelText = "LEVEL COMPLETE!";
                    Vector2 levelSize = font.MeasureString(levelText);
                    spriteBatch.DrawString(font, levelText, new Vector2(640 - levelSize.X / 2 + 2, 360 + 2), Color.Black);
                    spriteBatch.DrawString(font, levelText, new Vector2(640 - levelSize.X / 2, 360), Color.Gold);

                    string scoreText = $"Final Score: {score}";
                    Vector2 scoreSize = font.MeasureString(scoreText);
                    spriteBatch.DrawString(font, scoreText, new Vector2(640 - scoreSize.X / 2, 420), Color.White);

                    string restartText = "Press Space to Play Again";
                    Vector2 restartSize = font.MeasureString(restartText);
                    spriteBatch.DrawString(font, restartText, new Vector2(640 - restartSize.X / 2, 480), Color.LightGray);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                player?.Dispose();
                spriteBatch?.Dispose();

                foreach (var platform in platforms)
                {
                    platform?.Dispose();
                }

                foreach (var enemy in enemies)
                {
                    enemy?.Dispose();
                }

                foreach (var coin in coins)
                {
                    coin?.Dispose();
                }

                foreach (var mushroom in poisonMushrooms)
                {
                    mushroom?.Dispose();
                }

                foreach (var mushroom in powerMushrooms)
                {
                    mushroom?.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        GameOver,
        LevelComplete
    }

    /// <summary>
    /// Простая система частиц.
    /// </summary>
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public string Type;
        public float Lifetime;
        public bool IsDead => Lifetime <= 0;

        public Particle(Vector2 position, Vector2 velocity, string type)
        {
            Position = position;
            Velocity = velocity;
            Type = type;
            Lifetime = 1f;
        }

        public void Update(float elapsed)
        {
            Position += Velocity * elapsed;
            Velocity.Y += 500f * elapsed; // Гравитация
            Lifetime -= elapsed;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = Position - cameraPosition;
            Color color = Type switch
            {
                "coin" => Color.Gold,
                "gem" => Color.Magenta,
                "enemy" => Color.Brown,
                _ => Color.Gray
            };

            spriteBatch.Draw(
                GetTextureForType(spriteBatch.GraphicsDevice, Type),
                drawPos,
                null,
                new Color(color, Lifetime),
                0f,
                new Vector2(4, 4),
                Lifetime,
                SpriteEffects.None,
                0f
            );
        }

        private Texture2D GetTextureForType(GraphicsDevice gd, string type)
        {
            Texture2D tex = new Texture2D(gd, 8, 8);
            Color[] data = new Color[64];
            
            Color fill = type switch
            {
                "coin" => Color.Gold,
                "gem" => Color.Magenta,
                "enemy" => Color.Brown,
                _ => Color.Gray
            };

            for (int i = 0; i < 64; i++)
                data[i] = fill;

            tex.SetData(data);
            return tex;
        }
    }

    /// <summary>
    /// Декоративное облако.
    /// </summary>
    public class Cloud
    {
        public Vector2 Position;
        public float Speed;
        public float Scale;

        public Cloud(float x, float y, float scale)
        {
            Position = new Vector2(x, y);
            Speed = 10f;
            Scale = scale;
        }

        public void Update(float elapsed)
        {
            Position.X += Speed * elapsed;
            if (Position.X > 4500)
            {
                Position.X = -100;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Vector2 drawPos = Position - cameraPosition * 0.5f; // Параллакс
            
            // Рисуем простое облако из кругов
            Color cloudColor = new Color(255, 255, 255, 200);
            
            spriteBatch.Draw(
                GetCircleTexture(spriteBatch.GraphicsDevice),
                drawPos,
                null,
                cloudColor,
                0f,
                new Vector2(16, 16),
                Scale,
                SpriteEffects.None,
                0f
            );
        }

        private Texture2D GetCircleTexture(GraphicsDevice gd)
        {
            Texture2D tex = new Texture2D(gd, 32, 32);
            Color[] data = new Color[1024];
            
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dx = x - 16;
                    float dy = y - 16;
                    float dist = dx * dx + dy * dy;
                    
                    if (dist < 256)
                        data[y * 32 + x] = Color.White;
                    else
                        data[y * 32 + x] = Color.Transparent;
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
