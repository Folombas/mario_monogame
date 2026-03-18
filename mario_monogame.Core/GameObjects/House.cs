using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Домик зайчика - место, куда можно складывать собранную морковку.
    /// </summary>
    public class House
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Vector2 _position;
        private readonly float _scale;
        private Texture2D _pixelTexture;
        
        // Анимация
        private float _smokeTime;
        private bool _emitSmoke;
        
        // Размеры
        private readonly float _width;
        private readonly float _height;
        private readonly float _doorWidth;
        private readonly float _doorHeight;

        public Vector2 Position => _position;
        public float DoorX => _position.X;
        public float DoorY => _position.Y + _height - _doorHeight;
        public float InteractionRadius => 80f;

        public House(GraphicsDevice graphicsDevice, Vector2 position, float scale = 1f)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _scale = scale;
            
            _width = 180f * scale;
            _height = 140f * scale;
            _doorWidth = 40f * scale;
            _doorHeight = 50f * scale;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _smokeTime = 0f;
            _emitSmoke = false;
        }

        public void Update(GameTime gameTime)
        {
            _smokeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем домик
            DrawWalls(spriteBatch);
            DrawRoof(spriteBatch);
            DrawDoor(spriteBatch);
            DrawWindow(spriteBatch);
            DrawChimney(spriteBatch);
            DrawSmoke(spriteBatch);
            DrawDecorations(spriteBatch);
        }

        private void DrawWalls(SpriteBatch spriteBatch)
        {
            // Основной прямоугольник стен
            DrawRectangle(
                spriteBatch,
                _position - new Vector2(_width / 2, _height / 2),
                _width,
                _height,
                new Color(210, 180, 140)
            );
            
            // Текстура дерева (вертикальные доски)
            DrawWoodPlanks(spriteBatch);
        }

        private void DrawWoodPlanks(SpriteBatch spriteBatch)
        {
            int plankCount = 8;
            float plankWidth = _width / plankCount;
            
            for (int i = 0; i < plankCount; i++)
            {
                float x = _position.X - _width / 2 + i * plankWidth;
                
                // Тени между досками
                DrawLine(
                    spriteBatch,
                    new Vector2(x, _position.Y - _height / 2),
                    new Vector2(x, _position.Y + _height / 2),
                    new Color(180, 150, 110),
                    2f
                );
            }
        }

        private void DrawRoof(SpriteBatch spriteBatch)
        {
            float roofHeight = _height * 0.6f;
            float roofOverhang = _width * 0.1f;
            
            // Треугольная крыша
            Vector2 peak = _position - new Vector2(0, _height / 2 + roofHeight);
            Vector2 leftCorner = _position - new Vector2(_width / 2 + roofOverhang, _height / 2);
            Vector2 rightCorner = _position + new Vector2(_width / 2 + roofOverhang, _height / 2);
            
            // Основная крыша (красная черепица)
            DrawTriangle(spriteBatch, peak, leftCorner, rightCorner, new Color(180, 60, 60));
            
            // Черепица (декоративные полосы)
            DrawRoofTiles(spriteBatch, peak, leftCorner, rightCorner);
            
            // Конёк крыши
            DrawRectangle(
                spriteBatch,
                peak - new Vector2(10f, 5f),
                20f,
                10f,
                new Color(150, 50, 50)
            );
        }

        private void DrawRoofTiles(SpriteBatch spriteBatch, Vector2 peak, Vector2 leftCorner, Vector2 rightCorner)
        {
            int tileRows = 5;
            float roofHeight = peak.Y - leftCorner.Y;
            
            for (int i = 1; i < tileRows; i++)
            {
                float t = i / (float)tileRows;
                float y = peak.Y + (roofHeight * t);
                float halfWidth = (_width / 2 + _width * 0.1f * t);
                
                DrawLine(
                    spriteBatch,
                    new Vector2(peak.X - halfWidth, y),
                    new Vector2(peak.X + halfWidth, y),
                    new Color(160, 50, 50, 150),
                    3f
                );
            }
        }

        private void DrawDoor(SpriteBatch spriteBatch)
        {
            float doorX = _position.X - _doorWidth / 2;
            float doorY = _position.Y + _height / 2 - _doorHeight;
            
            // Дверная коробка
            DrawRectangle(
                spriteBatch,
                new Vector2(doorX - 3f, doorY - 3f),
                _doorWidth + 6f,
                _doorHeight + 6f,
                new Color(100, 70, 50)
            );
            
            // Основная дверь
            DrawRectangle(
                spriteBatch,
                new Vector2(doorX, doorY),
                _doorWidth,
                _doorHeight,
                new Color(139, 90, 43)
            );
            
            // Дверные панели
            float panelWidth = _doorWidth * 0.35f;
            float panelHeight = _doorHeight * 0.35f;
            
            // Верхняя панель
            DrawRectangle(
                spriteBatch,
                new Vector2(
                    _position.X - panelWidth / 2,
                    doorY + _doorHeight * 0.1f
                ),
                panelWidth,
                panelHeight,
                new Color(120, 80, 45)
            );
            
            // Нижняя панель
            DrawRectangle(
                spriteBatch,
                new Vector2(
                    _position.X - panelWidth / 2,
                    doorY + _doorHeight * 0.55f
                ),
                panelWidth,
                panelHeight,
                new Color(120, 80, 45)
            );
            
            // Дверная ручка (золотая)
            DrawCircle(
                spriteBatch,
                new Vector2(_position.X + _doorWidth * 0.25f, doorY + _doorHeight * 0.5f),
                4f,
                new Color(255, 215, 0)
            );
        }

        private void DrawWindow(SpriteBatch spriteBatch)
        {
            float windowSize = 35f * _scale;
            float windowX = _position.X - _width * 0.25f;
            float windowY = _position.Y - _height * 0.15f;
            
            // Рама
            DrawRectangle(
                spriteBatch,
                new Vector2(windowX - windowSize / 2, windowY - windowSize / 2),
                windowSize,
                windowSize,
                new Color(100, 70, 50)
            );
            
            // Стекло (голубое)
            DrawRectangle(
                spriteBatch,
                new Vector2(windowX - windowSize / 2 + 4f, windowY - windowSize / 2 + 4f),
                windowSize - 8f,
                windowSize - 8f,
                new Color(135, 206, 235, 200)
            );
            
            // Перекрёсток рамы
            DrawLine(
                spriteBatch,
                new Vector2(windowX, windowY - windowSize / 2 + 4f),
                new Vector2(windowX, windowY + windowSize / 2 - 4f),
                new Color(100, 70, 50),
                3f
            );
            
            DrawLine(
                spriteBatch,
                new Vector2(windowX - windowSize / 2 + 4f, windowY),
                new Vector2(windowX + windowSize / 2 - 4f, windowY),
                new Color(100, 70, 50),
                3f
            );
            
            // Свет из окна
            DrawRectangle(
                spriteBatch,
                new Vector2(windowX - windowSize / 2 + 8f, windowY - windowSize / 2 + 8f),
                windowSize - 16f,
                windowSize - 16f,
                new Color(255, 255, 200, 100)
            );
        }

        private void DrawChimney(SpriteBatch spriteBatch)
        {
            float chimneyWidth = 25f * _scale;
            float chimneyHeight = 40f * _scale;
            float chimneyX = _position.X + _width * 0.25f;
            float chimneyTopY = _position.Y - _height / 2 - _height * 0.3f;
            
            // Труба
            DrawRectangle(
                spriteBatch,
                new Vector2(chimneyX - chimneyWidth / 2, chimneyTopY),
                chimneyWidth,
                chimneyHeight,
                new Color(139, 60, 60)
            );
            
            // Верх трубы (шире)
            DrawRectangle(
                spriteBatch,
                new Vector2(chimneyX - chimneyWidth / 2 - 3f, chimneyTopY - 8f),
                chimneyWidth + 6f,
                15f,
                new Color(120, 50, 50)
            );
        }

        private void DrawSmoke(SpriteBatch spriteBatch)
        {
            if (_emitSmoke)
            {
                float chimneyX = _position.X + _width * 0.25f;
                float chimneyTopY = _position.Y - _height / 2 - _height * 0.3f - 8f;
                
                for (int i = 0; i < 3; i++)
                {
                    float smokeOffset = (float)Math.Sin(_smokeTime * 2 + i) * 5f;
                    float smokeY = chimneyTopY - 20f - i * 15f + (float)Math.Sin(_smokeTime * 3) * 5f;
                    float smokeSize = 10f + i * 8f;
                    byte alpha = (byte)(150 - i * 40);
                    
                    DrawCircle(
                        spriteBatch,
                        new Vector2(chimneyX + smokeOffset, smokeY),
                        smokeSize,
                        new Color((byte)200, (byte)200, (byte)200, alpha)
                    );
                }
            }
        }

        public void EmitSmokeEffect()
        {
            _emitSmoke = true;
        }

        private void DrawDecorations(SpriteBatch spriteBatch)
        {
            // Цветочный ящик под окном
            float windowX = _position.X - _width * 0.25f;
            float windowY = _position.Y - _height * 0.15f;
            float flowerBoxY = windowY + _height * 0.08f;
            
            // Ящик
            DrawRectangle(
                spriteBatch,
                new Vector2(windowX - _height * 0.1f, flowerBoxY),
                _height * 0.2f,
                15f,
                new Color(139, 90, 43)
            );
            
            // Цветы
            DrawFlower(spriteBatch, new Vector2(windowX - 8f, flowerBoxY - 5f), new Color(255, 100, 100));
            DrawFlower(spriteBatch, new Vector2(windowX, flowerBoxY - 8f), new Color(255, 255, 100));
            DrawFlower(spriteBatch, new Vector2(windowX + 8f, flowerBoxY - 5f), new Color(255, 150, 255));
            
            // Лампа у двери
            float lampX = _position.X + _doorWidth / 2 + 20f;
            float lampY = _position.Y + _height / 2 - _doorHeight * 0.7f;
            
            // Столб лампы
            DrawLine(
                spriteBatch,
                new Vector2(lampX, lampY + 15f),
                new Vector2(lampX, lampY + 40f),
                new Color(80, 80, 80),
                4f
            );
            
            // Плафон
            DrawCircle(
                spriteBatch,
                new Vector2(lampX, lampY + 10f),
                8f,
                new Color(255, 255, 200, 200)
            );
            
            // Свет
            DrawCircle(
                spriteBatch,
                new Vector2(lampX, lampY + 10f),
                15f,
                new Color(255, 255, 150, 80)
            );
        }

        private void DrawFlower(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            float petalSize = 5f;
            
            // Лепестки
            for (int i = 0; i < 5; i++)
            {
                float angle = (float)(i * Math.PI * 2 / 5);
                Vector2 petalPos = position + new Vector2(
                    (float)Math.Cos(angle) * petalSize * 0.5f,
                    (float)Math.Sin(angle) * petalSize * 0.5f
                );
                DrawCircle(spriteBatch, petalPos, petalSize, color);
            }
            
            // Центр
            DrawCircle(spriteBatch, position, petalSize * 0.6f, new Color(255, 255, 0));
        }

        private void DrawRectangle(SpriteBatch spriteBatch, Vector2 position, float width, float height, Color color)
        {
            spriteBatch.Draw(
                _pixelTexture,
                position,
                null,
                color,
                0f,
                Vector2.Zero,
                new Vector2(width, height),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            float minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            float maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            float minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            float maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
            
            for (float x = minX; x <= maxX; x += 2f)
            {
                for (float y = minY; y <= maxY; y += 2f)
                {
                    if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                    {
                        spriteBatch.Draw(_pixelTexture, new Vector2(x, y), color);
                    }
                }
            }
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            int segments = 24;
            
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
