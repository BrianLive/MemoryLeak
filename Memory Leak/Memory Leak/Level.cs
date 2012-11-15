using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MemoryLeak.Audio;
using MemoryLeak.Core;
using MemoryLeak.Entities;
using MemoryLeak.Graphics;
using MemoryLeak.Utility;
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

            public dynamic Tileset
            {
                get
                {
                    var tileset = Map.Element("tileset");
                    var img = tileset.Element("image");
                    var tiles = tileset.Elements("tile");

                    var tileDefinitions = new Dictionary<int, Dictionary<string, string>>(); //MY EYES

                    foreach (var tile in tiles)
                    {
                        int tileID = int.Parse(tile.Attribute("id").Value);
                        var properties = tile.Element("properties").Elements("property");

                        var propertyDict = new Dictionary<string, string>();
                        foreach (var property in properties)
                        {
                            string propertyName = property.Attribute("name").Value;
                            string propertyValue = property.Attribute("value").Value;

                            propertyDict[propertyName] = propertyValue;
                        }

                        tileDefinitions[tileID] = propertyDict;
                    }

                    return new
                    {
                        TileData = tileDefinitions,
                        Image = Path.GetFileNameWithoutExtension(img.Attribute("source").Value),
                        Width = int.Parse(img.Attribute("width").Value),
                        Height = int.Parse(img.Attribute("height").Value)
                    };
                }
            }

            public Dictionary<string, dynamic> Layers
            {
                get
                {
                    var result = new Dictionary<string, dynamic>();
                    var layers =  Map.Elements("layer").ToList();

                    foreach (var layer in layers)
                    {
                        var id = layer.Attribute("name").Value;

                        string indecipherableMess = layer.Element("data").Value; //This is a compressed and base64 encoded array of ints, which are actually the tile ids
                        byte[] bytes = Convert.FromBase64String(indecipherableMess); //Base64 decode
                        
                        var resultStream = new MemoryStream();

                        using (Stream zipStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
                        {
                            byte[] buffer = new byte[1024];
                            int nRead;

                            while ((nRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                                resultStream.Write(buffer, 0, nRead); //Read gzip buffer to decompress
                        }

                        var byteTiles = resultStream.ToArray(); 
                        var intTiles = new int[byteTiles.Length / 4];

                        for (int i = 0; i < byteTiles.Length; i += 4)
                            intTiles[i / 4] = BitConverter.ToInt32(byteTiles, i); //Convert byte array to int array

                        resultStream.Dispose();

                        result[id] = new { Tiles = intTiles };
                    }

                    return result;
                }
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
            var layers = level.Layers;

            foreach (var layer in layers)
            {
                int[] tiles = layer.Value.Tiles;

                for (int x = 0; x < chunk.Width; x++)
                    for (int y = 0; y < chunk.Height; y++)
                    {
                        Texture2D tex = Resource<Texture2D>.Get(level.Tileset.Image);

                        int id = tiles[x + y * chunk.Width];

                        //Texture coordinates

                        int tx = (int)(id * 32 / 1.5); //why does this work
                        int ty = 0;
                        //int tx = (id % (tex.Width / 32)) * 32;
                        //int ty = (id / (tex.Width / 32)) * 32;
                        int tw = 32;
                        int th = 32;

                        var tile = new Chunk.Tile(tex, tx, ty, tw, th);

                        Dictionary<string, string> properties;
                        if (level.Tileset.TileData.TryGetValue(id, out properties)) //If id isn't found, that means the tile id has no properties defined
                            foreach (var property in properties)
                                tile.AddProperty(property.Key, property.Value); //If id IS found, set all properties

                        tile.AddProperty("FrictionMultiplier", 1);

                        chunk.Set(x, y, 0, tile);
                    }
            }

            var player = new Physical(Resource<Texture2D>.Get("debug-entity"), 3, 3, 0);

            for (int i = 0; i < 10; i++) //LOOK AT ALL THOSE DUDES HAVING FUN
            {
                var otherDude = new Physical(Resource<Texture2D>.Get("debug-entity"), RandomHelper.Range(3, 16-3), RandomHelper.Range(3, 16-3), 0);

                float time = RandomHelper.Range();
                otherDude.Tick += f =>
                    {
                        time += f * 0.25f;
                        otherDude.Move((int)Math.Round(Math.Cos(time * Math.PI * 2)), (int)Math.Round(Math.Sin(time * Math.PI * 2)), 100 * f);
                    };

                chunk.Add(otherDude);
            }

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
