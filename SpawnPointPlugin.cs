using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CustomSpawnpoints
{
    public class SpawnPointPlugin : RocketPlugin<SpawnPointConfig>
    {
        public static SpawnPointConfig Config => Instance.Configuration.Instance;
        public static SpawnPointPlugin Instance;
        public static SpawnPoints AllCustomSpawns;

        protected override void Load()
        {
            Instance = this;
            AllCustomSpawns = Configuration.Instance.Spawns;

            if (Config.Enabled)
            {
                UnturnedPlayerEvents.OnPlayerRevive += UnturnedPlayerEvents_OnPlayerRevive;
            }

            Logger.Log("CustomSpawnpoints has Loaded!");
        }

        protected override void Unload()
        {
            Logger.Log("CustomSpawnpoints has Unloaded!");
            if (Config.Enabled)
            {
                UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
            }
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

        private void UnturnedPlayerEvents_OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            StartCoroutine(StartDelayedTeleport(player, Configuration.Instance.TeleportDelay));
        }

        private IEnumerator<WaitForSeconds> StartDelayedTeleport(UnturnedPlayer player, float delay)
        {
            SpawnPointUtils.SetGodmode(true, player);
            yield return new WaitForSeconds(delay);
            SpawnPointUtils.SetGodmode(false, player);

            if (Config.PrioritizeBeds && !Config.NoForcedBedSpawnPlayers.Contains(player.CSteamID) &&
                BarricadeManager.tryGetBed(player.CSteamID, out var bedPos, out _))
            {
                if (Vector3.Distance(bedPos, player.Position) <= Config.SpawnedNextToBedDistance)
                {
                    yield break;
                }

                player.Player.teleportToBed();
                UnturnedChat.Say(player, Translate("spawned_at_bed"), Color.yellow);
            }
            else
            {
                SpawnPointUtils.TeleportPlayerToSpawn(player);
            }
        }
    }
}