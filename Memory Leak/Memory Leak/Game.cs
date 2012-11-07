using System;
using System.Diagnostics;
using MemoryLeak.Core;
using MemoryLeak.Utility;
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

            public static Vector2 Resolution
            {
                get { return new Vector2(GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight); }
                set
                {
                    GraphicsDeviceManager.PreferredBackBufferWidth = (int)value.X;
                    GraphicsDeviceManager.PreferredBackBufferHeight = (int)value.Y;
                }
            }
        }

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //private readonly Stopwatch _stopwatch = new Stopwatch();
        //private double _previous;

        public State CurrentState { get; set; }

        public Game()
        {
            IsFixedTimeStep = false;

            _graphics = new GraphicsDeviceManager(this);
            Core.GraphicsDeviceManager = _graphics;

            //_stopwatch.Start();
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
            //double current = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency; 

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            float maxDT = 1 / 60.0f; //Our max deltatime
            int maxSteps = 10; //Limit the amount of times we update the game per frame
            int stepCounter = 0;

            float frameTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            while (frameTime > 0) //Clever timestep shit stolen from http://gafferongames.com/game-physics/fix-your-timestep/ (and also from Froid and Space Hazard)
            {
                if (stepCounter >= maxSteps)
                {
                    Console.WriteLine("Too much lag! Slowing simulation down...");
                    break; //Avoid spiral of death by slowing simulation down
                }

                float deltaTime = Math.Min(frameTime, maxDT);

                CurrentState.Update(deltaTime);
                base.Update(gameTime);

                frameTime -= deltaTime;
                stepCounter++;

            }

            //_previous = _stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
            //_stopwatch.Restart();

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

            var chunk = new Chunk(32, 32);
            var camera = new Camera();

            for (var x = 0; x < chunk.Width; x++)
                for (var y = 0; y < chunk.Height; y++ )
                    if (RandomWrapper.Range() > 0.2) chunk.Set(x, y, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug")) {IsPassable = true});

            var player = new Entity(Resource<Texture2D>.Get("debug-entity"), RandomWrapper.Range(10), RandomWrapper.Range(10), 0);

            player.Tick += dt =>
                               {
                                   var k = Keyboard.GetState();
                                   var isRunning = k.IsKeyDown(Keys.LeftShift);

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
                                       }
                                   }

                                   Console.WriteLine(dt);
                                   player.Move((int) move.X, (int) move.Y, (isRunning ? 300 : 100) * dt);
                                   camera.Position = player.Position;
                               };

            chunk.Add(player);

            return new State(chunk, camera);
        }
    }
}
