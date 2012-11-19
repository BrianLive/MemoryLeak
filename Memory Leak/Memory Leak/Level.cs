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

            public Dictionary<string, dynamic> ObjectGroups
            {
                get
                {
                    var result = new Dictionary<string, dynamic>();
                    var objectgroups = Map.Elements("objectgroup").ToList();

                    List<dynamic> entities = new List<dynamic>();

                    foreach (var objectgroup in objectgroups)
                    {
                        var id = objectgroup.Attribute("name").Value;
                        var objects = objectgroup.Elements("object");

                        foreach (var obj in objects)
                        {
                            string name = null;
                            if (obj.Attribute("name") != null) //Not all objects have a name
                                name = obj.Attribute("name").Value;

                            int? gid = null; //Nullable types like a boss
                            if (obj.Attribute("gid") != null) //Not all object have a GID (Global I'd Dump)
                                gid = int.Parse(obj.Attribute("gid").Value);

                            //TODO: Get gid (but might not be needed at all)

                            int x = int.Parse(obj.Attribute("x").Value);
                            int y = int.Parse(obj.Attribute("y").Value);

                            int? w = null, h = null; 

                            if (obj.Attribute("width") != null && obj.Attribute("height") != null) //Not all objects have a width and height
                            {
                                w = int.Parse(obj.Attribute("width").Value);
                                h = int.Parse(obj.Attribute("height").Value);
                            }

                            var prop = obj.Element("properties");
                            Dictionary<string, string> props = null;

                            if (prop != null) //Not all objects have a <properties> tag
                            {
                                props = new Dictionary<string, string>();
                                var properties = prop.Elements("property");

                                foreach (var property in properties)
                                {
                                    string key = property.Attribute("name").Value;
                                    string value = property.Attribute("value").Value;

                                    props[key] = value; //This is amazingly elegant
                                }
                            }

                            Point? size = null;
                            if (w.HasValue && h.HasValue)
                                size = new Point(w.Value, h.Value);

                            dynamic entity = new //SAY ONE MORE THING ABOUT DYNAMIC. ONE. MORE. THING.
                            {
                                Name = name,
                                GID = gid,
                                Position = new Point(x, y),
                                Size = size,
                                Properties = props
                            };

                            entities.Add(entity);
                        }

                        result[id] = new { Entities = entities };
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
            var objectgroups = level.ObjectGroups;

            foreach (var layer in layers)
            {
                int[] tiles = layer.Value.Tiles;
                int z = int.Parse(layer.Key);

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

                        chunk.Set(x, y, z, tile);
                    }
            }

            Physical player = null; //This gets set down here

            foreach (var objectgroup in objectgroups)
            {
                var objects = objectgroup.Value.Entities;
                int z = int.Parse(objectgroup.Key); //TODO: Does this even work too?

                foreach (var obj in objects)
                {
                    string name = obj.Name;

                    bool isPassable = false;
                    bool isRamp = false;

                    if (obj.Properties != null)
                    {
                        isRamp = bool.Parse(obj.Properties["IsRamp"]);
                        isPassable = bool.Parse(obj.Properties["IsPassable"]);
                    }

                    int x = (int)(obj.Position.X / 32f) - 0; //This is...
                    int y = (int)(obj.Position.Y / 32f) - 1; //Pretty wonky.

                    if (isRamp) //If it's a ramp, we don't create an entity at all, but a tile!
                    {
                        Texture2D tex = Resource<Texture2D>.Get(level.Tileset.Image);

                        int id = obj.GID;

                        //Texture coordinates

                        int tx = (int)(id * 16); //why does this work
                        int ty = 0;
                        int tw = 32;
                        int th = 32;

                        var tile = new Chunk.Tile(tex, tx, ty, tw, th);

                        foreach (var property in obj.Properties)
                            tile.AddProperty(property.Key, property.Value);

                        chunk.Set(x, y, z, tile); 
                    }
                    else
                    {
                        Physical entity;

                        if (name == "player.Start") //TODO: Maybe make these "hardcoded" entities a lookup table of prefabs?
                        {
                            entity = new Physical(Resource<Texture2D>.Get("debug-entity"), x, y, z);
                            player = entity;
                        }
                        else
                            entity = new Physical(Resource<Texture2D>.Get("debug-entity"), //TODO: Read texture from entity
                                                  x, y, z,
                                                  isPassable);

                        //TODO: For now, entities don't seem to have a way to set properties...

                        chunk.Add(entity);
                    }
                }
            }

            for (int i = 0; i < 0; i++) //LOOK AT ALL THOSE DUDES HAVING FUN
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

            if (player == null)
                throw new Exception("Well, well, well. Looks like SOMEONE forgot to put a player.Start in the level. Have fun with all your NullReferenceExceptions down here.");

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
