using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;
using System.Xml.Serialization;

namespace CustomSpawnpoints
{
    public class SpawnpointConfig : IRocketPluginConfiguration
    {
        public bool Enabled;
        public bool GiveGodModeOnRespawnUntilTeleport;
        public bool RandomlySelectSpawnPoint;
        public int TeleportDelay;
        public SpawnPoints Spawns;

        public void LoadDefaults()
        {
            Enabled = true;
            GiveGodModeOnRespawnUntilTeleport = true;
            RandomlySelectSpawnPoint = true;
            TeleportDelay = 1000;
            Spawns = new SpawnPoints();
        }
    }

    public class SpawnPoints
    {
        [XmlArrayItem(ElementName = "spawn")]
        public List<SpawnPoint> SavedSpawnPoints = new List<SpawnPoint>();
    }

    public class SpawnPoint
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("x")]
        public float x;
        [XmlAttribute("y")]
        public float y;
        [XmlAttribute("z")]
        public float z;
        [XmlAttribute("rotation")]
        public float Rotation;
    }
}
