using System;
using MemoryLeak.Audio;
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
            fader.Timestep = 0.1f;
            fader.FadeIn();

            //disabled because otherwise it gets annoying to run the game while listening to music and stuff
            Resource<Sound>.Get("austin_beatbox").IsLooped = true;
            Resource<Sound>.Get("austin_beatbox").Play();

            var chunk = new Chunk(32, 32);
            var camera = new Camera();

            for(int x = 0; x < 32; x++)
                for(int y = 0; y < 32; y++)
                {
                    var tile = new Chunk.Tile(Resource<Texture2D>.Get("debug"), 0, 0, 32, 32);
                    tile.AddProperty("IsPassable", true);
                    tile.AddProperty("FrictionMultiplier", 1);
                    chunk.Set(x, y, 0, tile);
                }

            var player = new Physical(Resource<Texture2D>.Get("debug-entity"), 2, 2, 0);
            var otherDude = new Physical(Resource<Texture2D>.Get("debug-entity"), 3, 3, 0);
            otherDude.Tick += f => otherDude.Move(1, 0, 100*f);
            chunk.Add(otherDude);

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
