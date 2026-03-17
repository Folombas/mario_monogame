using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Яблоня с красными спелыми яблоками и зелёными листьями.
    /// </summary>
    public class AppleTree
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Vector2 _position;
        private readonly float _scale;
        private readonly List<Apple> _apples;
        private readonly List<Leaf> _leaves;
        private readonly List<Branch> _branches;

        public Vector2 Position => _position;

        public AppleTree(GraphicsDevice graphicsDevice, Vector2 position, float scale = 1f)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _scale = scale;

            // Создаём ветви
            _branches = new List<Branch>
            {
                new Branch(new Vector2(0, -60), new Vector2(-40, -30), scale),
                new Branch(new Vector2(0, -60), new Vector2(40, -30), scale),
                new Branch(new Vector2(0, -60), new Vector2(0, -50), scale),
                new Branch(new Vector2(-40, -30), new Vector2(-60, -50), scale),
                new Branch(new Vector2(40, -30), new Vector2(60, -50), scale),
                new Branch(new Vector2(0, -50), new Vector2(-25, -70), scale),
                new Branch(new Vector2(0, -50), new Vector2(25, -70), scale),
            };

            // Создаём листья
            _leaves = new List<Leaf>
            {
                new Leaf(graphicsDevice, position + new Vector2(-60, -110) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(60, -110) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(0, -120) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(-40, -80) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(40, -80) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(-70, -90) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(70, -90) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(-30, -100) * scale, scale),
                new Leaf(graphicsDevice, position + new Vector2(30, -100) * scale, scale),
            };

            // Создаём яблоки
            _apples = new List<Apple>
            {
                new Apple(graphicsDevice, position + new Vector2(-50, -70) * scale, scale),
                new Apple(graphicsDevice, position + new Vector2(50, -70) * scale, scale),
                new Apple(graphicsDevice, position + new Vector2(0, -90) * scale, scale),
                new Apple(graphicsDevice, position + new Vector2(-30, -110) * scale, scale),
                new Apple(graphicsDevice, position + new Vector2(30, -110) * scale, scale),
                new Apple(graphicsDevice, position + new Vector2(-65, -100) * scale, scale),
                new Apple(graphicsDevice, position + new Vector2(65, -100) * scale, scale),
            };
        }

        public void Update(GameTime gameTime)
        {
            foreach (var apple in _apples)
            {
                apple.Update(gameTime);
            }

            foreach (var leaf in _leaves)
            {
                leaf.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем ствол
            DrawTrunk(spriteBatch);

            // Рисуем ветви
            foreach (var branch in _branches)
            {
                branch.Draw(spriteBatch, _position);
            }

            // Рисуем листья (сначала задний план)
            for (int i = 0; i < _leaves.Count; i++)
            {
                if (i % 2 == 0)
                    _leaves[i].Draw(spriteBatch);
            }

            // Рисуем яблоки
            foreach (var apple in _apples)
            {
                apple.Draw(spriteBatch);
            }

            // Рисуем листья (передний план)
            for (int i = 0; i < _leaves.Count; i++)
            {
                if (i % 2 == 1)
                    _leaves[i].Draw(spriteBatch);
            }
        }

        private void DrawTrunk(SpriteBatch spriteBatch)
        {
            float trunkWidth = 20 * _scale;
            float trunkHeight = 80 * _scale;

            using var trunkTexture = new Texture2D(_graphicsDevice, 1, 1);
            trunkTexture.SetData(new[] { new Color(101, 67, 33) });

            // Основной ствол
            spriteBatch.Draw(
                trunkTexture,
                new Rectangle(
                    (int)(_position.X - trunkWidth / 2),
                    (int)(_position.Y - trunkHeight),
                    (int)trunkWidth,
                    (int)trunkHeight
                ),
                Color.White
            );

            // Детали ствола (текстура коры)
            for (int i = 0; i < 5; i++)
            {
                float noise = GetNoise(i, 0);
                byte barkColor = (byte)(80 + noise * 30);
                trunkTexture.SetData(new[] { new Color(barkColor, (byte)(50 + noise * 20), (byte)(20 + noise * 10)) });

                spriteBatch.Draw(
                    trunkTexture,
                    new Rectangle(
                        (int)(_position.X - trunkWidth / 2 + 3),
                        (int)(_position.Y - trunkHeight + i * 15 * _scale),
                        (int)(trunkWidth - 6),
                        (int)(8 * _scale)
                    ),
                    Color.White
                );
            }
        }

        private float GetNoise(int x, int y)
        {
            int hash = x * 73856093 ^ y * 19349663;
            hash = (hash >> 13) ^ hash;
            return ((hash * 15731 + 789221) & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }
    }

    /// <summary>
    /// Ветвь дерева.
    /// </summary>
    public class Branch
    {
        private readonly Vector2 _start;
        private readonly Vector2 _end;
        private readonly float _scale;

        public Branch(Vector2 start, Vector2 end, float scale)
        {
            _start = start;
            _end = end;
            _scale = scale;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 treePosition)
        {
            Vector2 startPos = treePosition + _start * _scale;
            Vector2 endPos = treePosition + _end * _scale;

            Vector2 direction = endPos - startPos;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);

            using var branchTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            branchTexture.SetData(new[] { new Color(101, 67, 33) });

            float thickness = 8 * _scale;
            spriteBatch.Draw(
                branchTexture,
                startPos,
                null,
                new Color(139, 90, 43),
                angle,
                Vector2.Zero,
                new Vector2(length, thickness / length),
                SpriteEffects.None,
                0f
            );
        }
    }

    /// <summary>
    /// Зелёный листок.
    /// </summary>
    public class Leaf
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Vector2 _position;
        private readonly float _scale;
        private float _swayAngle;
        private float _swayTime;

        public Leaf(GraphicsDevice graphicsDevice, Vector2 position, float scale)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _scale = scale;
            _swayTime = 0f;
        }

        public void Update(GameTime gameTime)
        {
            _swayTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 3f;
            _swayAngle = (float)Math.Sin(_swayTime) * 0.1f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем листок как овал
            DrawLeafShape(spriteBatch);
        }

        private void DrawLeafShape(SpriteBatch spriteBatch)
        {
            float leafWidth = 15 * _scale;
            float leafHeight = 25 * _scale;

            // Рисуем листок из нескольких частей
            using var pixelTexture = new Texture2D(_graphicsDevice, 1, 1);

            // Основная часть листа
            for (int y = 0; y < leafHeight; y++)
            {
                float widthAtY = leafWidth * (1 - (float)Math.Pow((y - leafHeight / 2) / (leafHeight / 2), 2));
                
                for (int x = 0; x < widthAtY; x++)
                {
                    float offsetX = x - widthAtY / 2;
                    float noise = GetNoise((int)(_position.X + offsetX), (int)(_position.Y + y));
                    
                    Color leafColor = new Color(
                        (byte)(34 + noise * 30),
                        (byte)(139 + noise * 40),
                        (byte)(34 + noise * 20)
                    );

                    pixelTexture.SetData(new[] { leafColor });
                    
                    spriteBatch.Draw(
                        pixelTexture,
                        new Vector2(_position.X + offsetX, _position.Y + y - leafHeight / 2),
                        null,
                        leafColor,
                        _swayAngle,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            // Прожилка листа
            pixelTexture.SetData(new[] { new Color(60, 100, 40) });
            spriteBatch.Draw(
                pixelTexture,
                _position,
                null,
                new Color(60, 100, 40),
                _swayAngle,
                new Vector2(0.5f, 0f),
                new Vector2(1, leafHeight),
                SpriteEffects.None,
                0f
            );
        }

        private float GetNoise(int x, int y)
        {
            int hash = x * 73856093 ^ y * 19349663;
            hash = (hash >> 13) ^ hash;
            return ((hash * 15731 + 789221) & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }
    }

    /// <summary>
    /// Красное спелое яблоко.
    /// </summary>
    public class Apple
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Vector2 _position;
        private readonly float _scale;
        private float _swingAngle;
        private float _swingTime;

        public Apple(GraphicsDevice graphicsDevice, Vector2 position, float scale)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _scale = scale;
            _swingTime = 0f;
        }

        public void Update(GameTime gameTime)
        {
            _swingTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            _swingAngle = (float)Math.Sin(_swingTime) * 0.05f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float appleRadius = 12 * _scale;

            // Рисуем яблоко
            DrawAppleBody(spriteBatch, appleRadius);

            // Рисуем плодоножку
            DrawStem(spriteBatch, appleRadius);

            // Рисуем блик
            DrawHighlight(spriteBatch, appleRadius);
        }

        private void DrawAppleBody(SpriteBatch spriteBatch, float radius)
        {
            int segments = 24;

            for (int i = 0; i < segments; i++)
            {
                double angle1 = i * Math.PI * 2 / segments;
                double angle2 = (i + 1) * Math.PI * 2 / segments;

                Vector2 point1 = new Vector2(
                    _position.X + (float)Math.Cos(angle1) * radius,
                    _position.Y + (float)Math.Sin(angle1) * radius
                );
                Vector2 point2 = new Vector2(
                    _position.X + (float)Math.Cos(angle2) * radius,
                    _position.Y + (float)Math.Sin(angle2) * radius
                );

                // Градиент красного цвета для объёма
                float brightness = 0.7f + 0.3f * (float)Math.Sin(angle1 + Math.PI / 2);
                Color appleColor = new Color(
                    (byte)(200 * brightness),
                    (byte)(30 * brightness),
                    (byte)(30 * brightness)
                );

                DrawFilledTriangle(spriteBatch, _position, point1, point2, appleColor);
            }
        }

        private void DrawStem(SpriteBatch spriteBatch, float radius)
        {
            using var stemTexture = new Texture2D(_graphicsDevice, 1, 1);
            stemTexture.SetData(new[] { new Color(101, 67, 33) });

            spriteBatch.Draw(
                stemTexture,
                new Vector2(_position.X, _position.Y - radius),
                null,
                new Color(101, 67, 33),
                _swingAngle,
                Vector2.Zero,
                new Vector2(3 * _scale, 8 * _scale),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawHighlight(SpriteBatch spriteBatch, float radius)
        {
            using var highlightTexture = new Texture2D(_graphicsDevice, 1, 1);
            highlightTexture.SetData(new[] { Color.White });

            spriteBatch.Draw(
                highlightTexture,
                _position + new Vector2(-radius * 0.3f, -radius * 0.3f),
                null,
                new Color(255, 255, 255, 180),
                0f,
                Vector2.Zero,
                radius * 0.25f,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawFilledTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            float minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            float maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            float minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            float maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));

            using var pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { color });

            for (float x = minX; x <= maxX; x += 1.5f)
            {
                for (float y = minY; y <= maxY; y += 1.5f)
                {
                    if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                    {
                        spriteBatch.Draw(
                            pixelTexture,
                            new Vector2(x, y),
                            null,
                            color,
                            _swingAngle,
                            Vector2.Zero,
                            1f,
                            SpriteEffects.None,
                            0f
                        );
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
