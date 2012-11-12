using System;
using MemoryLeak.Core;
using MemoryLeak.Entities;
using MemoryLeak.Graphics;
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

            private static Vector2 _resolution;

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

        private static State LoadDebugMap()
        {
            //disabled because otherwise it gets annoying to run the game while listening to music and stuff
            //Resource<Sound>.Get("austin_beatbox").IsLooped = true;
            //Resource<Sound>.Get("austin_beatbox").Play();

            var chunk = new Chunk(64, 64, 5);
            var camera = new Camera();

            var houseRegion = new Region(3, 7, 8, 5);
            houseRegion.Properties.Add("isInside");

            chunk.Add(houseRegion);

            for (var x = 0; x < chunk.Width; x++)
                for (var y = 0; y < chunk.Height; y++ )
                    chunk.Set(x, y, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 0, 0, 32, 32) {IsPassable = true});

            for (int x = 3; x < 11; x++)
                for (int y = 7; y < 12; y++)
                    chunk.Set(x, y, 0, null);

            chunk.Set(3, 10, 2, new Chunk.Tile(Resource<Texture2D>.Get("house"), 0, 0, 32, 32) { IsFloater = true });
            for (int x = 4; x < 10; x++) chunk.Set(x, 10, 2, new Chunk.Tile(Resource<Texture2D>.Get("house"), 8, 0, 32, 32) { IsFloater = true });
            chunk.Set(10, 10, 2, new Chunk.Tile(Resource<Texture2D>.Get("house"), 32, 0, 32, 32) { IsFloater = true });

            chunk.Set(3, 11, 2, new Chunk.Tile(Resource<Texture2D>.Get("house"), 128, 32, 32, 32) { IsFloater = true });
            for (int x = 4; x < 10; x++) chunk.Set(x, 11, 2, new Chunk.Tile(Resource<Texture2D>.Get("house"), 160, 32, 32, 32) { IsFloater = true });
            chunk.Set(10, 11, 2, new Chunk.Tile(Resource<Texture2D>.Get("house"), 192, 32, 32, 32) { IsFloater = true });

            for (int x = 3; x < 11; x++)
                for (int y = 7; y < 10; y++)
                    chunk.Set(x, y, 4, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 64, 0, 32, 32) { IsFloater = true });

            for (int x = 3; x < 11; x++)
                for (int y = 7; y < 12; y++)
                    chunk.Set(x, y, 1, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 0, 0, 32, 32) {IsPassable = true});

            chunk.Set(5, 11, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 32, 0, 32, 32) {IsPassable = true, IsRamp = true, IsRampHorizontal = false, IsRampUpNegative = true});
            chunk.Set(5, 11, 1, null);
            chunk.Set(5, 11, 2, null);

            chunk.Set(6, 11, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 32, 0, 32, 32) { IsPassable = true, IsRamp = true, IsRampHorizontal = false, IsRampUpNegative = true });
            chunk.Set(6, 11, 1, null);
            chunk.Set(6, 11, 2, null);

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
