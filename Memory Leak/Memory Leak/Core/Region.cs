using System;
using System.Collections.Generic;
using System.Drawing;

namespace MemoryLeak.Core
{
	public class Region
	{
		public RectangleF Area { get; set; }

		// TODO: pls more efficient than this - Rohan
        // TODO: suck a dick - Brian
        /// <summary>
        /// Dictionary of properties and their values.
        /// </summary>
		private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

		public Region(int x, int y, int width, int height)
		{
			Area = new RectangleF(x * Chunk.Tile.Width, y * Chunk.Tile.Height,
								width * Chunk.Tile.Width, height * Chunk.Tile.Height);
		}

        public void AddProperty(string name, object value = null)
        {
            _properties.Add(name, value);
        }

	    public T HasProperty<T>(string name) where T : struct, IComparable<T>
	    {
	        var o = (_properties.ContainsKey(name) ? _properties[name] : null);
	        if (o != null)
	            return (T) o;

	        return default(T);
	    }
	}
}
