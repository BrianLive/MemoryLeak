using System;
using MemoryLeak.Core;
using MemoryLeak.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MemoryLeak
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        public class Core
        {
            public static GraphicsDeviceManager GraphicsDeviceManager { get; internal set; }
            public static GraphicsDevice GraphicsDevice { get { return GraphicsDeviceManager.GraphicsDevice; } }

            private static Vector2 _resolution = new Vector2();

            public static Vector2 Resolution
            {
                get { return _resolution; }
                set
                {
                    GraphicsDeviceManager.PreferredBackBufferWidth = (int)value.X;
                    GraphicsDeviceManager.PreferredBackBufferHeight = (int)value.Y;
                    _resolution = new Vector2(value.X, value.Y);
                }
            }
        }

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public State CurrentState { get; set; }

        public Game()
        {
            IsFixedTimeStep = false;

            _graphics = new GraphicsDeviceManager(this);
            Core.GraphicsDeviceManager = _graphics;
            Core.Resolution = new Vector2(1280, 720);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            CurrentState = LoadDebugMap();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            const float maxDt = 1 / 60.0f; //Our max deltatime
            const int maxSteps = 10; //Limit the amount of times we update the game per frame
            int stepCounter = 0;

            float frameTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            while (frameTime > 0) // fuck your comments
            {
                if (stepCounter >= maxSteps)
                {
                    Console.WriteLine("Too much lag! Slowing simulation down...");
                    break; //Avoid spiral of death by slowing simulation down
                }

                float deltaTime = Math.Min(frameTime, maxDt);

                CurrentState.Update(deltaTime);
                base.Update(gameTime);

                frameTime -= deltaTime;
                stepCounter++;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            CurrentState.Draw(_spriteBatch);

            base.Draw(gameTime);
        }

        private static void ElevateLand(Chunk chunk, int x, int y, int width, int height, int start)
        {
            for (var xx = x; xx < (x + width); xx++)
                for (var yy = y; yy < (y + height); yy++)
                {
                    chunk.Set(xx, yy, start, null);

                    chunk.Set(xx, yy, start + 1, new Chunk.Tile(Resource<Texture2D>.Get("debug-two")) { IsPassable = true });
                    chunk.Set(xx, yy, start + 1, new Chunk.Tile(Resource<Texture2D>.Get("debug-two")) { IsPassable = true });

                    if (xx == (x + (width / 2)) && yy == y)
                    {
                        chunk.Set(xx, yy, start, new Chunk.Tile(Resource<Texture2D>.Get("debug-ramp")) { IsPassable = true, IsRamp = true, IsRampHorizontal = false, IsRampUpNegative = false });
                        chunk.Set(xx, yy, start + 1, null);
                        continue;
                    }

                    if (xx == (x + width - 1) && yy == (y + (height / 2)))
                    {
                        chunk.Set(xx, yy, start, new Chunk.Tile(Resource<Texture2D>.Get("debug-ramp")) { IsPassable = true, IsRamp = true, IsRampHorizontal = true, IsRampUpNegative = true });
                        chunk.Set(xx, yy, start + 1, null);
                        continue;
                    }

                    if (xx == (x + (width / 2)) && yy == (y + height - 1))
                    {
                        chunk.Set(xx, yy, start, new Chunk.Tile(Resource<Texture2D>.Get("debug-ramp")) { IsPassable = true, IsRamp = true, IsRampHorizontal = false, IsRampUpNegative = true });
                        chunk.Set(xx, yy, start + 1, null);
                        continue;
                    }

                    if (xx == x && yy == (y + (height/2)))
                    {
                        chunk.Set(xx, yy, start, new Chunk.Tile(Resource<Texture2D>.Get("debug-ramp")) { IsPassable = true, IsRamp = true, IsRampHorizontal = true, IsRampUpNegative = false });
                        chunk.Set(xx, yy, start + 1, null);
                        continue;
                    }
                }
        }

        private static void CreateBridge(Chunk chunk, int x, int y, int width, int height, int start)
        {
            for (var xx = x; xx < (x + width); xx++)
                for (var yy = y; yy < (y + height); yy++)
                {
                    chunk.Set(xx, yy, start, new Chunk.Tile(Resource<Texture2D>.Get("debug-two")) {IsFloater = true, IsFloaterLayered = true, IsPassable = true});
                }
        }

        private static State LoadDebugMap()
        {
            //disabled because otherwise it gets annoying to run the game while listening to music and stuff
            //Resource<Sound>.Get("austin_beatbox").IsLooped = true;
            //Resource<Sound>.Get("austin_beatbox").Play();

            var chunk = new Chunk(64, 64, 5);
            var camera = new Camera();

            for (var x = 0; x < chunk.Width; x++)
                for (var y = 0; y < chunk.Height; y++ )
                    chunk.Set(x, y, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug")) {IsPassable = true});

            ElevateLand(chunk, 5, 5, 19, 19, 0);
            ElevateLand(chunk, 7, 7, 15, 15, 1);
            ElevateLand(chunk, 9, 9, 11, 11, 2);
            ElevateLand(chunk, 11, 11, 7, 7, 3);

            ElevateLand(chunk, 30, 5, 19, 19, 0);
            ElevateLand(chunk, 32, 7, 15, 15, 1);
            ElevateLand(chunk, 34, 9, 11, 11, 2);
            ElevateLand(chunk, 36, 11, 7, 7, 3);

            CreateBridge(chunk, 23, 10, 8, 5, 1);

            var player = new Physical(Resource<Texture2D>.Get("debug-entity"), 2, 2, 0);

            player.Tick += dt =>
            {
                var k = Keyboard.GetState();

                var move = Vector2.Zero;

                foreach (var i in k.GetPressedKeys())
                {
                    switch(i)
                    {
                        case Keys.W:
                            move.Y = -1;
                            break;
                        case Keys.S:
                            move.Y = 1;
                            break;
                        case Keys.A:
                            move.X = -1;
                            break;
                        case Keys.D:
                            move.X = 1;
                            break;
                        case Keys.LeftShift: 
                            Console.WriteLine(player.Depth);
                            break;
                    }
                }

                if(move != Vector2.Zero) player.Move((int)move.X, (int)move.Y, 200 * dt);

                camera.Position = player.Position;
            };

            chunk.Add(player);

            return new State(chunk, camera) {Player = player};
        }
    }
}
