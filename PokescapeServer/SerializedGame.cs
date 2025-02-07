using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokescapeServer
{
    public class SerializedGame
    {
        public string GameId;
        public string gameState;
        public User user;
        public Dictionary<string, Block> currentGrid;
        public List<Dictionary<string, Block>> grids;

        public static JsonSerializerSettings gameSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> { new GameObjectConverter() }
        };

        public class GameObjectConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) =>
                typeof(Block).IsAssignableFrom(objectType) || typeof(Item).IsAssignableFrom(objectType);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jsonObject = JObject.Load(reader);
                string typeName = jsonObject["Type"]?.ToString();

                if (string.IsNullOrEmpty(typeName))
                {
                    throw new JsonSerializationException("Missing Type information for object deserialization.");
                }

                object instance = typeName switch
                {
                    // Blocks
                    "FloorBlock" => new FloorBlock(),
                    "StoneFloorBlock" => new StoneFloorBlock(),
                    "BlankBlock" => new BlankBlock(),
                    "StoneWallBlock" => new StoneWallBlock(),
                    "WaterBlock" => new WaterBlock(),
                    "Entrance" => new Entrance(),
                    "TopEntrance" => new TopEntrance(),
                    "BottomEntrance" => new BottomEntrance(),
                    "RightEntrance" => new RightEntrance(),
                    "LeftEntrance" => new LeftEntrance(),

                    // Items
                    "SpikeBerry" => new SpikeBerry(),
                    "OrangeGrape" => new OrangeGrape(),
                    "GuavaBerry" => new GuavaBerry(),
                    "BlueTwistItem" => new BlueTwistItem(),
                    "MysticOrb" => new MysticOrb(),
                    "ResistanceCharm" => new ResistanceCharm(),
                    "UltraCharm" => new UltraCharm(),
                    "HystericalPotion" => new HystericalPotion(),
                    "CombatCharm" => new CombatCharm(),
                    "FortificationCharm" => new FortificationCharm(),
                    "ChestClosed" => new Chest(),
                    "Key" => new Key(),

                    _ => throw new JsonSerializationException($"Unknown object type: {typeName}")
                };

                serializer.Populate(jsonObject.CreateReader(), instance);
                return instance;
            }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var seenObjects = new HashSet<object>(ReferenceEqualityComparer.Instance);
                WriteJsonRecursive(writer, value, serializer, seenObjects);
            }

            private void WriteJsonRecursive(JsonWriter writer, object value, JsonSerializer serializer, HashSet<object> seenObjects)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                Type type = value.GetType();

                // Prevent infinite recursion
                if (!seenObjects.Add(value))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("$ref");
                    writer.WriteValue(type.Name);
                    writer.WriteEndObject();
                    return;
                }

                writer.WriteStartObject();

                // Add type information
                writer.WritePropertyName("Type");
                writer.WriteValue(type.FullName);  // Full type name for clarity

                // Serialize properties
                foreach (var prop in type.GetProperties())
                {
                    if (!prop.CanRead) continue;  // Skip write-only properties

                    object propValue = prop.GetValue(value);
                    writer.WritePropertyName(prop.Name);

                    if (propValue != null && !propValue.GetType().IsPrimitive && propValue is not string)
                    {
                        // Recursively serialise complex objects
                        WriteJsonRecursive(writer, propValue, serializer, seenObjects);
                    }
                    else
                    {
                        serializer.Serialize(writer, propValue); // Use default serialisation for simple values
                    }
                }

                writer.WriteEndObject();
            }


        }
    }
}
