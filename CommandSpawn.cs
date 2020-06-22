using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace CustomSpawnpoints
{
    internal class CommandSpawn : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, SpawnPointPlugin.Instance.Translate("wrong_usage"));
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
                removeSpawn((UnturnedPlayer)caller, command[1].ToLower(), caller);
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

                UnturnedChat.Say(caller, SpawnPointPlugin.Instance.Translate("wrong_usage"));
                return;
            }
        }
        #region methods

        private void updateSpawnsList()
        {
            SpawnPointPlugin.Instance.Configuration.Save();
            SpawnPointPlugin.AllCustomSpawns = SpawnPointPlugin.Instance.Configuration.Instance.Spawns;
        }

        private void tryTpToSpawn(UnturnedPlayer player, string spawnName)
        {
            var spawn = getSpawnByName(spawnName);
            if (spawn != null)
            {
                teleportToSpawnpoint(player, spawn.name);
            }

        }

        private bool checkIfSpawnExist(string spawnName)
        {
            spawnName = spawnName.ToLower();
            foreach (var spawn in SpawnPointPlugin.AllCustomSpawns.SavedSpawnPoints)
            {
                if (spawn.name == spawnName) return true;
            }

            return false;
        }

        private SpawnPoint getSpawnByName(string spawnName)
        {
            spawnName = spawnName.ToLower();

            foreach (var spawn in SpawnPointPlugin.AllCustomSpawns.SavedSpawnPoints)
            {
                if (spawn.name == spawnName) return spawn;
            }

            return null;
        }

        private void sendMSG(string msg, IRocketPlayer caller)
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

        private void addSpawn(string enteredName, UnturnedPlayer player, IRocketPlayer caller)
        {
            if (checkIfSpawnExist(enteredName))
            {
                string nameOfSpawn = getSpawnByName(enteredName).name;
                UnturnedChat.Say(caller, SpawnPointPlugin.Instance.Translate("spawn_already_exist", nameOfSpawn));
                return;
            }

            UnturnedPlayer uP = (UnturnedPlayer)caller;
            SpawnPointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Add(new SpawnPoint
            {
                name = enteredName,
                x = uP.Position.x,
                y = uP.Position.y,
                z = uP.Position.z,
                Rotation = uP.Rotation
            });

            UnturnedChat.Say(caller, SpawnPointPlugin.Instance.Translate("spawn_added", enteredName));

            updateSpawnsList();
        }

        private void removeSpawn(UnturnedPlayer player, string spawnName, IRocketPlayer caller)
        {
            SpawnPoint toRemove = getSpawnByName(spawnName);

            if (toRemove != null)
            {
                SpawnPointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Remove(toRemove);
                updateSpawnsList();
                sendMSG(SpawnPointPlugin.Instance.Translate("removed_spawn", toRemove.name), caller);
            }
            else
            {
                sendMSG(SpawnPointPlugin.Instance.Translate("spawn_not_found", spawnName), caller);
            }
        }

        private void listSpawns(IRocketPlayer caller)
        {
            if (SpawnPointPlugin.Instance.Configuration.Instance.Spawns.SavedSpawnPoints.Count == 0)
            {
                UnturnedChat.Say(caller, SpawnPointPlugin.Instance.Translate("no_spawns"));
                return;
            }

            foreach (var point in SpawnPointPlugin.AllCustomSpawns.SavedSpawnPoints)
            {
                UnturnedChat.Say(caller, SpawnPointPlugin.Instance.Translate("list", point.name, point.x, point.y, point.z));
            }
        }

        private void teleportToSpawnpoint(UnturnedPlayer uCaller, string name)
        {
            SpawnPoint spawnpoint = getSpawnByName(name);
            var spawnVector = new UnityEngine.Vector3(spawnpoint.x, spawnpoint.y, spawnpoint.z);

            uCaller.Teleport(spawnVector, spawnpoint.Rotation);
            UnturnedChat.Say(uCaller, SpawnPointPlugin.Instance.Translate("teleport_spawn", spawnpoint.name));
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
