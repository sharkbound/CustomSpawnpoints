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

            if (command.Length == 1 && command[0].ToLower() == "list")
            {
                listSpawns(caller);
            } 
            else if (command.Length == 2 && command[0].ToLower() == "add")
            {
                addSpawn(command[1].ToLower(), (UnturnedPlayer)caller, caller);
            }
            else if (command.Length == 2 && command[0].ToLower() == "remove")
            {
                removeSpawn((UnturnedPlayer)caller,  command[1].ToLower(), caller);
            }
            else
            {
                if (command.Length == 1)
                {
                    if (checkIfSpawnExist(command[0].ToLower()))
                    {
                        tryTpToSpawn((UnturnedPlayer)caller, command[0].ToLower());
                        return;
                    }
                }

                UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("wrong_usage"));
                return;
            }
        }
        #region methods
        void updateSpawnsList()
        {
            SpawnpointPlugin.Instance.Configuration.Save();
            SpawnpointPlugin.AllCustomSpawns = SpawnpointPlugin.Instance.Configuration.Instance.Spawns;
        }

        void tryTpToSpawn(UnturnedPlayer player, string spawnName)
        {
            var spawn = getSpawnByName(spawnName);
            if (spawn != null)
            {
                teleportToSpawnpoint(player, spawn.name);
            }
            
        }

        bool checkIfSpawnExist(string spawnName)
        {
            spawnName = spawnName.ToLower();
            foreach (var spawn in SpawnpointPlugin.AllCustomSpawns.SavedSpawnPoints)
            {
                if (spawn.name == spawnName) return true;
            }

            return false;
        }

        SpawnPoint getSpawnByName(string spawnName)
        {
            spawnName = spawnName.ToLower();

            foreach (var spawn in SpawnpointPlugin.AllCustomSpawns.SavedSpawnPoints)
            {
                if (spawn.name == spawnName) return spawn;
            }

            return null;
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

        void addSpawn(string enteredName, UnturnedPlayer player, IRocketPlayer caller)
        {
            if (checkIfSpawnExist(enteredName))
            {
                string nameOfSpawn = getSpawnByName(enteredName).name;
                UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("spawn_already_exist", nameOfSpawn));
                return;
            }

            UnturnedPlayer uP = (UnturnedPlayer)caller;
            SpawnpointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Add(new SpawnPoint
            {
                name = enteredName,
                x = uP.Position.x,
                y = uP.Position.y,
                z = uP.Position.z,
                Rotation = uP.Rotation
            });

            UnturnedChat.Say(caller, SpawnpointPlugin.Instance.Translate("spawn_added", enteredName));

            updateSpawnsList();
        }

        void removeSpawn(UnturnedPlayer player, string spawnName, IRocketPlayer caller)
        {
            SpawnPoint toRemove = getSpawnByName(spawnName);

            if (toRemove != null)
            {
                SpawnpointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Remove(toRemove);
                updateSpawnsList();
                sendMSG(SpawnpointPlugin.Instance.Translate("removed_spawn", toRemove.name), caller);
            }
            else
            {
                sendMSG(SpawnpointPlugin.Instance.Translate("spawn_not_found", spawnName), caller);
            }
        }

        void listSpawns(IRocketPlayer caller)
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

        void teleportToSpawnpoint(UnturnedPlayer uCaller, string name)
        {
            SpawnPoint spawnpoint = getSpawnByName(name);
            var spawnVector = new UnityEngine.Vector3(spawnpoint.x, spawnpoint.y, spawnpoint.z);

            uCaller.Teleport(spawnVector, spawnpoint.Rotation);
            UnturnedChat.Say(uCaller, SpawnpointPlugin.Instance.Translate("teleport_spawn", spawnpoint.name));
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
