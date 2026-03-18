using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace mario_monogame.Core.GameObjects
{
    /// <summary>
    /// Платформа в стиле Mario.
    /// </summary>
    public class Platform
    {
        public Rectangle Bounds { get; set; }
        public PlatformType Type { get; set; }
        public bool IsSolid { get; set; }
        public bool IsBreakable { get; set; }
        public bool IsUsed { get; set; }
        
        private Texture2D _texture;
        private readonly GraphicsDevice _graphicsDevice;
        
        public Platform(GraphicsDevice graphicsDevice, Rectangle bounds, PlatformType type)
        {
            _graphicsDevice = graphicsDevice;
            Bounds = bounds;
            Type = type;
            IsSolid = true;
            IsBreakable = false;
            IsUsed = false;
            
            CreateTexture();
        }
        
        private void CreateTexture()
        {
            _texture = new Texture2D(_graphicsDevice, Bounds.Width, Bounds.Height);
            Color[] pixels = new Color[Bounds.Width * Bounds.Height];
            
            switch (Type)
            {
                case PlatformType.Ground:
                    CreateGroundTexture(pixels);
                    break;
                case PlatformType.Brick:
                    CreateBrickTexture(pixels);
                    break;
                case PlatformType.Question:
                    CreateQuestionTexture(pixels);
                    break;
                case PlatformType.Block:
                    CreateBlockTexture(pixels);
                    break;
                case PlatformType.Pipe:
                    CreatePipeTexture(pixels);
                    break;
            }
            
            _texture.SetData(pixels);
        }
        
        private void CreateGroundTexture(Color[] pixels)
        {
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    // Коричневая земля с текстурой
                    int brickSize = 16;
                    bool isLine = (y % brickSize == 0) || (x % (brickSize * 2) == 0 && (y / brickSize) % 2 == 0);
                    
                    if (isLine)
                    {
                        pixels[y * Bounds.Width + x] = new Color(80, 50, 30);
                    }
                    else
                    {
                        float noise = ((x * 7 + y * 13) % 20) / 20f;
                        byte brown = (byte)(120 + noise * 40);
                        pixels[y * Bounds.Width + x] = new Color(brown, (byte)(80 + noise * 20), (byte)(40 + noise * 10));
                    }
                }
            }
        }
        
        private void CreateBrickTexture(Color[] pixels)
        {
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    int brickSize = 16;
                    bool isLine = (y % 4 == 0) || (x % brickSize == 0 && (y / 4) % 2 == 0);
                    
                    if (isLine)
                    {
                        pixels[y * Bounds.Width + x] = new Color(60, 30, 20);
                    }
                    else
                    {
                        float noise = ((x * 7 + y * 13) % 15) / 15f;
                        byte red = (byte)(180 + noise * 40);
                        pixels[y * Bounds.Width + x] = new Color(red, (byte)(80 + noise * 30), (byte)(40 + noise * 20));
                    }
                }
            }
        }
        
        private void CreateQuestionTexture(Color[] pixels)
        {
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    // Золотой блок
                    float noise = ((x * 7 + y * 13) % 20) / 20f;
                    byte gold = (byte)(200 + noise * 55);
                    pixels[y * Bounds.Width + x] = new Color(gold, (byte)(gold - 50), (byte)(50 + noise * 30));
                }
            }
            
            // Вопрос
            if (!IsUsed)
            {
                int centerX = Bounds.Width / 2;
                int centerY = Bounds.Height / 2;
                
                for (int y = 8; y < Bounds.Height - 8; y++)
                {
                    for (int x = centerX - 3; x <= centerX + 3; x++)
                    {
                        if (y < Bounds.Height / 2 - 2 || y > Bounds.Height / 2 + 2 || x < centerX - 1 || x > centerX + 1)
                        {
                            int idx = y * Bounds.Width + x;
                            if (idx >= 0 && idx < pixels.Length)
                            {
                                pixels[idx] = new Color(100, 60, 20);
                            }
                        }
                    }
                }
                
                // Точка вопроса
                for (int y = Bounds.Height / 2 + 4; y < Bounds.Height / 2 + 8; y++)
                {
                    for (int x = centerX - 2; x <= centerX + 2; x++)
                    {
                        int idx = y * Bounds.Width + x;
                        if (idx >= 0 && idx < pixels.Length)
                        {
                            pixels[idx] = new Color(100, 60, 20);
                        }
                    }
                }
            }
        }
        
        private void CreateBlockTexture(Color[] pixels)
        {
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    // Серый блок
                    byte gray = (byte)(150 + ((x + y) % 20));
                    pixels[y * Bounds.Width + x] = new Color(gray, gray, gray);
                }
            }
        }
        
        private void CreatePipeTexture(Color[] pixels)
        {
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    // Зелёная труба
                    bool isEdge = x < 4 || x > Bounds.Width - 4 || y < 4 || (y == Bounds.Height - 1);
                    
                    if (isEdge)
                    {
                        pixels[y * Bounds.Width + x] = new Color(0, 100, 0);
                    }
                    else
                    {
                        float gradient = x / (float)Bounds.Width;
                        byte green = (byte)(50 + gradient * 100);
                        pixels[y * Bounds.Width + x] = new Color((byte)(50 + gradient * 50), green, 50);
                    }
                }
            }
        }
        
        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Rectangle drawRect = new Rectangle(
                Bounds.X - (int)cameraPosition.X,
                Bounds.Y - (int)cameraPosition.Y,
                Bounds.Width,
                Bounds.Height
            );
            
            // Не рисуем если за пределами экрана
            if (drawRect.Right < 0 || drawRect.Left > 1280 || drawRect.Bottom < 0 || drawRect.Top > 720)
                return;
            
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, drawRect, Color.White);
            }
            else
            {
                // Fallback
                spriteBatch.Draw(_pixelTexture, drawRect, Color.Brown);
            }
        }
        
        public void Break()
        {
            IsSolid = false;
        }
        
        public void Use()
        {
            IsUsed = true;
            Type = PlatformType.UsedQuestion;
            CreateTexture();
        }
        
        private Texture2D _pixelTexture;
    }
    
    public enum PlatformType
    {
        Ground,
        Brick,
        Question,
        UsedQuestion,
        Block,
        Pipe
    }
}
