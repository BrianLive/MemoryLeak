using System.Collections.Generic;
using System.Drawing;

namespace MemoryLeak.Core
{
	public class Region
	{
		public RectangleF Area { get; set; }

		// TODO: pls more efficient than this - Rohan
		public List<string> Properties { get; set; }

		public Region(int x, int y, int width, int height)
		{
			Area = new RectangleF(x * Chunk.Tile.Width, y * Chunk.Tile.Height,
								width * Chunk.Tile.Width, height * Chunk.Tile.Height);

			Properties = new List<string>();
		}
	}
}
