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
                    {"list", "Name: {0}, X: {1}, Y: {2}, Z:{3}"},
                    {"wrong_usage", "Incorrect usage! Correct usage: <add || remove || list> (spawn name)"},
                    {"no_spawns", "No custom spawn points found!"}
                };
            }
        }

        void UnturnedPlayerEvents_OnPlayerRevive(Rocket.Unturned.Player.UnturnedPlayer player, UnityEngine.Vector3 position, byte angle)
        {
            if (Configuration.Instance.Spawns.SavedSpawnPoints.Count == 0 || !Configuration.Instance.Enabled) return;
            new Thread(() =>
            {
                setGodmode(false, player);
                Thread.Sleep(Configuration.Instance.TeleportDelay);
                setGodmode(true, player);

                if (Configuration.Instance.RandomlySelectSpawnPoint)
                {
                    teleportPlayerRandom(player);
                }
                else
                {
                    teleportPlayer(player);
                }
            }
            ).Start();
        }

        void setGodmode(bool alreadyHasGodmode, UnturnedPlayer pl)
        {
            if (!Configuration.Instance.GiveGodModeOnRespawnUntilTeleport) return;
            if (alreadyHasGodmode)
            {
                pl.Features.GodMode = false;
            }
            else
            {
                pl.Features.GodMode = true;
            }
        }

        protected override void Unload()
        {
            Logger.Log("CustomSpawnpoints has Unloaded!");
            UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
        }

        void teleportPlayer(UnturnedPlayer P)
        {
            if (AllCustomSpawns.SavedSpawnPoints[0].Rotation != 0)
            {

                P.Teleport(new UnityEngine.Vector3
                {
                    x = AllCustomSpawns.SavedSpawnPoints[0].x,
                    y = AllCustomSpawns.SavedSpawnPoints[0].y,
                    z = AllCustomSpawns.SavedSpawnPoints[0].z
                }, AllCustomSpawns.SavedSpawnPoints[0].Rotation);
            }
            else
            {
                P.Teleport(new UnityEngine.Vector3
                {
                    x = AllCustomSpawns.SavedSpawnPoints[0].x,
                    y = AllCustomSpawns.SavedSpawnPoints[0].y,
                    z = AllCustomSpawns.SavedSpawnPoints[0].z
                }, P.Rotation);
            }
        }

        void teleportPlayerRandom(UnturnedPlayer P)
        {
            SpawnPoint Randompoint = getRandomSpawn();
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

        SpawnPoint getRandomSpawn()
        {
            Random r = new Random();
            return AllCustomSpawns.SavedSpawnPoints[r.Next(AllCustomSpawns.SavedSpawnPoints.Count)]; //The return range for random.next doesnt
            // include the max value, but it does include the minimum value, so if i enter r.Next(0,4) it can return a value between 0-3
        }
    }
}
