using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;
using System.Xml.Serialization;
using Steamworks;

namespace CustomSpawnpoints
{
    public class SpawnPointConfig : IRocketPluginConfiguration
    {
        public bool Enabled;
        public bool GiveGodModeOnRespawnUntilTeleport;
        public bool RandomlySelectSpawnPoint;
        public bool PrioritizeBeds;
        public bool PrioritySpawnpointEnabled;
        public string PrioritySpawnName;
        public float TeleportDelay;
        public int SpawnedNextToBedDistance;
        public SpawnPoints Spawns;
        public List<CSteamID> NoForcedBedSpawnPlayers;

        public void LoadDefaults()
        {
            Enabled = true;
            GiveGodModeOnRespawnUntilTeleport = true;
            RandomlySelectSpawnPoint = true;
            PrioritizeBeds = true;
            PrioritySpawnpointEnabled = false;
            PrioritySpawnName = "default";
            TeleportDelay = 1000;
            Spawns = new SpawnPoints();
            SpawnedNextToBedDistance = 10;
            NoForcedBedSpawnPlayers = new List<CSteamID>();
        }
    }

    public class SpawnPoints
    {
        [XmlArrayItem(ElementName = "spawn")] public List<SpawnPoint> SavedSpawnPoints = new List<SpawnPoint>();
    }

    public class SpawnPoint
    {
        [XmlAttribute("name")] public string name;
        [XmlAttribute("x")] public float x;
        [XmlAttribute("y")] public float y;
        [XmlAttribute("z")] public float z;
        [XmlAttribute("rotation")] public float Rotation;
    }
}