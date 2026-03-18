using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Грядка с морковками.
    /// </summary>
    public class CarrotPatch
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Vector2 _position;
        private readonly float _scale;
        private readonly List<Carrot> _carrots;
        private Texture2D _pixelTexture;
        private Texture2D _soilTexture;

        public Vector2 Position => _position;

        public CarrotPatch(GraphicsDevice graphicsDevice, Vector2 position, float scale = 1f, int carrotCount = 8)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _scale = scale;

            // Создаём текстуры
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            CreateSoilTexture();

            // Создаём морковки
            _carrots = new List<Carrot>();
            for (int i = 0; i < carrotCount; i++)
            {
                float x = (i % 4) * 40 - 60;
                float y = (i / 4) * 50 - 25;
                _carrots.Add(new Carrot(graphicsDevice, _position + new Vector2(x, y), scale));
            }
        }

        private void CreateSoilTexture()
        {
            int width = 200;
            int height = 80;
            _soilTexture = new Texture2D(_graphicsDevice, width, height);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = GetNoise(x, y);
                    byte brown = (byte)(101 + noise * 40);
                    pixels[y * width + x] = new Color(brown, (byte)(67 + noise * 20), (byte)(33 + noise * 10));
                }
            }
            _soilTexture.SetData(pixels);
        }

        private float GetNoise(int x, int y)
        {
            int hash = x * 73856093 ^ y * 19349663;
            hash = (hash >> 13) ^ hash;
            return ((hash * 15731 + 789221) & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var carrot in _carrots)
            {
                carrot.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем грядку (земля)
            spriteBatch.Draw(_soilTexture, _position - new Vector2(_soilTexture.Width / 2, _soilTexture.Height / 2), Color.White);

            // Рисуем бортики грядки
            DrawBedEdges(spriteBatch);

            // Рисуем морковки
            foreach (var carrot in _carrots)
            {
                carrot.Draw(spriteBatch);
            }
        }

        private void DrawBedEdges(SpriteBatch spriteBatch)
        {
            int width = 200;
            int height = 80;
            float edgeHeight = 10 * _scale;

            // Передний бортик
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle((int)_position.X - width / 2, (int)_position.Y + height / 2 - (int)edgeHeight, width, (int)edgeHeight),
                new Color(139, 90, 43)
            );

            // Задний бортик
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle((int)_position.X - width / 2, (int)_position.Y - height / 2, width, (int)edgeHeight),
                new Color(139, 90, 43)
            );

            // Левый бортик
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle((int)_position.X - width / 2, (int)_position.Y - height / 2, (int)edgeHeight, height),
                new Color(139, 90, 43)
            );

            // Правый бортик
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle((int)_position.X + width / 2 - (int)edgeHeight, (int)_position.Y - height / 2, (int)edgeHeight, height),
                new Color(139, 90, 43)
            );
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
            _soilTexture?.Dispose();
        }
    }

    /// <summary>
    /// Морковка на грядке.
    /// </summary>
    public class Carrot
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Vector2 _position;
        private readonly float _scale;
        private float _swayTime;
        private Texture2D _pixelTexture;

        public Carrot(GraphicsDevice graphicsDevice, Vector2 position, float scale)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _scale = scale;
            _swayTime = 0f;

            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            _swayTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float sway = (float)Math.Sin(_swayTime) * 0.05f;

            // Рисуем оранжевую часть морковки
            DrawCarrotBody(spriteBatch, sway);

            // Рисуем зелёную ботву
            DrawCarrotTop(spriteBatch, sway);
        }

        private void DrawCarrotBody(SpriteBatch spriteBatch, float sway)
        {
            float carrotWidth = 8 * _scale;
            float carrotHeight = 20 * _scale;

            // Рисуем треугольную морковку
            Vector2 top = _position + new Vector2(0, -carrotHeight / 2);
            Vector2 bottomLeft = _position + new Vector2(-carrotWidth / 2, carrotHeight / 2);
            Vector2 bottomRight = _position + new Vector2(carrotWidth / 2, carrotHeight / 2);

            DrawTriangle(spriteBatch, top, bottomLeft, bottomRight, new Color(255, 140, 0), sway);
        }

        private void DrawCarrotTop(SpriteBatch spriteBatch, float sway)
        {
            float topY = _position.Y - 10 * _scale;

            // Рисуем зелёные листики ботвы
            for (int i = 0; i < 3; i++)
            {
                float leafAngle = (i - 1) * 0.3f + sway;
                float leafLength = 10 * _scale;
                float leafWidth = 4 * _scale;

                Vector2 leafStart = _position + new Vector2((i - 1) * 5 * _scale, -10 * _scale);
                Vector2 leafEnd = leafStart + new Vector2(
                    (float)Math.Sin(leafAngle) * leafLength,
                    -(float)Math.Cos(leafAngle) * leafLength
                );

                DrawLine(spriteBatch, leafStart, leafEnd, new Color(34, 139, 34), leafWidth);
            }
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color, float rotation)
        {
            // Применяем вращение к точкам
            Vector2 center = _position;
            p1 = RotatePoint(p1, center, rotation);
            p2 = RotatePoint(p2, center, rotation);
            p3 = RotatePoint(p3, center, rotation);

            float minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            float maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            float minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            float maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));

            for (float x = minX; x <= maxX; x += 1.5f)
            {
                for (float y = minY; y <= maxY; y += 1.5f)
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

        private Vector2 RotatePoint(Vector2 point, Vector2 center, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            return new Vector2(
                center.X + dx * cos - dy * sin,
                center.Y + dx * sin + dy * cos
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
    }
}
