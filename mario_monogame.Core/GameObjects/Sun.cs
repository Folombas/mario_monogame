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
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Vector2 _position;
        private readonly float _radius;
        private readonly int _rayCount;
        private readonly float _rayLength;
        private float _rotationAngle;

        public Vector2 Position => _position;
        public float Radius => _radius;

        public Sun(GraphicsDevice graphicsDevice, Vector2 position, float radius = 40f)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _radius = radius;
            _rayCount = 16;
            _rayLength = radius * 1.8f;
            _rotationAngle = 0f;
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
            int raySteps = 8; // Количество сегментов в каждом луче

            for (int i = 0; i < _rayCount; i++)
            {
                float angle = (float)(i * Math.PI * 2 / _rayCount) + _rotationAngle;

                // Создаём луч из сегментов с градиентом
                for (int step = 0; step < raySteps; step++)
                {
                    float stepRatio = (float)step / raySteps;
                    float currentRadius = _radius + stepRatio * (_rayLength - _radius);
                    
                    // Градиент от жёлтого к прозрачному
                    float alpha = 1f - stepRatio * 0.7f;
                    Color rayColor = new Color((byte)255, (byte)220, (byte)100, (byte)(alpha * 255));

                    float x = _position.X + (float)Math.Cos(angle) * currentRadius;
                    float y = _position.Y + (float)Math.Sin(angle) * currentRadius;

                    // Рисуем сегмент луча
                    DrawGradientRay(spriteBatch, _position, new Vector2(x, y), rayColor, stepRatio * 15f);
                }
            }
        }

        private void DrawGradientRay(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);

            // Создаём текстуру луча
            using var rayTexture = new Texture2D(_graphicsDevice, 1, 1);
            rayTexture.SetData(new[] { color });

            spriteBatch.Draw(
                rayTexture,
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

        private void DrawSunCircle(SpriteBatch spriteBatch)
        {
            // Рисуем основной круг солнца с градиентом
            int segments = 36;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(i * Math.PI * 2 / segments);
                float angle2 = (float)((i + 1) * Math.PI * 2 / segments);

                Vector2 point1 = new Vector2(
                    _position.X + (float)Math.Cos(angle1) * _radius,
                    _position.Y + (float)Math.Sin(angle1) * _radius
                );
                Vector2 point2 = new Vector2(
                    _position.X + (float)Math.Cos(angle2) * _radius,
                    _position.Y + (float)Math.Sin(angle2) * _radius
                );

                // Градиент от ярко-жёлтого к оранжевому
                Color sunColor = i < segments / 2 
                    ? new Color(255, 255, 100) 
                    : new Color(255, 200, 50);

                using var triangleTexture = new Texture2D(_graphicsDevice, 1, 1);
                triangleTexture.SetData(new[] { sunColor });

                // Рисуем треугольник от центра к краям
                DrawTriangle(spriteBatch, _position, point1, point2, sunColor);
            }

            // Яркий центр солнца
            using var centerTexture = new Texture2D(_graphicsDevice, 1, 1);
            centerTexture.SetData(new[] { Color.White });
            spriteBatch.Draw(
                centerTexture,
                _position,
                null,
                Color.White,
                0f,
                new Vector2(0.5f),
                _radius * 0.3f,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            // Рисуем треугольник как набор линий
            DrawLine(spriteBatch, p1, p2, color, 2f);
            DrawLine(spriteBatch, p2, p3, color, 2f);
            DrawLine(spriteBatch, p3, p1, color, 2f);

            // Заполняем треугольник
            float avgX = (p1.X + p2.X + p3.X) / 3f;
            float avgY = (p1.Y + p2.Y + p3.Y) / 3f;

            using var fillTexture = new Texture2D(_graphicsDevice, 1, 1);
            fillTexture.SetData(new[] { color });

            // Рисуем линии от центра к вершинам для заполнения
            DrawLine(spriteBatch, new Vector2(avgX, avgY), p1, color, 3f);
            DrawLine(spriteBatch, new Vector2(avgX, avgY), p2, color, 3f);
            DrawLine(spriteBatch, new Vector2(avgX, avgY), p3, color, 3f);
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);

            using var lineTexture = new Texture2D(_graphicsDevice, 1, 1);
            lineTexture.SetData(new[] { color });

            spriteBatch.Draw(
                lineTexture,
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
    }
}
