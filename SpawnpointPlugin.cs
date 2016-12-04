using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Core.Logging;
using SDG.Unturned;
using Rocket.Unturned.Events;
using System.Threading;
using Rocket.Unturned.Player;
using Rocket.Core.Commands;
using Steamworks;

namespace CustomSpawnpoints
{
    public class SpawnpointPlugin : RocketPlugin<SpawnpointConfig>
    {
        public static SpawnpointPlugin Instance;
        public static SpawnPoints AllCustomSpawns;

        protected override void Load()
        {
            Instance = this;
            AllCustomSpawns = Configuration.Instance.Spawns;
            UnturnedPlayerEvents.OnPlayerRevive += UnturnedPlayerEvents_OnPlayerRevive;
            Logger.Log("CustomSpawnpoints has Loaded!");
        }

        protected override void Unload()
        {
            Logger.Log("CustomSpawnpoints has Unloaded!");
            UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
        }

        public override Rocket.API.Collections.TranslationList DefaultTranslations
        {
            get
            {
                return new Rocket.API.Collections.TranslationList
                {
                    {"spawn_added", "Added spawn '{0}'."},
                    {"spawn_already_exist", "A spawn by the name '{0}' already exist!"},
                    {"removed_spawn", "Removed spawn '{0}'."},
                    {"spawn_not_found", "There is not any spawns by the name '{0}'"},
                    {"list", "Name: {0}, X: {1}, Y: {2}, Z: {3}"},
                    {"wrong_usage", "Incorrect usage! Correct usage: <add || remove || list> [spawn name]"},
                    {"teleport_spawn", "Teleported to spawn {0}!"},
                    {"no_spawns", "No custom spawn points found!"}
                };
            }
        }

        void UnturnedPlayerEvents_OnPlayerRevive(Rocket.Unturned.Player.UnturnedPlayer player, UnityEngine.Vector3 position, byte angle)
        {
            if (Configuration.Instance.Spawns.SavedSpawnPoints.Count == 0 || !Configuration.Instance.Enabled) return;
            new Thread(() =>
            {
                UnityEngine.Vector3 bedVector3;
                byte bedAngle;

                if (Configuration.Instance.PrioritizeBeds && hasBed(player.CSteamID, out bedVector3, out bedAngle))
                {
                    teleportToBed(player, bedVector3, bedAngle);
                }
                else
                {
                    teleportPlayerToSpawn(player);
                }
            }
            ).Start();
        }

        bool hasBed(CSteamID PlayerID, out UnityEngine.Vector3 Point, out byte angle)
        {
            return BarricadeManager.tryGetBed(PlayerID, out Point, out angle);
        }

        void teleportToBed(UnturnedPlayer player, UnityEngine.Vector3 bedPoint, byte bedAngle)
        {
            setGodmode(true, player);
            Thread.Sleep(Configuration.Instance.TeleportDelay);
            setGodmode(false, player);

            player.Teleport(bedPoint, bedAngle);
        }

        void teleportPlayerToSpawn(UnturnedPlayer player)
        {
            SpawnPoint prioritySpawn;
            if (PrioritySpawnEnabled() && PrioritySpawnIsValidSpawn(out prioritySpawn))
            {
                sleepAndToggleGodmode(player);

                teleportPlayer(player, prioritySpawn);
                return;
            }

            var accessableSpawns = getSpawnsPlayerCanUse(player);
            if (accessableSpawns.Count == 0)
            {
                return;
            }

            sleepAndToggleGodmode(player);

            if (Configuration.Instance.RandomlySelectSpawnPoint)
            {
                teleportPlayerRandom(player, accessableSpawns);
            }
            else
            {
                teleportPlayer(player, accessableSpawns[0]);
            }
        }

        void sleepAndToggleGodmode(UnturnedPlayer player)
        {
            setGodmode(true, player);
            Thread.Sleep(Configuration.Instance.TeleportDelay);
            setGodmode(false, player);
        }

        bool PrioritySpawnEnabled()
        {
            return Configuration.Instance.PrioritySpawnpointEnabled;
        }

        bool PrioritySpawnIsValidSpawn(out SpawnPoint spawn)
        {
            spawn = AllCustomSpawns.SavedSpawnPoints.FirstOrDefault(s => s.name.ToLower() == getPrioritySpawnName());

            if (spawn == null) return false;
            else return true;
        }

        void setGodmode(bool enableGod, UnturnedPlayer pl)
        {
            if (!Configuration.Instance.GiveGodModeOnRespawnUntilTeleport) return;
            if (enableGod)
            {
                pl.Features.GodMode = true;
            }
            else
            {
                pl.Features.GodMode = false;
            }
        }

        string getPrioritySpawnName()
        {
            return Configuration.Instance.PrioritySpawnName.ToLower();
        }

        List<SpawnPoint> getSpawnsPlayerCanUse(UnturnedPlayer p)
        {
            List<SpawnPoint> spawnsPlayerCanUse = new List<SpawnPoint>();
            
            foreach (var spawn in Configuration.Instance.Spawns.SavedSpawnPoints)
            {
                if (p.HasPermission("spawnpoint.all") || p.HasPermission("spawnpoint." + spawn.name))
                {
                    spawnsPlayerCanUse.Add(spawn);
                }
            }

            return spawnsPlayerCanUse;
        }

        void teleportPlayer(UnturnedPlayer P, SpawnPoint spawn)
        {
            if (spawn.Rotation != 0)
            {
                P.Teleport(new UnityEngine.Vector3
                {
                    x = spawn.x,
                    y = spawn.y,
                    z = spawn.z
                }, spawn.Rotation);
            }
            else
            {
                P.Teleport(new UnityEngine.Vector3
                {
                    x = spawn.x,
                    y = spawn.y,
                    z = spawn.z
                }, P.Rotation);
            }
        }

        void teleportPlayerRandom(UnturnedPlayer P, List<SpawnPoint> spawns)
        {
            if (spawns.Count == 0) return;

            SpawnPoint Randompoint = getRandomSpawn(spawns);
            if (Randompoint.Rotation != 0)
            {
                P.Teleport(new UnityEngine.Vector3
                {
                    x = Randompoint.x,
                    y = Randompoint.y,
                    z = Randompoint.z
                }, Randompoint.Rotation);
            }
            else
            {
                P.Teleport(new UnityEngine.Vector3
                {
                    x = Randompoint.x,
                    y = Randompoint.y,
                    z = Randompoint.z
                }, P.Rotation);
            }
        }

        SpawnPoint getRandomSpawn(List<SpawnPoint> list)
        {
            Random r = new Random();
            return list[r.Next(list.Count)]; //The return range for random.next doesnt
            // include the max value, but it does include the minimum value, so if i enter r.Next(0,4) it can return a value between 0-3
        }

        /*
        [RocketCommand("sim", "", "", Rocket.API.AllowedCaller.Player)]
        [RocketCommandPermission("sim")]
        public void SimulateSpawn(IRocketPlayer p, string[] para)
        {
            var spawns = getSpawnsPlayerCanUse((UnturnedPlayer)p);
            foreach (var spawn in spawns)
            {
                Rocket.Unturned.Chat.UnturnedChat.Say(p, string.Format("spawn found: {0}", spawn.name));
            }

            Rocket.Unturned.Chat.UnturnedChat.Say(p, string.Format("randon spawn selected: {0}", getRandomSpawn(spawns).name));
        }*/
    }
}
