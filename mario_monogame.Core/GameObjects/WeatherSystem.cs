using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Система погоды с поддержкой дождя и снега.
    /// </summary>
    public class WeatherSystem : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ParticleSystem _particleSystem;
        private WeatherType _currentWeather;
        private float _weatherDuration;
        private float _weatherTimer;
        private float _intensity;
        private readonly Random _random;
        
        private Texture2D _cloudOverlay;
        private float _cloudAlpha;
        
        public WeatherType CurrentWeather => _currentWeather;
        public float Intensity => _intensity;
        
        public WeatherSystem(GraphicsDevice graphicsDevice, ParticleSystem particleSystem)
        {
            _graphicsDevice = graphicsDevice;
            _particleSystem = particleSystem;
            _random = new Random();
            _currentWeather = WeatherType.Clear;
            _weatherDuration = 60f;
            _weatherTimer = 0f;
            _intensity = 0f;
            _cloudAlpha = 0f;
            
            // Создаём текстуру облачного оверлея
            CreateCloudOverlay();
        }
        
        private void CreateCloudOverlay()
        {
            _cloudOverlay = new Texture2D(_graphicsDevice, 1280, 720);
            Color[] pixels = new Color[1280 * 720];
            
            for (int y = 0; y < 720; y++)
            {
                for (int x = 0; x < 1280; x++)
                {
                    // Создаём шум для облаков
                    float noise = GetCloudNoise(x, y);
                    byte alpha = (byte)(noise * 80);
                    pixels[y * 1280 + x] = new Color((byte)150, (byte)150, (byte)150, alpha);
                }
            }
            
            _cloudOverlay.SetData(pixels);
        }
        
        private float GetCloudNoise(int x, int y)
        {
            // Простой псевдослучайный шум для облаков
            int hash = x * 73856093 ^ y * 19349663;
            hash = (hash >> 13) ^ hash;
            return ((hash * 15731 + 789221) & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }
        
        public void Update(GameTime gameTime, Vector2 cameraPosition)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _weatherTimer += elapsed;
            
            // Смена погоды
            if (_weatherTimer >= _weatherDuration)
            {
                ChangeWeather();
                _weatherTimer = 0f;
            }
            
            // Плавное изменение интенсивности
            UpdateWeatherIntensity(elapsed);
            
            // Эмиссия частиц погоды
            EmitWeatherParticles(cameraPosition);
            
            // Обновление облачности
            UpdateClouds(elapsed);
        }
        
        private void ChangeWeather()
        {
            // Выбираем новую погоду случайно
            int roll = _random.Next(100);
            
            if (roll < 40)
            {
                _currentWeather = WeatherType.Clear;
                _weatherDuration = 60f + _random.Next(60);
            }
            else if (roll < 70)
            {
                _currentWeather = WeatherType.Rain;
                _weatherDuration = 30f + _random.Next(60);
            }
            else if (roll < 90)
            {
                _currentWeather = WeatherType.LightRain;
                _weatherDuration = 30f + _random.Next(30);
            }
            else
            {
                _currentWeather = WeatherType.Snow;
                _weatherDuration = 60f + _random.Next(120);
            }
        }
        
        private void UpdateWeatherIntensity(float elapsed)
        {
            float targetIntensity = _currentWeather != WeatherType.Clear ? 1f : 0f;
            _intensity = MathHelper.Lerp(_intensity, targetIntensity, elapsed * 0.5f);
        }
        
        private void UpdateClouds(float elapsed)
        {
            float targetCloudAlpha = (_currentWeather == WeatherType.Rain || 
                                      _currentWeather == WeatherType.LightRain ||
                                      _currentWeather == WeatherType.Snow) ? 0.6f : 0f;
            _cloudAlpha = MathHelper.Lerp(_cloudAlpha, targetCloudAlpha, elapsed * 0.3f);
        }
        
        private void EmitWeatherParticles(Vector2 cameraPosition)
        {
            if (_intensity < 0.1f) return;
            
            int particleCount = _currentWeather switch
            {
                WeatherType.Rain => 3,
                WeatherType.LightRain => 1,
                WeatherType.Snow => 2,
                _ => 0
            };
            
            for (int i = 0; i < particleCount; i++)
            {
                float x = cameraPosition.X + _random.Next(1280);
                float y = cameraPosition.Y - 50;
                
                if (_currentWeather == WeatherType.Rain || _currentWeather == WeatherType.LightRain)
                {
                    _particleSystem.EmitRaindrop(new Vector2(x, y));
                }
                else if (_currentWeather == WeatherType.Snow)
                {
                    _particleSystem.EmitSnowflake(new Vector2(x, y));
                }
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем облачный оверлей
            if (_cloudAlpha > 0.01f)
            {
                spriteBatch.Draw(
                    _cloudOverlay,
                    Vector2.Zero,
                    null,
                    new Color(1f, 1f, 1f, _cloudAlpha),
                    0f,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        public void ForceWeather(WeatherType weather)
        {
            _currentWeather = weather;
            _weatherTimer = 0f;
        }
        
        public void Dispose()
        {
            _cloudOverlay?.Dispose();
        }
    }
    
    public enum WeatherType
    {
        Clear,
        Rain,
        LightRain,
        Snow
    }
}
