using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace CustomSpawnpoints
{
    internal class CommandForceBed : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "forcebed";

        public string Help => "toggles if PriorizeBed affects this player";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        private static SpawnPointConfig Config => SpawnPointPlugin.Instance.Configuration.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer) caller;
            if (player == null)
            {
                return;
            }

            if (Config.NoForcedBedSpawnPlayers.Contains(player.CSteamID))
            {
                Config.NoForcedBedSpawnPlayers.Remove(player.CSteamID);
                UnturnedChat.Say(player, SpawnPointPlugin.Instance.Translate("forcebed_use_bed"),
                    UnityEngine.Color.green);
            }
            else
            {
                Config.NoForcedBedSpawnPlayers.Add(player.CSteamID);
                UnturnedChat.Say(player, SpawnPointPlugin.Instance.Translate("forcebed_ignore_bed"),
                    UnityEngine.Color.green);
            }

            SpawnPointPlugin.Instance.Configuration.Save();
        }
    }
}