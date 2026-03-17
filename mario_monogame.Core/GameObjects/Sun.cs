using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Солнце с лучами, которое испускает свет.
    /// </summary>
    public class Sun
    {
        private readonly Vector2 _position;
        private readonly float _radius;
        private readonly int _rayCount;
        private readonly float _rayLength;
        private float _rotationAngle;
        private Texture2D _pixelTexture;

        public Vector2 Position => _position;
        public float Radius => _radius;

        public Sun(GraphicsDevice graphicsDevice, Vector2 position, float radius = 40f)
        {
            _position = position;
            _radius = radius;
            _rayCount = 16;
            _rayLength = radius * 1.8f;
            _rotationAngle = 0f;

            // Создаём белый пиксель для отрисовки
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            // Медленное вращение лучей
            _rotationAngle += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем лучи солнца
            DrawRays(spriteBatch);

            // Рисуем основной круг солнца
            DrawSunCircle(spriteBatch);
        }

        private void DrawRays(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _rayCount; i++)
            {
                float angle = (float)(i * Math.PI * 2 / _rayCount) + _rotationAngle;

                // Рисуем луч
                float x = _position.X + (float)Math.Cos(angle) * _rayLength;
                float y = _position.Y + (float)Math.Sin(angle) * _rayLength;

                DrawLine(spriteBatch, _position, new Vector2(x, y), new Color(255, 220, 100, 180), 8f);
            }
        }

        private void DrawSunCircle(SpriteBatch spriteBatch)
        {
            // Рисуем основной круг солнца
            DrawCircle(spriteBatch, _position, _radius, new Color(255, 255, 100));
            
            // Яркий центр
            DrawCircle(spriteBatch, _position, _radius * 0.5f, new Color(255, 255, 200));
            
            // Белый центр
            DrawCircle(spriteBatch, _position, _radius * 0.2f, Color.White);
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            int segments = 36;

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

                DrawTriangle(spriteBatch, center, point1, point2, color);
            }
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            // Находим границы треугольника
            float minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            float maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            float minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            float maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));

            // Заполняем треугольник пикселями
            for (float x = minX; x <= maxX; x += 1f)
            {
                for (float y = minY; y <= maxY; y += 1f)
                {
                    if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                    {
                        spriteBatch.Draw(_pixelTexture, new Vector2(x, y), color);
                    }
                }
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);

            spriteBatch.Draw(
                _pixelTexture,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f
            );
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

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }
}
