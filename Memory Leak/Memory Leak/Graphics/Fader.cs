using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Graphics
{
    class Fader : SpriteBatch
    {
        private readonly Texture2D _whiteTexture;
        private int _alpha;
        private float _time;
        public float Timestep { get; set; }
        private int _mode;

        public Fader(GraphicsDevice graphicsDevice, float tStep)
            : base(graphicsDevice)
        {

            // This is the big texture we use to mask the screen.
            _whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.Black });

            _alpha = 0;
            _time = 0f;
            _mode = 0;
            Timestep = tStep;
        }

        /// <summary>
        /// Update the easing properties of the fading mask.
        /// </summary>
        public void Update()
        {
            if (_mode == 1)         // ease in
            {
                _time += Timestep;
                CubicEaseIn();
                if (GetCubic(_time) >= 255)     // if the time is just going to generate a +255 alpha then there is no point in further calcs
                {
                    _mode = 0;
                }
            }
            else if (_mode == 2)        // ease out
            {
                _time += Timestep;
                CubicEaseOut();
                if (GetCubic(_time) >= 255)     // see above
                {
                    _mode = 0;
                }
            }
        }


        /// <summary>
        /// Initiate fade from black to transparrent
        /// </summary>
        public void FadeIn()
        {
            _time = 0;
            _alpha = 255;
            _mode = 1;

        }

        /// <summary>
        /// Initiate fade from transparrent to black
        /// </summary>
        public void FadeOut()
        {
            _time = 0;
            _alpha = 0;
            _mode = 2;
        }

        /// <summary>
        /// Draw the fading mask.
        /// </summary>
        public void DrawFader()
        {
            Color col = Color.Black;
            col.A = (byte)_alpha;
            Draw(_whiteTexture, new Rectangle(0, 0, Game.Core.GraphicsDevice.Viewport.Width, Game.Core.GraphicsDevice.Viewport.Height), col);
        }

        private float GetCubic(float t)
        {
            return MathHelper.Clamp(t * t * t, 0, 255);
        }

        /// <summary>
        /// changes the alpha value of the mask using the current time frame.
        /// </summary>
        private void CubicEaseOut()
        {
            _alpha = (byte)GetCubic(_time);
        }

        /// <summary>
        /// Changes the alpha value of the mash using the current time frame - 255 to inverse it.
        /// </summary>
        private void CubicEaseIn()
        {
            _alpha = 255 - (byte)GetCubic(_time);
        }
    }
}
