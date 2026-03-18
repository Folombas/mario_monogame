using mario_monogame.Core.GameObjects;
using mario_monogame.Core.Localization;
using mario_monogame.Core.UI;
using mario_monogame.Core.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace mario_monogame.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings,
    /// and platform-specific configurations.
    /// </summary>
    public class mario_monogameGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

        // Игровые объекты
        private Sky sky;
        private Ground ground;
        private List<AppleTree> trees;
        private List<CarrotPatch> carrotPatches;
        
        // Новый зайчик и его домик
        private Rabbit rabbit;
        private House rabbitHouse;
        private ParticleSystem particleSystem;
        
        // Новые системы
        private DayNightCycle dayNightCycle;
        private WeatherSystem weatherSystem;
        private List<Fox> foxes;
        private List<Crow> crows;
        private MainMenu mainMenu;
        private SoundManager soundManager;
        
        // Состояние игры
        private GameState gameState;
        private int score;
        private int carrotsInHouse;
        private int currentLevel;
        private float gameTimeTotal;
        
        // UI
        private SpriteFont font;
        
        // Камера
        private Vector2 cameraPosition;
        private float cameraTargetX;
        
        // Для меню
        private KeyboardState previousKeyboardState;

        /// <summary>
        /// Indicates if the game is running on a mobile platform.
        /// </summary>
        public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Indicates if the game is running on a desktop platform.
        /// </summary>
        public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings,
        /// initializes services like settings and leaderboard managers, and sets up the
        /// screen manager for screen transitions.
        /// </summary>
        public mario_monogameGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";

            // Configure screen orientations.
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // Устанавливаем разрешение экрана
            graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            graphicsDeviceManager.PreferredBackBufferHeight = 720;
            
            // Инициализация состояния
            gameState = GameState.Menu;  // Начинаем с меню
            score = 0;
            carrotsInHouse = 0;
            currentLevel = 1;
            gameTimeTotal = 0f;
            cameraPosition = Vector2.Zero;
            cameraTargetX = 0f;
            previousKeyboardState = Keyboard.GetState();
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // TODO You should load this from a settings file or similar,
            // based on what the user or operating system selected.
            var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
            LocalizationManager.SetCulture(selectedLanguage);
        }

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            // Создаём небо с солнцем и облаками
            sky = new Sky(GraphicsDevice, screenWidth, screenHeight);
            
            // Создаём систему смены дня и ночи
            dayNightCycle = new DayNightCycle(GraphicsDevice, dayDurationSeconds: 120f);

            // Создаём землю с зелёной лужайкой
            ground = new Ground(GraphicsDevice, screenWidth, screenHeight, 150);
            
            // Загружаем шрифт для UI
            try
            {
                font = Content.Load<SpriteFont>("Fonts/Hud");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки шрифта: {ex.Message}");
                font = null;
            }

            // Создаём систему частиц
            particleSystem = new ParticleSystem(GraphicsDevice);
            if (font != null)
            {
                particleSystem.SetFont(font);
            }
            
            // Создаём менеджер звука
            soundManager = new SoundManager(Content);
            // Звуки будут загружены когда добавим файлы
            
            // Создаём главное меню
            mainMenu = new MainMenu(GraphicsDevice);
            try
            {
                mainMenu.LoadContent(Content);
                mainMenu.OnItemSelected += HandleMenuSelection;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки меню: {ex.Message}");
                // Если меню не загрузилось, сразу переходим в режим игры
                gameState = GameState.Playing;
                if (mainMenu != null)
                {
                    mainMenu.Visible = false;
                }
            }

            // Создаём домик зайчика слева
            rabbitHouse = new House(GraphicsDevice, new Vector2(150, screenHeight - 150), 1f);

            // Создаём зайчика рядом с домиком
            rabbit = new Rabbit(GraphicsDevice, new Vector2(300, screenHeight - 150), screenHeight - 150);
            
            // Создаём систему погоды
            weatherSystem = new WeatherSystem(GraphicsDevice, particleSystem);

            // Создаём деревья с яблоками
            trees = new List<AppleTree>
            {
                new AppleTree(GraphicsDevice, new Vector2(500, screenHeight - 150), 1.2f),
                new AppleTree(GraphicsDevice, new Vector2(900, screenHeight - 150), 1f),
                new AppleTree(GraphicsDevice, new Vector2(1300, screenHeight - 150), 1.1f),
            };

            // Создаём грядки с морковками
            carrotPatches = new List<CarrotPatch>
            {
                new CarrotPatch(GraphicsDevice, new Vector2(700, screenHeight - 120), 1f, 8),
                new CarrotPatch(GraphicsDevice, new Vector2(1100, screenHeight - 120), 1f, 8),
                new CarrotPatch(GraphicsDevice, new Vector2(1500, screenHeight - 120), 1f, 8),
            };
            
            // Создаём врагов
            foxes = new List<Fox>
            {
                new Fox(GraphicsDevice, new Vector2(800, screenHeight - 150), screenHeight - 150, 600, 1000),
                new Fox(GraphicsDevice, new Vector2(1400, screenHeight - 150), screenHeight - 150, 1200, 1600),
            };
            
            crows = new List<Crow>
            {
                new Crow(GraphicsDevice, new Vector2(600, 200), 100, 150),
                new Crow(GraphicsDevice, new Vector2(1200, 180), 120, 130),
            };
        }

        /// <summary>
        /// Updates the game's logic, called once per frame.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            gameTimeTotal += elapsed;
            
            KeyboardState keyboardState = Keyboard.GetState();

            // Exit the game if the Back button (GamePad) or Escape key (Keyboard) is pressed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || keyboardState.IsKeyDown(Keys.Escape))
                Exit();
            
            // Обновляем главное меню
            if (gameState == GameState.Menu)
            {
                mainMenu.Update(gameTime, keyboardState, previousKeyboardState);
            }
            else if (gameState == GameState.Playing)
            {
                // Обновляем зайчика
                rabbit.Update(gameTime, keyboardState, 2000);
                
                // Обновляем камеру (следует за зайчиком)
                cameraTargetX = MathHelper.Clamp(rabbit.Position.X - 640, 0, 1000);
                cameraPosition.X = MathHelper.Lerp(cameraPosition.X, cameraTargetX, elapsed * 5f);
                
                // Обновляем домик
                rabbitHouse.Update(gameTime);
                
                // Обновляем систему частиц
                particleSystem.Update(gameTime);
                
                // Обновляем цикл дня и ночи
                dayNightCycle.Update(gameTime);
                
                // Обновляем погоду
                weatherSystem.Update(gameTime, cameraPosition);
                
                // Обновляем врагов
                foreach (var fox in foxes)
                {
                    fox.Update(gameTime, rabbit.Position);
                }
                
                foreach (var crow in crows)
                {
                    crow.Update(gameTime);
                }
                
                // Проверяем взаимодействие с морковкой
                CheckCarrotCollection();
                
                // Проверяем взаимодействие с домиком
                CheckHouseInteraction(keyboardState);
                
                // Проверяем столкновения с врагами
                CheckEnemyCollisions();
                
                // Обновляем деревья
                foreach (var tree in trees)
                {
                    tree.Update(gameTime);
                }
                
                // Обновляем грядки
                foreach (var carrotPatch in carrotPatches)
                {
                    carrotPatch.Update(gameTime);
                }
            }
            
            // Обновляем небо
            sky.Update(gameTime);
            
            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }
        
        private void HandleMenuSelection(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0: // Играть
                    gameState = GameState.Playing;
                    mainMenu.Visible = false;
                    soundManager?.PlayBackgroundMusic();
                    break;
                case 1: // Настройки
                    // TODO: Открыть настройки
                    break;
                case 2: // Выход
                    Exit();
                    break;
            }
        }
        
        private void CheckCarrotCollection()
        {
            foreach (var carrotPatch in carrotPatches)
            {
                // Простая проверка расстояния до грядки
                float distance = Vector2.Distance(rabbit.Position, carrotPatch.Position);
                if (distance < 60f && !rabbit.HasCarrot)
                {
                    rabbit.CollectCarrot();
                    score += 10;
                    particleSystem.EmitCarrotCollectEffect(rabbit.Position);
                    particleSystem.EmitFloatingText("+10", rabbit.Position - new Vector2(0, 30), Color.Yellow);
                    soundManager?.PlayCollectSound();
                }
            }
        }
        
        private void CheckHouseInteraction(KeyboardState keyboardState)
        {
            float distanceToHouse = Vector2.Distance(rabbit.Position, rabbitHouse.Position);
            
            // Если зайчик рядом с домиком и у него есть морковка
            if (distanceToHouse < rabbitHouse.InteractionRadius && rabbit.HasCarrot)
            {
                // Показывем подсказку
                if (keyboardState.IsKeyDown(Keys.E))
                {
                    rabbit.DropCarrot();
                    carrotsInHouse++;
                    score += 25;
                    rabbitHouse.EmitSmokeEffect();
                    particleSystem.EmitHeartEffect(rabbit.Position);
                    particleSystem.EmitFloatingText("+25", rabbit.Position - new Vector2(0, 30), Color.Lime);
                    soundManager?.PlayDepositSound();
                    
                    // Проверка уровня
                    if (carrotsInHouse >= currentLevel * 5)
                    {
                        currentLevel++;
                        particleSystem.EmitFloatingText($"Уровень {currentLevel}!", rabbit.Position - new Vector2(0, 60), Color.Gold);
                        soundManager?.PlayLevelUpSound();
                    }
                }
            }
        }
        
        private void CheckEnemyCollisions()
        {
            // Проверка столкновений с лисами
            foreach (var fox in foxes)
            {
                float distance = Vector2.Distance(rabbit.Position, fox.Position);
                if (distance < fox.InteractionRadius)
                {
                    // Отталкиваем зайчика
                    float pushDirection = Math.Sign(rabbit.Position.X - fox.Position.X);
                    rabbit = new Rabbit(GraphicsDevice, new Vector2(fox.Position.X + pushDirection * 100, rabbit.Position.Y), rabbit.Position.Y);
                    score = Math.Max(0, score - 5);
                    particleSystem.EmitFloatingText("-5", rabbit.Position - new Vector2(0, 30), Color.Red);
                    soundManager?.PlayHitSound();
                }
            }
            
            // Проверка столкновений с воронами
            foreach (var crow in crows)
            {
                float distance = Vector2.Distance(rabbit.Position, crow.Position);
                if (distance < crow.InteractionRadius)
                {
                    // Ворона мешает, но не отнимает очки
                    particleSystem.EmitFloatingText("Кар!", crow.Position, Color.Gray);
                }
            }
        }

        /// <summary>
        /// Draws the game's graphics, called once per frame.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Очищаем экран голубым цветом неба
            GraphicsDevice.Clear(new Color(135, 206, 235));

            if (gameState == GameState.Menu)
            {
                // Рисуем только меню
                spriteBatch.Begin();
                mainMenu?.Draw(spriteBatch);
                spriteBatch.End();
            }
            else if (gameState == GameState.Playing)
            {
                // Применяем камеру через матрицу трансформации
                Matrix transformMatrix = Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: transformMatrix);

                // Рисуем небо с солнцем и облаками
                sky.Draw(spriteBatch);
                
                // Рисуем цикл дня и ночи
                dayNightCycle.Draw(spriteBatch);
                
                // Рисуем погоду
                weatherSystem.Draw(spriteBatch);

                // Рисуем деревья (на заднем плане)
                foreach (var tree in trees)
                {
                    tree.Draw(spriteBatch);
                }

                // Рисуем землю с зелёной лужайкой
                ground.Draw(spriteBatch);
                
                // Рисуем домик
                rabbitHouse.Draw(spriteBatch);

                // Рисуем грядки с морковками (поверх земли)
                foreach (var carrotPatch in carrotPatches)
                {
                    carrotPatch.Draw(spriteBatch);
                }
                
                // Рисуем врагов
                foreach (var fox in foxes)
                {
                    fox.Draw(spriteBatch);
                }
                
                foreach (var crow in crows)
                {
                    crow.Draw(spriteBatch);
                }
                
                // Рисуем зайчика
                rabbit.Draw(spriteBatch);
                
                // Рисуем систему частиц
                particleSystem.Draw(spriteBatch);

                spriteBatch.End();
                
                // Рисуем UI (без камеры)
                spriteBatch.Begin();
                DrawUI();
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
        
        private void DrawUI()
        {
            if (font == null) return;
            
            // Счёт
            string scoreText = $"Score: {score}";
            spriteBatch.DrawString(font, scoreText, new Vector2(15, 15), Color.Black);
            spriteBatch.DrawString(font, scoreText, new Vector2(13, 13), Color.White);
            
            // Морковки в домике
            string carrotsText = $"Carrots: {carrotsInHouse}";
            spriteBatch.DrawString(font, carrotsText, new Vector2(15, 45), Color.Black);
            spriteBatch.DrawString(font, carrotsText, new Vector2(13, 43), Color.White);
            
            // Уровень
            string levelText = $"Level: {currentLevel}";
            spriteBatch.DrawString(font, levelText, new Vector2(15, 75), Color.Black);
            spriteBatch.DrawString(font, levelText, new Vector2(13, 73), Color.White);
            
            // Подсказка
            float distanceToHouse = Vector2.Distance(rabbit.Position, rabbitHouse.Position);
            if (distanceToHouse < rabbitHouse.InteractionRadius && rabbit.HasCarrot)
            {
                string hint = "Press E to deposit carrot";
                Vector2 hintSize = font.MeasureString(hint);
                spriteBatch.DrawString(font, hint, new Vector2(640 - hintSize.X / 2 + 2, 650 + 2), Color.Black);
                spriteBatch.DrawString(font, hint, new Vector2(640 - hintSize.X / 2, 650), Color.Yellow);
            }
            
            // Управление
            string controls = "A/D - Move | Space - Jump | E - Interact";
            Vector2 controlsSize = font.MeasureString(controls);
            spriteBatch.DrawString(font, controls, new Vector2(640 - controlsSize.X / 2 + 2, 700 + 2), Color.Black);
            spriteBatch.DrawString(font, controls, new Vector2(640 - controlsSize.X / 2, 700), Color.White);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sky?.Dispose();
                ground?.Dispose();
                rabbit?.Dispose();
                rabbitHouse?.Dispose();
                particleSystem?.Dispose();
                dayNightCycle?.Dispose();
                weatherSystem?.Dispose();
                mainMenu?.Dispose();
                soundManager?.Dispose();
                spriteBatch?.Dispose();

                foreach (var tree in trees)
                {
                    // AppleTree не реализует IDisposable, но его листья и яблоки имеют текстуры
                }
                
                foreach (var fox in foxes)
                {
                    fox?.Dispose();
                }
                
                foreach (var crow in crows)
                {
                    crow?.Dispose();
                }

                foreach (var carrotPatch in carrotPatches)
                {
                    carrotPatch?.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
    
    /// <summary>
    /// Состояния игры.
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }
}