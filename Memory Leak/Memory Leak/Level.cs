﻿using System;
using System.IO;
using System.Xml.Linq;
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
        private class LoadedLevel
        {
            public readonly XDocument Root;

            public LoadedLevel(FileStream fs)
            {
                Root = XDocument.Load(fs);
            }

            public XElement Map
            {
                get { return Root.Element("map"); }
            }

            public XElement Tileset
            {
                get { return Map.Element("tileset"); }
            }

            public int Width
            {
                get { return int.Parse(Map.Attribute("width").Value); }
            }

            public int Height
            {
                get { return int.Parse(Map.Attribute("height").Value); }
            }
        }
        public static State Load(string fileName, Fader fader) //TODO: Actually load from file
        {
            fader.Timestep = 0.1f;
            fader.FadeIn();

            //disabled because otherwise it gets annoying to run the game while listening to music and stuff
            //Resource<Sound>.Get("BGM/HonorForAll", "mp3").IsLooped = true;
            //Resource<Sound>.Get("BGM/HonorForAll", "mp3").Play();

            string file = Path.Combine("Content/Maps/", fileName);
            var level = new LoadedLevel(new FileStream(file, FileMode.Open));

            var chunk = new Chunk(level.Width, level.Height);
            var camera = new Camera();

            for (int x = 0; x < chunk.Width; x++)
                for (int y = 0; y < chunk.Height; y++)
                {
                    //int xSource = id % (_texture.Width / Map.TileWidth);
                    //int ySource = id / (_texture.Width / Map.TileWidth);

                    int tx = 0;
                    int ty = 0;
                    int tw = 64;
                    int th = 64;

                    var tile = new Chunk.Tile(Resource<Texture2D>.Get("debug"), tx, ty, tw, th);
                    tile.AddProperty("IsPassable", true);
                    tile.AddProperty("FrictionMultiplier", 1);
                    chunk.Set(x, y, 0, tile);
                }

            var player = new Physical(Resource<Texture2D>.Get("debug-entity"), 2, 2, 0);
            var otherDude = new Physical(Resource<Texture2D>.Get("debug-entity"), 6, 6, 0);

            float time = 0;
            otherDude.Tick += f =>
                {
                    time += f * 0.25f;
                    otherDude.Move((int)Math.Round(Math.Cos(time * Math.PI * 2)), (int)Math.Round(Math.Sin(time * Math.PI * 2)), 100 * f);
                };

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
