using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using Random = System.Random;

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

        public override TranslationList DefaultTranslations { get; } =
            new TranslationList
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
                {"spawned_at_bed", "you were spawned at your bed, do /forcebed to disable/enable this"},
            };

        private IEnumerator<WaitForSeconds> StartDelayedTeleport(UnturnedPlayer player, float delay)
        {
            yield return new WaitForSeconds(delay);

            var config = Configuration.Instance;
            if (config.PrioritizeBeds && !config.NoForcedBedSpawnPlayers.Contains(player.CSteamID) &&
                HasBed(player.CSteamID, out _, out _))
            {
                SetGodmode(true, player);
                Thread.Sleep(Configuration.Instance.TeleportDelay);
                SetGodmode(false, player);

                player.Player.teleportToBed();
                UnturnedChat.Say(player, Translate("spawned_at_bed"), Color.yellow);
            }
            else
            {
                TeleportPlayerToSpawn(player);
            }
        }

        private void UnturnedPlayerEvents_OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            StartCoroutine(StartDelayedTeleport(player, Configuration.Instance.TeleportDelay));
            // if (Configuration.Instance.Spawns.SavedSpawnPoints.Count == 0 || !Configuration.Instance.Enabled) return;
            // new Thread(() =>
            //     {
            //         SpawnpointConfig config = Configuration.Instance;
            //         if (config.PrioritizeBeds && !config.NoForcedBedSpawnPlayers.Contains(player.CSteamID) &&
            //             HasBed(player.CSteamID, out UnityEngine.Vector3 bedVector3, out var bedAngle))
            //         {
            //             TeleportToBed(player, bedVector3, bedAngle);
            //         }
            //         else
            //         {
            //             TeleportPlayerToSpawn(player);
            //         }
            //     }
            // ).Start();
        }

        private static bool HasBed(CSteamID playerId, out Vector3 point, out byte angle) =>
            BarricadeManager.tryGetBed(playerId, out point, out angle);

        private void TeleportPlayerToSpawn(UnturnedPlayer player)
        {
            var config = Configuration.Instance;
            if (HasBed(player.CSteamID, out var bedPos, out var bedAngle) &&
                Vector3.Distance(player.Position, bedPos) <= config.SpawnedNextToBedDistance)
            {
                return;
            }

            if (PrioritySpawnEnabled() && PrioritySpawnIsValidSpawn(out var prioritySpawn))
            {
                SleepAndToggleGodmode(player);

                TeleportPlayer(player, prioritySpawn);
                return;
            }

            var accessableSpawns = GetSpawnsPlayerCanUse(player);
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

        private void SleepAndToggleGodmode(UnturnedPlayer player)
        {
            SetGodmode(true, player);
            Thread.Sleep(Configuration.Instance.TeleportDelay);
            SetGodmode(false, player);
        }

        private bool PrioritySpawnEnabled()
        {
            return Configuration.Instance.PrioritySpawnpointEnabled;
        }

        private bool PrioritySpawnIsValidSpawn(out SpawnPoint spawn)
        {
            spawn = AllCustomSpawns.SavedSpawnPoints.FirstOrDefault(s => s.name.ToLower() == GetPrioritySpawnName());
            return spawn != null;
        }

        private void SetGodmode(bool enableGod, UnturnedPlayer pl)
        {
            if (!Configuration.Instance.GiveGodModeOnRespawnUntilTeleport) return;
            pl.Features.GodMode = enableGod;
        }

        private string GetPrioritySpawnName() => Configuration.Instance.PrioritySpawnName.ToLower();

        private List<SpawnPoint> GetSpawnsPlayerCanUse(IRocketPlayer player) =>
            Configuration.Instance.Spawns.SavedSpawnPoints.Where(spawn =>
                player.HasPermission("spawnpoint.all") || player.HasPermission($"spawnpoint.{spawn.name}")).ToList();

        private static void TeleportPlayer(UnturnedPlayer player, SpawnPoint spawn)
        {
            player.Player.teleportToLocationUnsafe(new Vector3
            {
                x = spawn.x,
                y = spawn.y,
                z = spawn.z
            }, spawn.Rotation != 0 ? spawn.Rotation : player.Rotation);
        }

        private static void TeleportPlayerRandom(UnturnedPlayer p, IList<SpawnPoint> spawns)
        {
            if (spawns.Count == 0) return;

            var randompoint = GetRandomSpawn(spawns);
            p.Teleport(new Vector3
            {
                x = randompoint.x,
                y = randompoint.y,
                z = randompoint.z
            }, randompoint.Rotation != 0 ? randompoint.Rotation : p.Rotation);
        }

        private static SpawnPoint GetRandomSpawn(IList<SpawnPoint> list)
        {
            var r = new Random();
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