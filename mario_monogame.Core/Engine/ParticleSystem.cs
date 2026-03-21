using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.Engine
{
    /// <summary>
    /// Система частиц.
    /// </summary>
    public class ParticleSystem
    {
        private List<Particle> _particles;
        private List<Particle> _deadParticles;
        private Texture2D _defaultTexture;
        private GraphicsDevice _graphicsDevice;

        public int ActiveParticleCount => _particles.Count;
        public int MaxParticles { get; set; } = 1000;

        public ParticleSystem(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _particles = new List<Particle>();
            _deadParticles = new List<Particle>();
            _defaultTexture = CreateCircleTexture(graphicsDevice, 8, Color.White);
        }

        private Texture2D CreateCircleTexture(GraphicsDevice gd, int radius, Color color)
        {
            Texture2D texture = new Texture2D(gd, radius * 2, radius * 2);
            Color[] data = new Color[radius * 2 * radius * 2];

            for (int y = 0; y < radius * 2; y++)
            {
                for (int x = 0; x < radius * 2; x++)
                {
                    float dx = x - radius;
                    float dy = y - radius;
                    float dist = dx * dx + dy * dy;

                    if (dist <= radius * radius)
                    {
                        float alpha = 1f - (dist / (radius * radius));
                        data[y * radius * 2 + x] = new Color(color.R, color.G, color.B, (byte)(alpha * 255));
                    }
                    else
                    {
                        data[y * radius * 2 + x] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        public void Update(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                particle.Lifetime -= deltaTime;

                if (particle.Lifetime <= 0)
                {
                    _deadParticles.Add(particle);
                    _particles.RemoveAt(i);
                }
                else
                {
                    particle.Update(deltaTime);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera2D camera = null)
        {
            foreach (var particle in _particles)
            {
                particle.Draw(spriteBatch, camera);
            }
        }

        public void Clear()
        {
            _particles.Clear();
            _deadParticles.Clear();
        }

        public Particle Emit(Vector2 position, ParticleSettings settings)
        {
            if (_particles.Count >= MaxParticles)
                return null;

            Particle particle;

            if (_deadParticles.Count > 0)
            {
                particle = _deadParticles[_deadParticles.Count - 1];
                _deadParticles.RemoveAt(_deadParticles.Count - 1);
            }
            else
            {
                particle = new Particle();
            }

            particle.Initialize(position, settings, _defaultTexture);
            _particles.Add(particle);

            return particle;
        }

        public void Burst(Vector2 position, ParticleSettings baseSettings, int count, float spread = 0f)
        {
            for (int i = 0; i < count; i++)
            {
                var settings = baseSettings;
                float angle = spread > 0 ? (float)(i * (spread / count)) : 0;
                settings.Direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                settings.Direction *= baseSettings.Speed * (0.5f + Random.Shared.NextSingle());
                settings.Lifetime = baseSettings.Lifetime * (0.5f + Random.Shared.NextSingle() * 0.5f);
                settings.Scale = baseSettings.Scale * (0.5f + Random.Shared.NextSingle() * 0.5f);
                
                Emit(position, settings);
            }
        }

        public void CreateExplosion(Vector2 position, Color color, int count = 30)
        {
            var settings = new ParticleSettings
            {
                Direction = Vector2.Zero,
                Speed = 200f,
                Gravity = new Vector2(0, 500f),
                Lifetime = 0.8f,
                Scale = 1f,
                Color = color,
                FadeOut = true
            };
            Burst(position, settings, count, MathHelper.Pi * 2);
        }

        public void CreateDust(Vector2 position, int count = 5)
        {
            var settings = new ParticleSettings
            {
                Direction = new Vector2(0, -1),
                Speed = 50f,
                Gravity = new Vector2(0, -20f),
                Lifetime = 1f,
                Scale = 0.5f,
                Color = new Color(200, 180, 150, 150),
                FadeOut = true
            };
            Burst(position, settings, count, MathHelper.Pi);
        }

        public void CreateCoinCollect(Vector2 position)
        {
            var settings = new ParticleSettings
            {
                Direction = new Vector2(0, -1),
                Speed = 150f,
                Gravity = new Vector2(0, -100f),
                Lifetime = 0.6f,
                Scale = 0.8f,
                Color = Color.Gold,
                FadeOut = true,
                RotationSpeed = 10f
            };
            Burst(position, settings, 15, MathHelper.Pi);
        }

        public void CreateHitEffect(Vector2 position, Color color)
        {
            var settings = new ParticleSettings
            {
                Direction = Vector2.Zero,
                Speed = 250f,
                Gravity = new Vector2(0, 300f),
                Lifetime = 0.4f,
                Scale = 0.6f,
                Color = color,
                FadeOut = true
            };
            Burst(position, settings, 10, MathHelper.Pi * 2);
        }
    }

    public struct ParticleSettings
    {
        public Vector2 Direction;
        public float Speed;
        public Vector2 Gravity;
        public float Lifetime;
        public float Scale;
        public Color Color;
        public bool FadeOut;
        public float RotationSpeed;
        public Texture2D CustomTexture;

        public static ParticleSettings Default => new ParticleSettings
        {
            Direction = new Vector2(0, -1),
            Speed = 100f,
            Gravity = Vector2.Zero,
            Lifetime = 1f,
            Scale = 1f,
            Color = Color.White,
            FadeOut = false,
            RotationSpeed = 0f
        };
    }

    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Gravity;
        public float Lifetime;
        public float MaxLifetime;
        public float Scale;
        public Color Color;
        public float Rotation;
        public float RotationSpeed;
        public bool FadeOut;
        public Texture2D Texture;

        public void Initialize(Vector2 position, ParticleSettings settings, Texture2D defaultTexture)
        {
            Position = position;
            Velocity = settings.Direction * settings.Speed;
            Gravity = settings.Gravity;
            Lifetime = settings.Lifetime;
            MaxLifetime = settings.Lifetime;
            Scale = settings.Scale;
            Color = settings.Color;
            FadeOut = settings.FadeOut;
            Rotation = 0f;
            RotationSpeed = settings.RotationSpeed;
            Texture = settings.CustomTexture ?? defaultTexture;
        }

        public void Update(float deltaTime)
        {
            Velocity += Gravity * deltaTime;
            Position += Velocity * deltaTime;
            Rotation += RotationSpeed * deltaTime;

            if (FadeOut)
            {
                float alpha = Lifetime / MaxLifetime;
                Color = new Color(Color.R, Color.G, Color.B, (byte)(Color.A * alpha));
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera2D camera = null)
        {
            Vector2 drawPos = camera != null ? camera.WorldToScreen(Position) : Position;
            
            spriteBatch.Draw(
                Texture,
                drawPos,
                null,
                Color,
                Rotation,
                new Vector2(Texture.Width / 2f, Texture.Height / 2f),
                Scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}
