using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Менеджер системы частиц для создания визуальных эффектов.
    /// </summary>
    public class ParticleSystem : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly List<Particle> _particles;
        private Texture2D _pixelTexture;
        private SpriteFont _font;
        private readonly Random _random;

        public ParticleSystem(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _particles = new List<Particle>();
            _random = new Random();
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        
        public void SetFont(SpriteFont font)
        {
            _font = font;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update(elapsed);
                
                if (_particles[i].Life <= 0)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var particle in _particles)
            {
                particle.Draw(spriteBatch, _pixelTexture, _font);
            }
        }

        /// <summary>
        /// Создать эффект сбора моркови (звёздочки и искры).
        /// </summary>
        public void EmitCarrotCollectEffect(Vector2 position)
        {
            // Золотые искры
            for (int i = 0; i < 15; i++)
            {
                var particle = new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1),
                        (float)(_random.NextDouble() * 2 - 1)
                    ) * (100 + _random.Next(100)),
                    Size = 3 + _random.Next(4),
                    Color = new Color(255, 215, 0),
                    Life = 0.8f,
                    MaxLife = 0.8f,
                    Rotation = _random.Next(360),
                    RotationSpeed = (float)(_random.NextDouble() * 10 - 5),
                    Gravity = 200f,
                    FadeOut = true
                };
                _particles.Add(particle);
            }
            
            // Оранжевые частицы
            for (int i = 0; i < 10; i++)
            {
                var particle = new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1),
                        (float)(_random.NextDouble() * -1 - 0.5f)
                    ) * (150 + _random.Next(100)),
                    Size = 4 + _random.Next(3),
                    Color = new Color(255, 140, 0),
                    Life = 0.6f,
                    MaxLife = 0.6f,
                    Rotation = _random.Next(360),
                    RotationSpeed = (float)(_random.NextDouble() * 8 - 4),
                    Gravity = 150f,
                    FadeOut = true
                };
                _particles.Add(particle);
            }
        }

        /// <summary>
        /// Создать эффект прыжка (пыль).
        /// </summary>
        public void EmitJumpEffect(Vector2 position)
        {
            for (int i = 0; i < 8; i++)
            {
                var particle = new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1) * 50,
                        (float)(_random.NextDouble() * -0.5f - 0.5f) * 100
                    ),
                    Size = 5 + _random.Next(8),
                    Color = new Color(200, 180, 150, 180),
                    Life = 0.5f,
                    MaxLife = 0.5f,
                    Rotation = 0,
                    RotationSpeed = 0,
                    Gravity = 50f,
                    FadeOut = true
                };
                _particles.Add(particle);
            }
        }

        /// <summary>
        /// Создать эффект приземления.
        /// </summary>
        public void EmitLandEffect(Vector2 position)
        {
            for (int i = 0; i < 6; i++)
            {
                var particle = new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1) * 80,
                        (float)(_random.NextDouble() * -0.3f - 0.2f) * 100
                    ),
                    Size = 4 + _random.Next(6),
                    Color = new Color(180, 160, 130, 200),
                    Life = 0.4f,
                    MaxLife = 0.4f,
                    Rotation = 0,
                    RotationSpeed = 0,
                    Gravity = 80f,
                    FadeOut = true
                };
                _particles.Add(particle);
            }
        }

        /// <summary>
        /// Создать эффект сердечек (например, при получении бонуса).
        /// </summary>
        public void EmitHeartEffect(Vector2 position)
        {
            for (int i = 0; i < 5; i++)
            {
                var particle = new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1) * 30,
                        (float)(_random.NextDouble() * -1 - 0.5f) * 80
                    ),
                    Size = 8 + _random.Next(4),
                    Color = new Color(255, 100, 100),
                    Life = 1f,
                    MaxLife = 1f,
                    Rotation = _random.Next(360),
                    RotationSpeed = (float)(_random.NextDouble() * 4 - 2),
                    Gravity = 30f,
                    FadeOut = true,
                    Shape = ParticleShape.Heart
                };
                _particles.Add(particle);
            }
        }

        /// <summary>
        /// Создать эффект снежинок (для погоды).
        /// </summary>
        public void EmitSnowflake(Vector2 position)
        {
            var particle = new Particle
            {
                Position = position,
                Velocity = new Vector2(
                    (float)(_random.NextDouble() * 0.4f - 0.2f) * 50,
                    30 + _random.Next(30)
                ),
                Size = 3 + _random.Next(3),
                Color = new Color(255, 255, 255, 200),
                Life = 3f,
                MaxLife = 3f,
                Rotation = _random.Next(360),
                RotationSpeed = (float)(_random.NextDouble() * 2 - 1),
                Gravity = 10f,
                FadeOut = false,
                Shape = ParticleShape.Snowflake
            };
            _particles.Add(particle);
        }

        /// <summary>
        /// Создать эффект дождевых капель.
        /// </summary>
        public void EmitRaindrop(Vector2 position)
        {
            var particle = new Particle
            {
                Position = position,
                Velocity = new Vector2(0, 300 + _random.Next(200)),
                Size = 2 + _random.Next(2),
                Color = new Color(150, 180, 220, 180),
                Life = 1.5f,
                MaxLife = 1.5f,
                Rotation = 0,
                RotationSpeed = 0,
                Gravity = 500f,
                FadeOut = false,
                Shape = ParticleShape.Raindrop
            };
            _particles.Add(particle);
        }

        /// <summary>
        /// Создать эффект всплывающего текста (+очки).
        /// </summary>
        public void EmitFloatingText(string text, Vector2 position, Color color)
        {
            var particle = new Particle
            {
                Position = position,
                Velocity = new Vector2(0, -50),
                Size = 0,
                Color = color,
                Life = 1.5f,
                MaxLife = 1.5f,
                Rotation = 0,
                RotationSpeed = 0,
                Gravity = -30f,
                FadeOut = true,
                Shape = ParticleShape.Text,
                Text = text
            };
            _particles.Add(particle);
        }

        public void Clear()
        {
            _particles.Clear();
        }

        public void Dispose()
        {
            _pixelTexture?.Dispose();
        }
    }

    /// <summary>
    /// Отдельная частица.
    /// </summary>
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Rotation;
        public float RotationSpeed;
        public float Gravity;
        public bool FadeOut;
        public ParticleShape Shape;
        public string Text;

        public void Update(float elapsed)
        {
            Position += Velocity * elapsed;
            Velocity.Y += Gravity * elapsed;
            Rotation += RotationSpeed * elapsed;
            Life -= elapsed;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font)
        {
            if (Life <= 0) return;
            
            float alpha = FadeOut ? (Life / MaxLife) : 1f;
            Color drawColor = new Color(Color.R, Color.G, Color.B, (byte)(Color.A * alpha));
            
            switch (Shape)
            {
                case ParticleShape.Heart:
                    DrawHeart(spriteBatch, Position, Size, drawColor, Rotation, pixelTexture);
                    break;
                case ParticleShape.Snowflake:
                    DrawSnowflake(spriteBatch, Position, Size, drawColor, Rotation, pixelTexture);
                    break;
                case ParticleShape.Raindrop:
                    DrawRaindrop(spriteBatch, Position, Size, drawColor, pixelTexture);
                    break;
                case ParticleShape.Text:
                    if (font != null)
                    {
                        DrawTextParticle(spriteBatch, Position, Text, drawColor, font);
                    }
                    break;
                default:
                    DrawCircle(spriteBatch, Position, Size, drawColor, pixelTexture);
                    break;
            }
        }

        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, Texture2D texture)
        {
            spriteBatch.Draw(
                texture,
                center,
                null,
                color,
                0f,
                new Vector2(0.5f),
                radius,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawHeart(SpriteBatch spriteBatch, Vector2 center, float size, Color color, float rotation, Texture2D texture)
        {
            // Рисуем сердечко из двух кругов и треугольника
            float offset = size * 0.3f;
            
            spriteBatch.Draw(
                texture,
                center + new Vector2(-offset, -offset * 0.5f),
                null,
                color,
                rotation,
                new Vector2(0.5f),
                size * 0.6f,
                SpriteEffects.None,
                0f
            );
            
            spriteBatch.Draw(
                texture,
                center + new Vector2(offset, -offset * 0.5f),
                null,
                color,
                rotation,
                new Vector2(0.5f),
                size * 0.6f,
                SpriteEffects.None,
                0f
            );
            
            spriteBatch.Draw(
                texture,
                center + new Vector2(0, offset * 0.3f),
                null,
                color,
                rotation,
                new Vector2(0.5f),
                new Vector2(size * 0.7f, size * 0.5f),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawSnowflake(SpriteBatch spriteBatch, Vector2 center, float size, Color color, float rotation, Texture2D texture)
        {
            float length = size;
            float thickness = size * 0.2f;
            
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)(i * Math.PI / 3) + rotation;
                Vector2 end = center + new Vector2(
                    (float)Math.Cos(angle) * length,
                    (float)Math.Sin(angle) * length
                );
                
                DrawLine(spriteBatch, center, end, color, thickness, texture);
            }
        }

        private void DrawRaindrop(SpriteBatch spriteBatch, Vector2 center, float size, Color color, Texture2D texture)
        {
            spriteBatch.Draw(
                texture,
                center,
                null,
                color,
                0f,
                new Vector2(0.5f),
                new Vector2(size * 0.3f, size * 1.5f),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawTextParticle(SpriteBatch spriteBatch, Vector2 position, string text, Color color, SpriteFont font)
        {
            spriteBatch.DrawString(font, text, position, color);
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness, Texture2D texture)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);
            
            spriteBatch.Draw(
                texture,
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

    public enum ParticleShape
    {
        Circle,
        Heart,
        Snowflake,
        Raindrop,
        Text
    }
}
