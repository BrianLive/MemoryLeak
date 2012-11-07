using MemoryLeak.Audio;
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

        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        public State CurrentState { get; set; }

        public Game()
        {
            // DON'T DO INITIALIZATION STUFF HERE, DO IT IN LOAD CONTENT gosh - Brian
            _graphics = new GraphicsDeviceManager(this);
            Core.GraphicsDeviceManager = _graphics;
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

            CurrentState.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            CurrentState.Draw(_spriteBatch);

            base.Draw(gameTime);
        }

        private static State LoadDebugMap()
        {
            Resource<Sound>.Get("austin_beatbox").IsLooped = true;
            Resource<Sound>.Get("austin_beatbox").Play();

            var chunk = new Chunk(32, 32);
            var camera = new Camera();

            for (var x = 0; x < chunk.Width; x++)
                for (var y = 0; y < chunk.Height; y++ )
                    if (RandomWrapper.Range() > 0.2) chunk.Set(x, y, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug")) {IsPassable = true});

            var player = new Entity(Resource<Texture2D>.Get("debug-entity"), RandomWrapper.Range(10), RandomWrapper.Range(10), 0);

            player.Tick += sender =>
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

                                   player.Move((int) move.X, (int) move.Y, isRunning ? 3 : 1);
                                   if (move != Vector2.Zero) camera.Position = player.Position;
                               };

            camera.Position = player.Position;

            chunk.Add(player);

            return new State(chunk, camera);
        }
    }
}
