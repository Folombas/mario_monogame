using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.UI
{
    /// <summary>
    /// Главное меню игры с кнопками и анимацией.
    /// </summary>
    public class MainMenu : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Texture2D _pixelTexture;
        private SpriteFont _font;
        private SpriteFont _titleFont;
        
        private List<MenuItem> _menuItems;
        private int _selectedIndex;
        private float _animationTime;
        private float _titleBounce;
        
        private Vector2 _carrotPosition;
        private float _carrotAngle;
        
        public event Action<int> OnItemSelected;
        
        public bool Visible { get; set; } = true;

        public MainMenu(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _menuItems = new List<MenuItem>();
            _selectedIndex = 0;
            _animationTime = 0f;
            _titleBounce = 0f;
            
            _carrotPosition = new Vector2(640, 200);
            _carrotAngle = 0f;
        }
        
        public void LoadContent(ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("Fonts/Hud");
                _titleFont = _font;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки шрифта: {ex.Message}");
                // Шрифт не загрузился, меню не будет работать
                return;
            }
            
            // Создаём пункты меню
            _menuItems = new List<MenuItem>
            {
                new MenuItem("Play Game", new Vector2(640, 350)),
                new MenuItem("Settings", new Vector2(640, 420)),
                new MenuItem("Exit", new Vector2(640, 490))
            };
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            if (!Visible) return;
            
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _animationTime += elapsed;
            _titleBounce = (float)Math.Sin(_animationTime * 3f) * 5f;
            _carrotAngle += elapsed * 2f;
            
            // Навигация клавиатурой
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                if (previousKeyboardState.IsKeyUp(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.S))
                {
                    _selectedIndex = (_selectedIndex + 1) % _menuItems.Count;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                if (previousKeyboardState.IsKeyUp(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.W))
                {
                    _selectedIndex = (_selectedIndex - 1 + _menuItems.Count) % _menuItems.Count;
                }
            }
            
            // Выбор
            if (keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space))
            {
                if (previousKeyboardState.IsKeyUp(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    OnItemSelected?.Invoke(_selectedIndex);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            
            // Рисуем фон меню
            DrawBackground(spriteBatch);
            
            // Рисуем заголовок
            DrawTitle(spriteBatch);
            
            // Рисуем декоративную морковку
            DrawCarrot(spriteBatch);
            
            // Рисуем пункты меню
            DrawMenuItems(spriteBatch);
            
            // Рисуем подсказки
            DrawHints(spriteBatch);
        }
        
        private void DrawBackground(SpriteBatch spriteBatch)
        {
            // Градиентный фон
            for (int y = 0; y < 720; y += 10)
            {
                float t = y / 720f;
                Color color = new Color(
                    (byte)(50 + t * 50),
                    (byte)(50 + t * 30),
                    (byte)(100 + t * 50)
                );
                
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(0, y, 1280, 10),
                    color
                );
            }
        }
        
        private void DrawTitle(SpriteBatch spriteBatch)
        {
            string title = "Happy Carrot";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(640 - titleSize.X / 2, 100 + _titleBounce);
            
            // Тень
            spriteBatch.DrawString(_font, title, titlePos + new Vector2(3, 3), Color.Black);
            // Текст
            spriteBatch.DrawString(_font, title, titlePos, new Color(255, 220, 100));
            
            // Подзаголовок
            string subtitle = "Rabbit farmer adventures";
            Vector2 subtitleSize = _font.MeasureString(subtitle);
            Vector2 subtitlePos = new Vector2(640 - subtitleSize.X / 2, 150);
            spriteBatch.DrawString(_font, subtitle, subtitlePos, Color.LightGray);
        }
        
        private void DrawCarrot(SpriteBatch spriteBatch)
        {
            float carrotX = _carrotPosition.X + (float)Math.Sin(_carrotAngle) * 30f;
            float carrotY = _carrotPosition.Y + (float)Math.Cos(_carrotAngle * 0.5f) * 10f;
            
            // Рисуем морковку
            DrawCarrotShape(spriteBatch, new Vector2(carrotX, carrotY), _carrotAngle);
        }
        
        private void DrawCarrotShape(SpriteBatch spriteBatch, Vector2 position, float rotation)
        {
            // Тело морковки
            spriteBatch.Draw(
                _pixelTexture,
                position,
                null,
                new Color(255, 140, 0),
                rotation,
                new Vector2(10, 15),
                new Vector2(10, 25),
                SpriteEffects.None,
                0f
            );
            
            // Ботва
            for (int i = 0; i < 3; i++)
            {
                float leafAngle = rotation + (i - 1) * 0.5f;
                spriteBatch.Draw(
                    _pixelTexture,
                    position + new Vector2(0, -15),
                    null,
                    new Color(34, 139, 34),
                    leafAngle,
                    Vector2.Zero,
                    new Vector2(3, 15),
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private void DrawMenuItems(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _menuItems.Count; i++)
            {
                var item = _menuItems[i];
                bool isSelected = i == _selectedIndex;
                
                // Фон выбранного пункта
                if (isSelected)
                {
                    float pulse = (float)Math.Sin(_animationTime * 5f) * 0.1f + 0.9f;
                    spriteBatch.Draw(
                        _pixelTexture,
                        new Rectangle((int)(item.Position.X - 100), (int)(item.Position.Y - 20), 200, 40),
                        new Color((byte)255, (byte)200, (byte)100, (byte)(pulse * 100))
                    );
                }

                // Текст
                Color textColor = isSelected ? new Color((byte)255, (byte)220, (byte)100) : Color.White;
                Vector2 textSize = _font.MeasureString(item.Text);
                Vector2 textPos = new Vector2(item.Position.X - textSize.X / 2, item.Position.Y);

                // Маркер выбора
                if (isSelected)
                {
                    string marker = "> ";
                    spriteBatch.DrawString(_font, marker, textPos - new Vector2(30, 0), textColor);
                }

                spriteBatch.DrawString(_font, item.Text, textPos, textColor);
            }
        }
        
        private void DrawHints(SpriteBatch spriteBatch)
        {
            string hint1 = "Up/Down or W/S - Navigate";
            string hint2 = "Enter or Space - Select";

            Vector2 hint1Size = _font.MeasureString(hint1);
            Vector2 hint2Size = _font.MeasureString(hint2);

            spriteBatch.DrawString(_font, hint1, new Vector2(640 - hint1Size.X / 2, 600), Color.Gray);
            spriteBatch.DrawString(_font, hint2, new Vector2(640 - hint2Size.X / 2, 630), Color.Gray);

            // Копирайт
            string copyright = "2024 Mario Monogame";
            Vector2 copyrightSize = _font.MeasureString(copyright);
            spriteBatch.DrawString(_font, copyright, new Vector2(640 - copyrightSize.X / 2, 690), new Color(100, 100, 100));
        }
        
        public void SetFont(SpriteFont font)
        {
            _font = font;
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
        
        private class MenuItem
        {
            public string Text { get; set; }
            public Vector2 Position { get; set; }
            
            public MenuItem(string text, Vector2 position)
            {
                Text = text;
                Position = position;
            }
        }
    }
}
