using mario_monogame.Core.GameObjects;
using mario_monogame.Core.Localization;
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

            // Создаём землю с зелёной лужайкой
            ground = new Ground(GraphicsDevice, screenWidth, screenHeight, 150);

            // Создаём деревья с яблоками
            trees = new List<AppleTree>
            {
                new AppleTree(GraphicsDevice, new Vector2(200, screenHeight - 150), 1.2f),
                new AppleTree(GraphicsDevice, new Vector2(600, screenHeight - 150), 1f),
                new AppleTree(GraphicsDevice, new Vector2(950, screenHeight - 150), 1.1f),
            };

            // Создаём грядки с морковками
            carrotPatches = new List<CarrotPatch>
            {
                new CarrotPatch(GraphicsDevice, new Vector2(400, screenHeight - 120), 1f, 8),
                new CarrotPatch(GraphicsDevice, new Vector2(750, screenHeight - 120), 1f, 8),
            };
        }

        /// <summary>
        /// Updates the game's logic, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for game updates.
        /// </param>
        protected override void Update(GameTime gameTime)
        {
            // Exit the game if the Back button (GamePad) or Escape key (Keyboard) is pressed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Обновляем игровые объекты
            sky.Update(gameTime);

            foreach (var tree in trees)
            {
                tree.Update(gameTime);
            }

            foreach (var carrotPatch in carrotPatches)
            {
                carrotPatch.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game's graphics, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for rendering.
        /// </param>
        protected override void Draw(GameTime gameTime)
        {
            // Очищаем экран голубым цветом неба
            GraphicsDevice.Clear(new Color(135, 206, 235));

            spriteBatch.Begin();

            // Рисуем небо с солнцем и облаками
            sky.Draw(spriteBatch);

            // Рисуем деревья (на заднем плане)
            foreach (var tree in trees)
            {
                tree.Draw(spriteBatch);
            }

            // Рисуем землю с зелёной лужайкой
            ground.Draw(spriteBatch);

            // Рисуем грядки с морковками (поверх земли)
            foreach (var carrotPatch in carrotPatches)
            {
                carrotPatch.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sky?.Dispose();
                ground?.Dispose();
                spriteBatch?.Dispose();

                foreach (var tree in trees)
                {
                    // AppleTree не реализует IDisposable, но его листья и яблоки имеют текстуры
                }

                foreach (var carrotPatch in carrotPatches)
                {
                    carrotPatch?.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}