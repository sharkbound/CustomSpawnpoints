using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                    {"no_spawns", "No custom spawn points found!"},
                    {"forcebed_ignore_bed", "You will no longer be forced to spawn at your bed"},
                    {"forcebed_use_bed", "You will spawn at your bed from now on, if priorizebed is enabled on the server"},
                };
            }
        }

        void UnturnedPlayerEvents_OnPlayerRevive(UnturnedPlayer player, UnityEngine.Vector3 position, byte angle)
        {
            if (Configuration.Instance.Spawns.SavedSpawnPoints.Count == 0 || !Configuration.Instance.Enabled) return;
            new Thread(() =>
            {
                SpawnpointConfig config = Configuration.Instance;
                if (config.PrioritizeBeds && !config.NoForcedBedSpawnPlayers.Contains(player.CSteamID) && HasBed(player.CSteamID, out UnityEngine.Vector3 bedVector3, out byte bedAngle))
                {
                    TeleportToBed(player, bedVector3, bedAngle);
                }
                else
                {
                    TeleportPlayerToSpawn(player);
                }
            }
            ).Start();
        }

        bool HasBed(CSteamID PlayerID, out UnityEngine.Vector3 Point, out byte angle)
        {
            return BarricadeManager.tryGetBed(PlayerID, out Point, out angle);
        }

        void TeleportToBed(UnturnedPlayer player, UnityEngine.Vector3 bedPoint, byte bedAngle)
        {
            SetGodmode(true, player);
            Thread.Sleep(Configuration.Instance.TeleportDelay);
            SetGodmode(false, player);

            player.Teleport(bedPoint, bedAngle);
            UnturnedChat.Say(player, "you where spawned at your bed, do /forcebed to disable/enable this", UnityEngine.Color.yellow);
        }

        void TeleportPlayerToSpawn(UnturnedPlayer player)
        {
            var config = Configuration.Instance;
            if (HasBed(player.CSteamID, out var bedPos, out var bedAngle) && UnityEngine.Vector3.Distance(player.Position, bedPos) <= config.SpawnedNextToBedDistance)
            {
                UnturnedChat.Say($"skipped tp'ing player {player.CharacterName}");
                return;
            }

            if (PrioritySpawnEnabled() && PrioritySpawnIsValidSpawn(out SpawnPoint prioritySpawn))
            {
                SleepAndToggleGodmode(player);

                TeleportPlayer(player, prioritySpawn);
                return;
            }

            List<SpawnPoint> accessableSpawns = GetSpawnsPlayerCanUse(player);
            if (accessableSpawns.Count == 0)
            {
                return;
            }

            SleepAndToggleGodmode(player);

            if (Configuration.Instance.RandomlySelectSpawnPoint)
            {
                TeleportPlayerRandom(player, accessableSpawns);
            }
            else
            {
                TeleportPlayer(player, accessableSpawns[0]);
            }
        }

        void SleepAndToggleGodmode(UnturnedPlayer player)
        {
            SetGodmode(true, player);
            Thread.Sleep(Configuration.Instance.TeleportDelay);
            SetGodmode(false, player);
        }

        bool PrioritySpawnEnabled()
        {
            return Configuration.Instance.PrioritySpawnpointEnabled;
        }

        bool PrioritySpawnIsValidSpawn(out SpawnPoint spawn)
        {
            spawn = AllCustomSpawns.SavedSpawnPoints.FirstOrDefault(s => s.name.ToLower() == GetPrioritySpawnName());

            if (spawn == null) return false;
            else return true;
        }

        void SetGodmode(bool enableGod, UnturnedPlayer pl)
        {
            if (!Configuration.Instance.GiveGodModeOnRespawnUntilTeleport) return;
            pl.Features.GodMode = enableGod;
        }

        string GetPrioritySpawnName()
        {
            return Configuration.Instance.PrioritySpawnName.ToLower();
        }

        List<SpawnPoint> GetSpawnsPlayerCanUse(UnturnedPlayer player)
        {
            List<SpawnPoint> spawnsPlayerCanUse = new List<SpawnPoint>();

            foreach (SpawnPoint spawn in Configuration.Instance.Spawns.SavedSpawnPoints)
            {
                if (player.HasPermission("spawnpoint.all") || player.HasPermission("spawnpoint." + spawn.name))
                {
                    spawnsPlayerCanUse.Add(spawn);
                }
            }

            return spawnsPlayerCanUse;
        }

        void TeleportPlayer(UnturnedPlayer player, SpawnPoint spawn)
        {
            player.Teleport(new UnityEngine.Vector3
            {
                x = spawn.x,
                y = spawn.y,
                z = spawn.z
            }, spawn.Rotation != 0 ? spawn.Rotation : player.Rotation);
        }

        void TeleportPlayerRandom(UnturnedPlayer P, List<SpawnPoint> spawns)
        {
            if (spawns.Count == 0) return;

            SpawnPoint Randompoint = GetRandomSpawn(spawns);
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

        SpawnPoint GetRandomSpawn(List<SpawnPoint> list)
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
