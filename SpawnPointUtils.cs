using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Random = System.Random;

namespace CustomSpawnpoints
{
    internal static class SpawnPointUtils
    {
        private static readonly Random Rand = new Random();

        public static void TeleportPlayer(UnturnedPlayer player, SpawnPoint spawn)
        {
            player.Player.teleportToLocationUnsafe(new Vector3
            {
                x = spawn.x,
                y = spawn.y,
                z = spawn.z
            }, spawn.Rotation != 0 ? spawn.Rotation : player.Rotation);
        }

        public static void TeleportPlayerRandom(UnturnedPlayer player, IList<SpawnPoint> spawns)
        {
            if (spawns.Count == 0) return;
            TeleportPlayer(player, GetRandomSpawn(spawns));
        }

        private static SpawnPoint GetRandomSpawn(IList<SpawnPoint> list)
        {
            return list[Rand.Next(list.Count)];
        }

        public static bool CheckPrioritySpawn(out SpawnPoint spawn)
        {
            spawn = SpawnPointPlugin.AllCustomSpawns.SavedSpawnPoints.FirstOrDefault(s =>
                s.name.ToLower() == GetPrioritySpawnName());
            return spawn != null;
        }

        public static List<SpawnPoint> GetSpawnsPlayerCanUse(IRocketPlayer player) =>
            SpawnPointPlugin.Config.Spawns.SavedSpawnPoints
                .Where(
                    spawn => player.HasPermission("spawnpoint.all") || player.HasPermission($"spawnpoint.{spawn.name}")
                ).ToList();

        public static string GetPrioritySpawnName() => SpawnPointPlugin.Config.PrioritySpawnName.ToLower();

        public static void SetGodmode(bool enableGod, UnturnedPlayer player)
        {
            if (!SpawnPointPlugin.Config.GiveGodModeOnRespawnUntilTeleport) return;
            player.Features.GodMode = enableGod;
        }

        public static void TeleportPlayerToSpawn(UnturnedPlayer player)
        {
            if (BarricadeManager.tryGetBed(player.CSteamID, out var bedPos, out _) &&
                Vector3.Distance(player.Position, bedPos) <= SpawnPointPlugin.Config.SpawnedNextToBedDistance)
            {
                return;
            }

            if (SpawnPointPlugin.Config.PrioritySpawnpointEnabled && CheckPrioritySpawn(out var prioritySpawn))
            {
                TeleportPlayer(player, prioritySpawn);
                return;
            }

            var accessableSpawns = GetSpawnsPlayerCanUse(player);
            if (accessableSpawns.Count == 0)
            {
                return;
            }

            if (SpawnPointPlugin.Config.RandomlySelectSpawnPoint)
            {
                TeleportPlayerRandom(player, accessableSpawns);
            }
            else
            {
                TeleportPlayer(player, accessableSpawns[0]);
            }
        }
    }
}