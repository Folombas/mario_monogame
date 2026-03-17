using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Земля с зелёной лужайкой и текстурой травы.
    /// </summary>
    public class Ground
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly int _groundHeight;
        private Texture2D _dirtTexture;
        private Texture2D _grassTopTexture;

        public int GroundHeight => _groundHeight;

        public Ground(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight, int groundHeight = 150)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _groundHeight = groundHeight;

            CreateGroundTextures();
        }

        private void CreateGroundTextures()
        {
            // Создаём текстуру травы (верхний слой)
            _grassTopTexture = new Texture2D(_graphicsDevice, _screenWidth, 30);
            Color[] grassTopPixels = new Color[_screenWidth * 30];

            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < _screenWidth; x++)
                {
                    int index = y * _screenWidth + x;
                    
                    // Создаём эффект травы с неровным верхним краем
                    float noise = GetNoise(x, y);
                    
                    if (y < 5 + noise * 3)
                    {
                        // Отдельные травинки наверху
                        grassTopPixels[index] = new Color(100 + (byte)(noise * 50), 180, 50, 255);
                    }
                    else if (y < 15)
                    {
                        // Основная зелёная часть травы
                        byte green = (byte)(140 + noise * 40);
                        byte red = (byte)(80 + noise * 30);
                        grassTopPixels[index] = new Color(red, green, 40);
                    }
                    else
                    {
                        // Переход к земле
                        float t = (y - 15) / 15f;
                        byte green = (byte)(140 - t * 80);
                        byte red = (byte)(80 + t * 40);
                        byte brown = (byte)(60 + t * 40);
                        grassTopPixels[index] = new Color(red, green, brown);
                    }
                }
            }
            _grassTopTexture.SetData(grassTopPixels);

            // Создаём текстуру земли (нижний слой)
            _dirtTexture = new Texture2D(_graphicsDevice, _screenWidth, _groundHeight - 30);
            Color[] dirtPixels = new Color[_screenWidth * (_groundHeight - 30)];

            for (int y = 0; y < _groundHeight - 30; y++)
            {
                for (int x = 0; x < _screenWidth; x++)
                {
                    int index = y * _screenWidth + x;
                    float noise = GetNoise(x * 3, y * 3);
                    
                    // Коричневая земля с текстурой
                    byte baseBrown = (byte)(101 + noise * 30);
                    byte dirtR = (byte)(baseBrown + 20);
                    byte dirtG = (byte)(67 + noise * 20);
                    byte dirtB = (byte)(33 + noise * 10);
                    
                    dirtPixels[index] = new Color(dirtR, dirtG, dirtB);
                }
            }
            _dirtTexture.SetData(dirtPixels);
        }

        private float GetNoise(int x, int y)
        {
            // Простая псевдослучайная функция для текстуры
            int hash = x * 73856093 ^ y * 19349663;
            hash = (hash >> 13) ^ hash;
            return ((hash * 15731 + 789221) & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int groundY = _screenHeight - _groundHeight;

            // Рисуем землю
            spriteBatch.Draw(_dirtTexture, new Vector2(0, groundY + 30), Color.White);

            // Рисуем траву сверху
            spriteBatch.Draw(_grassTopTexture, new Vector2(0, groundY), Color.White);

            // Рисуем декоративные травинки по верхнему краю
            DrawGrassBlades(spriteBatch, groundY);
        }

        private void DrawGrassBlades(SpriteBatch spriteBatch, int groundY)
        {
            using var bladeTexture = new Texture2D(_graphicsDevice, 1, 1);
            bladeTexture.SetData(new[] { Color.Green });

            for (int x = 0; x < _screenWidth; x += 8)
            {
                float noise = GetNoise(x, 0);
                int bladeHeight = 5 + (int)(noise * 8);
                float angle = (noise - 0.5f) * 0.3f;

                spriteBatch.Draw(
                    bladeTexture,
                    new Vector2(x, groundY),
                    null,
                    new Color(50, 150, 30),
                    angle,
                    new Vector2(0.5f, 1f),
                    new Vector2(3, bladeHeight),
                    SpriteEffects.None,
                    0f
                );
            }
        }

        public void Dispose()
        {
            _grassTopTexture?.Dispose();
            _dirtTexture?.Dispose();
        }
    }
}
