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
    /// 2D платформер в стиле Mario.
    /// </summary>
    public class MarioPlatformerGame : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

        // Игрок
        private Player player;

        // Платформы
        private List<Platform> platforms;
        private List<Platform> visiblePlatforms;

        // Враги
        private List<Goomba> goombas;
        private List<PoisonMushroom> poisonMushrooms;

        // Бонусы
        private List<Coin> coins;
        private List<PowerMushroom> powerMushrooms;

        // Камера
        private Vector2 cameraPosition;
        private float cameraTargetX;

        // Уровень
        private float groundY;
        private int levelWidth;
        
        // UI
        private SpriteFont font;
        
        // Состояние
        private int score;
        private int coinsCollected;
        private int lives;
        private GameState gameState;
        
        // Для меню
        private KeyboardState previousKeyboardState;
        private int menuIndex;

        public MarioPlatformerGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            graphicsDeviceManager.PreferredBackBufferHeight = 720;
            
            groundY = 600f;
            levelWidth = 3000;
            score = 0;
            coinsCollected = 0;
            lives = 3;
            gameState = GameState.Menu;
            menuIndex = 0;
            previousKeyboardState = Keyboard.GetState();
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
            
            // Создаём уровень
            CreateLevel();
            
            // Создаём игрока
            player = new Player(GraphicsDevice, new Vector2(100, groundY - 100));
            
            // Камера
            cameraPosition = Vector2.Zero;
            cameraTargetX = 0f;
        }
        
        private void CreateLevel()
        {
            platforms = new List<Platform>();
            goombas = new List<Goomba>();
            coins = new List<Coin>();
            poisonMushrooms = new List<PoisonMushroom>();
            powerMushrooms = new List<PowerMushroom>();
            
            // Земля по всей длине уровня
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(0, 650, levelWidth, 100), PlatformType.Ground));
            
            // Платформы
            CreatePlatform(200, 520, 3);  // 3 блока
            CreatePlatform(400, 450, 5);
            CreatePlatform(700, 520, 4);
            CreatePlatform(950, 400, 3);
            CreatePlatform(1200, 500, 6);
            CreatePlatform(1500, 420, 4);
            CreatePlatform(1800, 520, 5);
            CreatePlatform(2100, 450, 3);
            CreatePlatform(2400, 520, 4);
            
            // Трубы
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(350, 570, 60, 80), PlatformType.Pipe));
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(850, 550, 60, 100), PlatformType.Pipe));
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(1350, 570, 60, 80), PlatformType.Pipe));
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(1950, 550, 60, 100), PlatformType.Pipe));
            
            // Блоки с вопросами
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(300, 400, 40, 40), PlatformType.Question));
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(600, 350, 40, 40), PlatformType.Question));
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(1100, 380, 40, 40), PlatformType.Question));
            platforms.Add(new Platform(GraphicsDevice, new Rectangle(1600, 350, 40, 40), PlatformType.Question));
            
            // Монеты
            CreateCoin(220, 480);
            CreateCoin(250, 480);
            CreateCoin(420, 410);
            CreateCoin(450, 410);
            CreateCoin(480, 410);
            CreateCoin(720, 480);
            CreateCoin(750, 480);
            CreateCoin(970, 360);
            CreateCoin(1220, 460);
            CreateCoin(1250, 460);
            CreateCoin(1280, 460);
            CreateCoin(1520, 380);
            CreateCoin(1820, 480);
            CreateCoin(2120, 410);
            CreateCoin(2420, 480);
            
            // Гумбы
            goombas.Add(new Goomba(GraphicsDevice, new Vector2(500, groundY - 50)));
            goombas.Add(new Goomba(GraphicsDevice, new Vector2(800, groundY - 50)));
            goombas.Add(new Goomba(GraphicsDevice, new Vector2(1100, groundY - 50)));
            goombas.Add(new Goomba(GraphicsDevice, new Vector2(1400, groundY - 50)));
            goombas.Add(new Goomba(GraphicsDevice, new Vector2(1700, groundY - 50)));
            goombas.Add(new Goomba(GraphicsDevice, new Vector2(2000, groundY - 50)));
            goombas.Add(new Goomba(GraphicsDevice, new Vector2(2300, groundY - 50)));
            
            // Ядовитые грибы
            poisonMushrooms.Add(new PoisonMushroom(GraphicsDevice, new Vector2(650, 400)));
            poisonMushrooms.Add(new PoisonMushroom(GraphicsDevice, new Vector2(1250, 350)));
            poisonMushrooms.Add(new PoisonMushroom(GraphicsDevice, new Vector2(1850, 400)));
        }
        
        private void CreatePlatform(int x, int y, int brickCount)
        {
            for (int i = 0; i < brickCount; i++)
            {
                platforms.Add(new Platform(GraphicsDevice, new Rectangle(x + i * 40, y, 40, 40), PlatformType.Brick));
            }
        }
        
        private void CreateCoin(int x, int y)
        {
            coins.Add(new Coin(GraphicsDevice, new Vector2(x, y)));
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
            
            previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }
        
        private void UpdateMenu(KeyboardState keyboardState)
        {
            // Навигация
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
            
            // Выбор
            if (keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space))
            {
                if (previousKeyboardState.IsKeyUp(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    if (menuIndex == 0) // Start
                    {
                        gameState = GameState.Playing;
                        ResetGame();
                    }
                    else if (menuIndex == 2) // Exit
                    {
                        Exit();
                    }
                }
            }
        }
        
        private void UpdateGame(GameTime gameTime, KeyboardState keyboardState, float elapsed)
        {
            // Игрок
            player.Update(gameTime, keyboardState, groundY - 16f);
            
            // Камера
            cameraTargetX = MathHelper.Clamp(player.Position.X - 640, 0, levelWidth - 1280);
            cameraPosition.X = MathHelper.Lerp(cameraPosition.X, cameraTargetX, elapsed * 5f);
            
            // Коллизии с платформами
            CheckPlatformCollisions();
            
            // Враги
            foreach (var goomba in goombas)
            {
                if (goomba.IsAlive)
                {
                    goomba.Update(gameTime, groundY - 16f, player);
                    CheckGoombaCollision(goomba);
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
            
            // Ядовитые грибы
            foreach (var mushroom in poisonMushrooms)
            {
                if (!mushroom.IsCollected)
                {
                    mushroom.Update(gameTime, groundY - 16f);
                    CheckPoisonCollision(mushroom);
                }
            }
            
            // Проверка падения в пропасть
            if (player.Position.Y > 800)
            {
                PlayerDie();
            }
            
            // Проверка победы (достиг конца уровня)
            if (player.Position.X > levelWidth - 100)
            {
                LevelComplete();
            }
        }
        
        private void CheckPlatformCollisions()
        {
            Rectangle playerBounds = player.Bounds;
            
            foreach (var platform in platforms)
            {
                if (!platform.IsSolid) continue;
                
                Rectangle platBounds = platform.Bounds;
                
                if (playerBounds.Intersects(platBounds))
                {
                    // Определяем направление коллизии
                    float overlapLeft = playerBounds.Right - platBounds.Left;
                    float overlapRight = platBounds.Right - playerBounds.Left;
                    float overlapTop = playerBounds.Bottom - platBounds.Top;
                    float overlapBottom = platBounds.Bottom - playerBounds.Top;
                    
                    float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));
                    
                    if (minOverlap == overlapTop && player.Velocity.Y >= 0)
                    {
                        // Приземление на платформу
                        player.Position = new Vector2(player.Position.X, platBounds.Top - player.Bounds.Height / 2);
                        player.ApplyGravity(0);
                    }
                    else if (minOverlap == overlapBottom && player.Velocity.Y < 0)
                    {
                        // Удар головой
                        player.Position = new Vector2(player.Position.X, platBounds.Bottom + player.Bounds.Height / 2);
                        player.Velocity = new Vector2(player.Velocity.X, 0);
                        
                        // Блок с вопросом
                        if (platform.Type == PlatformType.Question && !platform.IsUsed)
                        {
                            platform.Use();
                            score += 100;
                            // Спавним бонус
                            powerMushrooms.Add(new PowerMushroom(GraphicsDevice, platform.Bounds.Center.ToVector2()));
                        }
                        else if (platform.Type == PlatformType.Brick && player.State == PlayerState.Big)
                        {
                            platform.Break();
                            score += 50;
                        }
                    }
                    else if (minOverlap == overlapLeft)
                    {
                        player.Position = new Vector2(platBounds.Left - player.Bounds.Width / 2, player.Position.Y);
                    }
                    else if (minOverlap == overlapRight)
                    {
                        player.Position = new Vector2(platBounds.Right + player.Bounds.Width / 2, player.Position.Y);
                    }
                }
            }
        }
        
        private void CheckGoombaCollision(Goomba goomba)
        {
            if (!goomba.IsAlive) return;
            
            if (player.Bounds.Intersects(goomba.Bounds))
            {
                if (player.Velocity.Y > 0 && player.Position.Y < goomba.Position.Y - 10f)
                {
                    // Убили гумбу
                    goomba.Squish();
                    score += 200;
                    player.ApplyGravity(-400f);
                }
                else if (!player.IsInvincible)
                {
                    // Игрок получил урон
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
            score += 1000;
            gameState = GameState.LevelComplete;
        }
        
        private void ResetGame()
        {
            score = 0;
            coinsCollected = 0;
            lives = 3;
            ResetPlayer();
            CreateLevel();
        }
        
        private void ResetPlayer()
        {
            player = new Player(GraphicsDevice, new Vector2(100, groundY - 100));
            cameraPosition = Vector2.Zero;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(100, 149, 237)); // Небесно-голубой

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
            
            if (font != null)
            {
                // Заголовок
                string title = "SUPER MARIO PLATFORMER";
                Vector2 titleSize = font.MeasureString(title);
                spriteBatch.DrawString(font, title, new Vector2(640 - titleSize.X / 2 + 2, 200 + 2), Color.Black);
                spriteBatch.DrawString(font, title, new Vector2(640 - titleSize.X / 2, 200), Color.White);
                
                // Пункты меню
                string[] menuItems = { "Start Game", "Controls", "Exit" };
                for (int i = 0; i < menuItems.Length; i++)
                {
                    Color color = i == menuIndex ? Color.Yellow : Color.White;
                    string marker = i == menuIndex ? "> " : "  ";
                    Vector2 itemSize = font.MeasureString(menuItems[i]);
                    spriteBatch.DrawString(font, marker + menuItems[i], new Vector2(640 - itemSize.X / 2, 350 + i * 60), color);
                }
                
                // Управление
                string controls = "Arrow Keys/WASD - Move | Space - Jump";
                Vector2 controlsSize = font.MeasureString(controls);
                spriteBatch.DrawString(font, controls, new Vector2(640 - controlsSize.X / 2, 550), Color.LightGray);
            }
            
            spriteBatch.End();
        }
        
        private void DrawGame()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: Matrix.CreateTranslation(-cameraPosition.X, 0, 0));
            
            // Платформы
            foreach (var platform in platforms)
            {
                if (platform.Bounds.Right > cameraPosition.X && platform.Bounds.Left < cameraPosition.X + 1280)
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
            
            // Монеты
            foreach (var coin in coins)
            {
                coin.Draw(spriteBatch, cameraPosition);
            }
            
            // Враги
            foreach (var goomba in goombas)
            {
                goomba.Draw(spriteBatch, cameraPosition);
            }
            
            // Игрок
            player.Draw(spriteBatch, cameraPosition);
            
            spriteBatch.End();
            
            // UI
            spriteBatch.Begin();
            DrawUI();
            spriteBatch.End();
        }
        
        private void DrawUI()
        {
            if (font == null) return;
            
            // Счёт
            spriteBatch.DrawString(font, $"Score: {score}", new Vector2(20, 20), Color.White);
            
            // Монеты
            spriteBatch.DrawString(font, $"Coins: {coinsCollected}", new Vector2(20, 50), Color.Yellow);
            
            // Жизни
            spriteBatch.DrawString(font, $"Lives: {lives}", new Vector2(20, 80), Color.Red);
            
            // Game Over
            if (gameState == GameState.GameOver)
            {
                string gameOverText = "GAME OVER";
                Vector2 gameOverSize = font.MeasureString(gameOverText);
                spriteBatch.DrawString(font, gameOverText, new Vector2(640 - gameOverSize.X / 2 + 2, 360 + 2), Color.Black);
                spriteBatch.DrawString(font, gameOverText, new Vector2(640 - gameOverSize.X / 2, 360), Color.Red);
                
                string restartText = "Press Space to Restart";
                Vector2 restartSize = font.MeasureString(restartText);
                spriteBatch.DrawString(font, restartText, new Vector2(640 - restartSize.X / 2, 420), Color.White);
            }
            
            // Level Complete
            if (gameState == GameState.LevelComplete)
            {
                string levelText = "LEVEL COMPLETE!";
                Vector2 levelSize = font.MeasureString(levelText);
                spriteBatch.DrawString(font, levelText, new Vector2(640 - levelSize.X / 2 + 2, 360 + 2), Color.Black);
                spriteBatch.DrawString(font, levelText, new Vector2(640 - levelSize.X / 2, 360), Color.Gold);
                
                string scoreText = $"Final Score: {score}";
                Vector2 scoreSize = font.MeasureString(scoreText);
                spriteBatch.DrawString(font, scoreText, new Vector2(640 - scoreSize.X / 2, 420), Color.White);
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
                    // Platform не реализует IDisposable
                }
                
                foreach (var goomba in goombas)
                {
                    goomba?.Dispose();
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
}
