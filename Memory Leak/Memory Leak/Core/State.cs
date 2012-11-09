using MemoryLeak.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Core
{
    public class State
    {
        public Chunk Chunk { get; set; }
        public Camera Camera { get; set; }
        public Physical Player { get; set; }

        public State(Chunk chunk = null, Camera camera = null)
        {
            Chunk = chunk ?? new Chunk(32, 32);
            Chunk.Parent = this;
            Camera = camera ?? new Camera();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix);
            Chunk.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void Update(float delta)
        {
            Chunk.Update(delta);
        }
    }
}
