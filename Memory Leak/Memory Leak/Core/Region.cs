using System.Collections.Generic;
using System.Drawing;
using MemoryLeak.Utility;

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
		private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();

		public Region(int x, int y, int width, int height)
		{
			Area = new RectangleF(x * Chunk.Tile.Width, y * Chunk.Tile.Height,
								width * Chunk.Tile.Width, height * Chunk.Tile.Height);
		}

        public void AddProperty(string name, object value)
        {
            _properties.Add(name, new Property(value));
        }

	    public Property HasProperty(string name)
	    {
            return _properties.ContainsKey(name) ? _properties[name] : Property.Empty;
	    }
	}
}
