
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Core
{
    public class State
    {
        public Chunk Chunk { get; set; }
        public Camera Camera { get; set; }

        public State(Chunk chunk = null, Camera camera = null)
        {
            Chunk = chunk ?? new Chunk(32, 32);
            Chunk.Parent = this;
            Camera = camera ?? new Camera();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix);
            Chunk.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {
            Chunk.Update(gameTime);
        }
    }
}
