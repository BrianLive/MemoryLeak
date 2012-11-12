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
            fader._timestep = 0.1f;
            fader.FadeIn();
            //disabled because otherwise it gets annoying to run the game while listening to music and stuff
            //Resource<Sound>.Get("austin_beatbox").IsLooped = true;
            //Resource<Sound>.Get("austin_beatbox").Play();

            var chunk = new Chunk(64, 64, 5);
            var camera = new Camera();

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
