using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace mario_monogame.Core.Extensions
{
    /// <summary>
    /// Extension методы для GraphicsDevice.
    /// </summary>
    public static class GraphicsDeviceExtensions
    {
        private static readonly Dictionary<GraphicsDevice, Texture2D> _whitePixelCache = new Dictionary<GraphicsDevice, Texture2D>();
        
        /// <summary>
        /// Получить белый пиксель текстуры (кэшируется).
        /// </summary>
        public static Texture2D GetWhitePixel(this GraphicsDevice device)
        {
            if (!_whitePixelCache.TryGetValue(device, out var texture))
            {
                texture = new Texture2D(device, 1, 1);
                texture.SetData(new[] { Color.White });
                _whitePixelCache[device] = texture;
            }
            
            return texture;
        }
    }
}
