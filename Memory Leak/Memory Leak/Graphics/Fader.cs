using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Graphics
{
    class Fader : SpriteBatch
    {
        private Texture2D _WhiteTexture;
        private int _alpha;
        private float _time;
        public float _timestep { get; set; }
        private int _mode;

        public Fader(GraphicsDevice graphicsDevice, float t_step)
            : base(graphicsDevice)
        {

            // This is the big texture we use to mask the screen.
            this._WhiteTexture = new Texture2D(graphicsDevice, 1, 1);
            this._WhiteTexture.SetData(new Color[] { Color.Black });

            _alpha = 0;
            _time = 0f;
            _mode = 0;
            _timestep = t_step;
        }

        /// <summary>
        /// Update the easing properties of the fading mask.
        /// </summary>
        public void Update()
        {
            if (_mode == 1)         // ease in
            {
                _time += _timestep;
                CubicEaseIn();
                if (GetCubic(_time) >= 255)     // if the time is just going to generate a +255 alpha then there is no point in further calcs
                {
                    _mode = 0;
                }
            }
            else if (_mode == 2)        // ease out
            {
                _time += _timestep;
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
            this.Draw(this._WhiteTexture, new Rectangle(0, 0, MemoryLeak.Game.Core.GraphicsDevice.Viewport.Width, MemoryLeak.Game.Core.GraphicsDevice.Viewport.Height), col);
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
