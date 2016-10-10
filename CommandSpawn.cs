using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace CustomSpawnpoints
{
    class CommandSpawn : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Player; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("wrong_usage"));
                return;
            }
            #region listParameter
            if (command.Length == 1 && command[0].ToLower() == "list")
            {
                if (SpawnpointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Count == 0)
                {
                    UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("no_spawns"));
                    return;
                }

                foreach (var point in SpawnpointPlugin.AllCustomSpawns.SavedSpawnPoints)
                {
                    UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("list", point.name, point.x, point.y, point.z));
                }
            } 
            #endregion
            #region addParameter
            else if (command.Length == 2 && command[0].ToLower() == "add")
            {
                foreach (var p in SpawnpointPlugin.AllCustomSpawns.SavedSpawnPoints)
                {
                    if (command[1].ToLower() == p.name.ToLower())
                    {
                        UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("spawn_already_exist", p.name));
                        return;
                    }
                }

                UnturnedPlayer uP = (UnturnedPlayer)caller;
                SpawnpointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Add(new SpawnPoint
                {
                    name = command[1],
                    x = uP.Position.x,
                    y = uP.Position.y,
                    z = uP.Position.z,
                    Rotation = uP.Rotation
                });

                UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("spawn_added", command[1]));

                updateSpawnsList();
            }
            #endregion
            #region removeParameter
            else if (command.Length == 2 && command[0].ToLower() == "remove")
            {
                SpawnPoint toRemove = null;
                foreach (var p in SpawnpointPlugin.AllCustomSpawns.SavedSpawnPoints)
                {
                    if (command[1].ToLower() == p.name.ToLower())
                    {
                        toRemove = p;
                        break;
                    }
                }

                if (toRemove != null)
                {
                    SpawnpointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Remove(toRemove);
                    updateSpawnsList();
                    sendMSG(SpawnpointPlugin.Instance.Translate("removed_spawn", toRemove.name), caller);
                }
                else
                {
                    sendMSG(SpawnpointPlugin.Instance.Translate("spawn_not_found", command[1]), caller);
                }
            }
            #endregion
            #region wrongUsage
            else
            {
                UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("wrong_usage"));
                return;
            }
            #endregion
        }
        #region methods
        void updateSpawnsList()
        {
            SpawnpointPlugin.Instance.Configuration.Save();
            SpawnpointPlugin.AllCustomSpawns = SpawnpointPlugin.Instance.Configuration.Instance.Spawns;
        }

        void sendMSG(string msg, IRocketPlayer caller)
        {
            if (caller is ConsolePlayer)
            {
                Logger.LogWarning(msg);
            }
            else
            {
                UnturnedChat.Say(caller, msg);
            }
        }
        #endregion

        public string Help
        {
            get { return "list, remove, or add custom spawns"; }
        }

        public string Name
        {
            get { return "spawn"; }
        }

        public List<string> Permissions
        {
            get { return new List<string> { "spawn" }; }
        }

        public string Syntax
        {
            get { return "<add || remove || list> [spawn point name]"; }
        }
    }
}
