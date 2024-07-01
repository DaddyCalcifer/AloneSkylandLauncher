using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

[System.Serializable]
public class WorldData
{
    public int seed;
    public int islandType;
    public float[] player_pos;
    public float hp, maxHP, hunger, maxHunger, thist, maxThist;
    public string version;
    public int Lvl, learnPTS;
    public float exp;
    public List<int> learnedCrafts;

    public WorldData()
    {
    }

    public static WorldData Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WorldData));
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    WorldData data = (WorldData)serializer.Deserialize(stream);
                    return data;
                }
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}

[System.Serializable]
public class SerializableKeyValuePair<K, V>
{
    public K Key;
    public V Value;

    // Parameterless constructor needed for XML serialization
    public SerializableKeyValuePair() { }

    public SerializableKeyValuePair(K key, V value)
    {
        Key = key;
        Value = value;
    }
}
