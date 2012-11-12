using System;
using MemoryLeak.Core;
using MemoryLeak.Entities;
using MemoryLeak.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MemoryLeak
{
    class Level
    {
        public static State Load(string fileName, Fader fader) //TODO: Actually load from file
        {
            fader._timestep = 0.05f;
            fader.FadeIn();
            //disabled because otherwise it gets annoying to run the game while listening to music and stuff
            //Resource<Sound>.Get("austin_beatbox").IsLooped = true;
            //Resource<Sound>.Get("austin_beatbox").Play();

            var chunk = new Chunk(64, 64, 5);
            var camera = new Camera();

            var houseRegion = new Region(3, 7, 8, 5);
            houseRegion.Properties.Add("isInside");

            chunk.Add(houseRegion);

            for (var x = 0; x < chunk.Width; x++)
                for (var y = 0; y < chunk.Height; y++)
                    chunk.Set(x, y, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 0, 0, 32, 32) { IsPassable = true });

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
                    chunk.Set(x, y, 1, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 0, 0, 32, 32) { IsPassable = true });

            chunk.Set(5, 11, 0, new Chunk.Tile(Resource<Texture2D>.Get("debug"), 32, 0, 32, 32) { IsPassable = true, IsRamp = true, IsRampHorizontal = false, IsRampUpNegative = true });
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
                    switch (i)
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

                if (move != Vector2.Zero) player.Move((int)move.X, (int)move.Y, 200 * dt);

                camera.Position = player.CenterPosition + Vector2.One / 2; //Add (0.5, 0.5) to player position so we don't get shakyness (it works trust me DON'T REMOVE IT)
            };

            chunk.Add(player);

            return new State(chunk, camera) { Player = player };
        }
    }
}
