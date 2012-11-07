using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Graphics
{
    public class DebugRectangle : Drawable
    {
        public DebugRectangle() : base(Resource<Texture2D>.Get("transparent"))
        {
            Color = Color.Red;
        }
    }
}
