using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Голубое небо с солнцем и облаками.
    /// </summary>
    public class Sky
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Sun _sun;
        private readonly List<Cloud> _clouds;
        private Texture2D _gradientTexture;

        public Sky(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight)
        {
            _graphicsDevice = graphicsDevice;
            
            // Создаём солнце в правом верхнем углу
            _sun = new Sun(graphicsDevice, new Vector2(screenWidth - 100, 100), 50f);
            
            // Создаём несколько облаков на разных высотах и скоростях
            _clouds = new List<Cloud>
            {
                new Cloud(graphicsDevice, new Vector2(100, 80), 20f, 0.8f),
                new Cloud(graphicsDevice, new Vector2(400, 150), 15f, 1f),
                new Cloud(graphicsDevice, new Vector2(700, 60), 25f, 0.7f),
                new Cloud(graphicsDevice, new Vector2(200, 200), 18f, 0.9f),
            };

            // Создаём текстуру градиента для неба
            CreateSkyGradient(screenWidth, screenHeight);
        }

        private void CreateSkyGradient(int width, int height)
        {
            _gradientTexture = new Texture2D(_graphicsDevice, 1, height);
            Color[] pixels = new Color[height];

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                
                // Градиент от светло-голубого (вверху) к более светлому (внизу)
                byte r = (byte)(135 + t * 70);  // 135 -> 205
                byte g = (byte)(206 + t * 44);  // 206 -> 250
                byte b = (byte)(235 + t * 20);  // 235 -> 255
                
                pixels[y] = new Color(r, g, b);
            }

            _gradientTexture.SetData(pixels);
        }

        public void Update(GameTime gameTime)
        {
            _sun.Update(gameTime);
            
            foreach (var cloud in _clouds)
            {
                cloud.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем градиентное небо
            spriteBatch.Draw(_gradientTexture, Vector2.Zero, Color.White);

            // Рисуем солнце
            _sun.Draw(spriteBatch);

            // Рисуем облака
            foreach (var cloud in _clouds)
            {
                cloud.Draw(spriteBatch);
            }
        }

        public void Dispose()
        {
            _gradientTexture?.Dispose();
        }
    }
}
