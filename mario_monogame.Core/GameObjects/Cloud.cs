using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Пушистое белое облако, которое перемещается по небу.
    /// </summary>
    public class Cloud
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Vector2 _position;
        private readonly float _speed;
        private readonly float _scale;
        private readonly List<CloudPuff> _puffs;
        private float _floatOffset;

        private struct CloudPuff
        {
            public Vector2 Offset;
            public float Radius;
        }

        public Cloud(GraphicsDevice graphicsDevice, Vector2 startPosition, float speed, float scale = 1f)
        {
            _graphicsDevice = graphicsDevice;
            _position = startPosition;
            _speed = speed;
            _scale = scale;
            _floatOffset = 0f;
            
            // Создаём пуфы для облака
            _puffs = new List<CloudPuff>
            {
                new CloudPuff { Offset = new Vector2(0, 0), Radius = 30f },
                new CloudPuff { Offset = new Vector2(-25f, 5f), Radius = 25f },
                new CloudPuff { Offset = new Vector2(25f, 5f), Radius = 25f },
                new CloudPuff { Offset = new Vector2(-45f, 15f), Radius = 18f },
                new CloudPuff { Offset = new Vector2(45f, 15f), Radius = 18f },
                new CloudPuff { Offset = new Vector2(-15f, -10f), Radius = 20f },
                new CloudPuff { Offset = new Vector2(15f, -10f), Radius = 20f },
            };
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Движение вправо
            _position.X += _speed * elapsed;
            
            // Лёгкое покачивание вверх-вниз
            _floatOffset += elapsed * 2f;
            float floatY = (float)Math.Sin(_floatOffset) * 3f;
            
            // Если облако ушло за экран, перемещаем его влево
            if (_position.X > 1200)
            {
                _position.X = -200;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float floatY = (float)Math.Sin(_floatOffset) * 3f;
            Vector2 drawPosition = _position + new Vector2(0, floatY);

            // Рисуем каждый пуф облака
            foreach (var puff in _puffs)
            {
                DrawPuff(spriteBatch, drawPosition + puff.Offset * _scale, puff.Radius * _scale);
            }
        }

        private void DrawPuff(SpriteBatch spriteBatch, Vector2 position, float radius)
        {
            int segments = 20;

            // Рисуем пуф как набор треугольников от центра
            for (int i = 0; i < segments; i++)
            {
                double angle1 = i * Math.PI * 2 / segments;
                double angle2 = (i + 1) * Math.PI * 2 / segments;

                Vector2 point1 = new Vector2(
                    position.X + (float)Math.Cos(angle1) * radius,
                    position.Y + (float)Math.Sin(angle1) * radius
                );
                Vector2 point2 = new Vector2(
                    position.X + (float)Math.Cos(angle2) * radius,
                    position.Y + (float)Math.Sin(angle2) * radius
                );

                // Белый цвет с лёгкой прозрачностью по краям
                Color puffColor = Color.White;

                DrawFilledTriangle(spriteBatch, position, point1, point2, puffColor);
            }

            // Добавляем яркую середину для объёма
            using var highlightTexture = new Texture2D(_graphicsDevice, 1, 1);
            highlightTexture.SetData(new[] { Color.White });
            
            spriteBatch.Draw(
                highlightTexture,
                position - new Vector2(0, radius * 0.3f),
                null,
                new Color(255, 255, 255, 200),
                0f,
                Vector2.Zero,
                radius * 0.4f,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawFilledTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            // Рисуем заполненный треугольник
            float minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            float maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            float minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            float maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));

            using var pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { color });

            // Простой алгоритм заполнения треугольника
            for (float x = minX; x <= maxX; x += 2f)
            {
                for (float y = minY; y <= maxY; y += 2f)
                {
                    if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                    {
                        spriteBatch.Draw(pixelTexture, new Vector2(x, y), color);
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
