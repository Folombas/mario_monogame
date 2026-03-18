using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Система смены дня и ночи с циклическим изменением освещения.
    /// </summary>
    public class DayNightCycle : IDisposable
    {
        private Texture2D _overlayTexture;
        private readonly GraphicsDevice _graphicsDevice;
        
        // Время суток (0-1, где 0.5 = полдень)
        private float _timeOfDay;
        private float _dayDuration;
        
        // Цвета для разного времени суток
        private Color _dayColor;
        private Color _duskColor;
        private Color _nightColor;
        private Color _dawnColor;
        
        // Звёзды
        private Star[] _stars;
        private Texture2D _starTexture;
        
        // Луна
        private Vector2 _moonPosition;
        private Texture2D _moonTexture;
        
        public float TimeOfDay 
        { 
            get => _timeOfDay; 
            set => _timeOfDay = MathHelper.Clamp(value, 0f, 1f); 
        }
        
        public bool IsNight => _timeOfDay < 0.25f || _timeOfDay > 0.75f;
        public bool IsDay => _timeOfDay >= 0.35f && _timeOfDay <= 0.65f;
        public bool IsDusk => _timeOfDay >= 0.25f && _timeOfDay < 0.35f;
        public bool IsDawn => _timeOfDay > 0.65f && _timeOfDay <= 0.75f;
        
        public DayNightCycle(GraphicsDevice graphicsDevice, float dayDurationSeconds = 120f)
        {
            _graphicsDevice = graphicsDevice;
            _dayDuration = dayDurationSeconds;
            _timeOfDay = 0.5f; // Начинаем с полудня
            
            // Цвета
            _dayColor = new Color(1f, 1f, 1f, 0f);      // Прозрачный
            _duskColor = new Color(0.8f, 0.6f, 0.4f, 0.3f);  // Оранжевый закат
            _nightColor = new Color(0.1f, 0.1f, 0.2f, 0.7f); // Тёмно-синий
            _dawnColor = new Color(0.8f, 0.5f, 0.5f, 0.3f);  // Розовый рассвет
            
            // Создаём текстуру оверлея
            _overlayTexture = new Texture2D(graphicsDevice, 1, 1);
            _overlayTexture.SetData(new[] { Color.White });
            
            // Создаём звёзды
            CreateStars();
            
            // Создаём луну
            CreateMoon();
        }
        
        private void CreateStars()
        {
            _starTexture = new Texture2D(_graphicsDevice, 1, 1);
            _starTexture.SetData(new[] { Color.White });
            
            _stars = new Star[100];
            var random = new Random();
            
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i] = new Star
                {
                    X = (float)(random.NextDouble() * 1280),
                    Y = (float)(random.NextDouble() * 400),
                    Size = random.Next(1, 3),
                    TwinkleSpeed = 0.5f + (float)random.NextDouble() * 2f,
                    TwinkleOffset = (float)(random.NextDouble() * Math.PI * 2)
                };
            }
        }
        
        private void CreateMoon()
        {
            _moonTexture = new Texture2D(_graphicsDevice, 32, 32);
            Color[] moonPixels = new Color[32 * 32];
            
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dx = x - 16;
                    float dy = y - 16;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    if (dist <= 14)
                    {
                        // Основной диск луны
                        moonPixels[y * 32 + x] = new Color(255, 255, 220, 255);
                    }
                    else if (dist <= 16)
                    {
                        // Края (полупрозрачные)
                        byte alpha = (byte)(255 * (16 - dist) / 2);
                        moonPixels[y * 32 + x] = new Color((byte)255, (byte)255, (byte)220, alpha);
                    }
                    else
                    {
                        moonPixels[y * 32 + x] = Color.Transparent;
                    }
                }
            }
            
            // Кратеры
            DrawCrater(moonPixels, 10, 12, 4);
            DrawCrater(moonPixels, 20, 18, 3);
            DrawCrater(moonPixels, 18, 8, 2);
            DrawCrater(moonPixels, 8, 22, 3);
            
            _moonTexture.SetData(moonPixels);
        }
        
        private void DrawCrater(Color[] pixels, int cx, int cy, int radius)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    if (dist <= radius)
                    {
                        int idx = y * 32 + x;
                        byte gray = (byte)(200 - dist * 5);
                        pixels[idx] = new Color(gray, gray, (byte)(gray - 20), pixels[idx].A);
                    }
                }
            }
        }
        
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeOfDay += elapsed / _dayDuration;
            
            if (_timeOfDay >= 1f)
                _timeOfDay = 0f;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            // Рисуем звёзды (только ночью)
            if (IsNight || IsDusk || IsDawn)
            {
                float starAlpha = IsNight ? 1f : 0.5f;
                foreach (var star in _stars)
                {
                    float twinkle = (float)Math.Sin(star.TwinkleOffset + _timeOfDay * Math.PI * 2 * star.TwinkleSpeed);
                    float alpha = starAlpha * (0.5f + 0.5f * twinkle);

                    spriteBatch.Draw(
                        _starTexture,
                        new Vector2(star.X, star.Y),
                        null,
                        new Color((byte)255, (byte)255, (byte)255, (byte)(alpha * 255)),
                        0f,
                        Vector2.Zero,
                        star.Size,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
            
            // Рисуем луну (ночью и в сумерках)
            if (IsNight || IsDusk || IsDawn)
            {
                float moonAlpha = IsNight ? 1f : 0.7f;
                float moonX = 1280 - 100 - _timeOfDay * 1080;
                float moonY = 100 + (float)Math.Sin(_timeOfDay * Math.PI) * 50;
                
                spriteBatch.Draw(
                    _moonTexture,
                    new Vector2(moonX, moonY),
                    null,
                    new Color(1f, 1f, 1f, moonAlpha),
                    0f,
                    new Vector2(16),
                    1.5f,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Рисуем цветовой оверлей
            Color overlayColor = GetOverlayColor();
            
            if (overlayColor.A > 0)
            {
                spriteBatch.Draw(
                    _overlayTexture,
                    Vector2.Zero,
                    null,
                    overlayColor,
                    0f,
                    Vector2.Zero,
                    new Vector2(1280, 720),
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private Color GetOverlayColor()
        {
            float t = _timeOfDay;
            
            if (t < 0.25f) // Ночь -> Рассвет
            {
                float lerp = t / 0.25f;
                return LerpColor(_nightColor, _dawnColor, lerp);
            }
            else if (t < 0.35f) // Рассвет
            {
                float lerp = (t - 0.25f) / 0.1f;
                return LerpColor(_dawnColor, _dayColor, lerp);
            }
            else if (t < 0.65f) // День
            {
                return _dayColor;
            }
            else if (t < 0.75f) // Закат
            {
                float lerp = (t - 0.65f) / 0.1f;
                return LerpColor(_dayColor, _duskColor, lerp);
            }
            else // Сумерки -> Ночь
            {
                float lerp = (t - 0.75f) / 0.25f;
                return LerpColor(_duskColor, _nightColor, lerp);
            }
        }
        
        private Color LerpColor(Color from, Color to, float amount)
        {
            return new Color(
                from.R + (byte)((to.R - from.R) * amount),
                from.G + (byte)((to.G - from.G) * amount),
                from.B + (byte)((to.B - from.B) * amount),
                from.A + (byte)((to.A - from.A) * amount)
            );
        }
        
        public void Dispose()
        {
            _overlayTexture?.Dispose();
            _starTexture?.Dispose();
            _moonTexture?.Dispose();
        }
        
        private struct Star
        {
            public float X;
            public float Y;
            public int Size;
            public float TwinkleSpeed;
            public float TwinkleOffset;
        }
    }
}
