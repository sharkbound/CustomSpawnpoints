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
                    {"wrong_usage", "Incorrect usage! Correct usage: <add || remove || list> [spawn name]"},
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
                    teleportPlayerRandom(player, getSpawnsPlayerCanUse(player));
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

        List<SpawnPoint> getSpawnsPlayerCanUse(UnturnedPlayer p)
        {
            List<SpawnPoint> spawnsPLayerCanUse = new List<SpawnPoint>();
            
            foreach (var spawn in Configuration.Instance.Spawns.SavedSpawnPoints)
            {
                if (p.HasPermission("spawn.all") || p.HasPermission("spawn." + spawn.name))
                {
                    spawnsPLayerCanUse.Add(spawn);
                }
            }

            return spawnsPLayerCanUse;
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
