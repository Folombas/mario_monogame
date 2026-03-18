using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using mario_monogame.Core.Extensions;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Голубое небо с солнцем, луной и облаками с параллаксом.
    /// </summary>
    public class Sky
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Sun _sun;
        private readonly List<Cloud> _clouds;
        private Texture2D _gradientTexture;
        
        // Параллакс слои
        private readonly List<ParallaxCloud> _parallaxClouds;
        private float _cameraX;

        public Sky(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight)
        {
            _graphicsDevice = graphicsDevice;

            // Создаём солнце в правом верхнем углу
            _sun = new Sun(graphicsDevice, new Vector2(screenWidth - 100, 100), 50f);
            
            // Создаём несколько слоёв облаков для параллакса
            _parallaxClouds = new List<ParallaxCloud>();
            CreateParallaxClouds(screenWidth, screenHeight);

            // Создаём несколько обычных облаков
            _clouds = new List<Cloud>
            {
                new Cloud(graphicsDevice, new Vector2(100, 80), 15f, 0.8f),
                new Cloud(graphicsDevice, new Vector2(400, 150), 12f, 1f),
                new Cloud(graphicsDevice, new Vector2(700, 60), 18f, 0.7f),
            };

            // Создаём текстуру градиента для неба
            CreateSkyGradient(screenWidth, screenHeight);
        }
        
        private void CreateParallaxClouds(int screenWidth, int screenHeight)
        {
            var random = new Random();
            
            // Дальний слой (медленные)
            for (int i = 0; i < 8; i++)
            {
                _parallaxClouds.Add(new ParallaxCloud
                {
                    X = random.Next(screenWidth * 2),
                    Y = random.Next(50, 150),
                    Speed = 10f + random.Next(10),
                    Scale = 0.5f + (float)random.NextDouble() * 0.3f,
                    ParallaxFactor = 0.2f
                });
            }
            
            // Средний слой
            for (int i = 0; i < 5; i++)
            {
                _parallaxClouds.Add(new ParallaxCloud
                {
                    X = random.Next(screenWidth * 2),
                    Y = random.Next(100, 200),
                    Speed = 15f + random.Next(15),
                    Scale = 0.7f + (float)random.NextDouble() * 0.4f,
                    ParallaxFactor = 0.5f
                });
            }
            
            // Ближний слой (быстрые)
            for (int i = 0; i < 3; i++)
            {
                _parallaxClouds.Add(new ParallaxCloud
                {
                    X = random.Next(screenWidth * 2),
                    Y = random.Next(30, 100),
                    Speed = 20f + random.Next(20),
                    Scale = 0.9f + (float)random.NextDouble() * 0.3f,
                    ParallaxFactor = 0.8f
                });
            }
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

        public void Update(GameTime gameTime, float cameraX)
        {
            _cameraX = cameraX;
            _sun.Update(gameTime);
            
            // Обновляем параллакс облака
            foreach (var cloud in _parallaxClouds)
            {
                cloud.Update(gameTime, cameraX);
            }

            foreach (var cloud in _clouds)
            {
                cloud.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, float cameraX = 0f)
        {
            // Рисуем градиентное небо
            spriteBatch.Draw(_gradientTexture, Vector2.Zero, Color.White);

            // Рисуем дальние облака (параллакс)
            foreach (var cloud in _parallaxClouds)
            {
                cloud.Draw(spriteBatch, cameraX);
            }

            // Рисуем солнце
            _sun.Draw(spriteBatch);

            // Рисуем обычные облака
            foreach (var cloud in _clouds)
            {
                cloud.Draw(spriteBatch);
            }
        }

        public void Dispose()
        {
            _gradientTexture?.Dispose();
            _sun?.Dispose();
        }
        
        private class ParallaxCloud
        {
            public float X;
            public float Y;
            public float Speed;
            public float Scale;
            public float ParallaxFactor;
            private float _floatOffset;
            
            public void Update(GameTime gameTime, float cameraX)
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Движение с учётом параллакса
                X += Speed * elapsed;
                
                // Покачивание вверх-вниз
                _floatOffset += elapsed * 2f;
                
                // Зацикливание
                if (X > cameraX + 1400)
                {
                    X = cameraX - 200;
                }
            }
            
            public void Draw(SpriteBatch spriteBatch, float cameraX)
            {
                float floatY = (float)Math.Sin(_floatOffset) * 3f;
                Vector2 drawPosition = new Vector2(X, Y + floatY);
                
                // Рисуем облако из нескольких кругов
                DrawCloud(spriteBatch, drawPosition, Scale);
            }
            
            private void DrawCloud(SpriteBatch spriteBatch, Vector2 position, float scale)
            {
                Color cloudColor = new Color(255, 255, 255, 200);
                
                // Основной круг
                DrawCircle(spriteBatch, position, 30f * scale, cloudColor);
                
                // Дополнительные круги
                DrawCircle(spriteBatch, position + new Vector2(-25f, 5f) * scale, 25f * scale, cloudColor);
                DrawCircle(spriteBatch, position + new Vector2(25f, 5f) * scale, 25f * scale, cloudColor);
                DrawCircle(spriteBatch, position + new Vector2(-45f, 15f) * scale, 18f * scale, cloudColor);
                DrawCircle(spriteBatch, position + new Vector2(45f, 15f) * scale, 18f * scale, cloudColor);
            }
            
            private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
            {
                int segments = 24;
                Texture2D pixel = spriteBatch.GraphicsDevice.GetWhitePixel();
                
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = (float)(i * Math.PI * 2 / segments);
                    float angle2 = (float)((i + 1) * Math.PI * 2 / segments);
                    
                    Vector2 point1 = new Vector2(
                        center.X + (float)Math.Cos(angle1) * radius,
                        center.Y + (float)Math.Sin(angle1) * radius
                    );
                    Vector2 point2 = new Vector2(
                        center.X + (float)Math.Cos(angle2) * radius,
                        center.Y + (float)Math.Sin(angle2) * radius
                    );
                    
                    DrawTriangle(spriteBatch, center, point1, point2, color, pixel);
                }
            }
            
            private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color, Texture2D pixel)
            {
                float minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
                float maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
                float minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
                float maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
                
                for (float x = minX; x <= maxX; x += 3f)
                {
                    for (float y = minY; y <= maxY; y += 3f)
                    {
                        if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                        {
                            spriteBatch.Draw(pixel, new Vector2(x, y), color);
                        }
                    }
                }
            }
            
            private bool IsPointInTriangle(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3)
            {
                float sign(Vector2 a, Vector2 b, Vector2 c)
                {
                    return (a.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (a.Y - c.Y);
                }
                
                bool b1 = sign(point, p1, p2) < 0.0f;
                bool b2 = sign(point, p2, p3) < 0.0f;
                bool b3 = sign(point, p3, p1) < 0.0f;
                
                return (b1 == b2) && (b2 == b3);
            }
        }
    }
}
