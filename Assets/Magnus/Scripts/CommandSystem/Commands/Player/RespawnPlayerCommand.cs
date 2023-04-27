using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem.Player
{
    [CommandInfo("Respawns the player", "Player")]
    public class RespawnPlayerCommand : IConsoleCommand
    {
        public string CommandName => "respawn-player";
        public string Syntax => "respawn-player [<position x,y,z> <rotation x,y,z> <persistent>]";

        public string[] Execute(string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                PlayerManager.Instance.RespawnPlayer();
                return new[] { "Player respawned." };
            }

            if (!UnityTypeParser.TryParseVector3(args[0], out Vector3 position))
                return new[] { "Unable to parse position, format is x,y,z" };

            if (args.Length == 1)
            {
                PlayerManager.Instance.RespawnPlayer(position);
                return new[] { $"Player respawned at position {position}" };
            }

            if (!UnityTypeParser.TryParseVector3(args[1], out Vector3 rotation))
                return new[] { "Unable to parse rotation, format is x,y,z" };
            Quaternion rot = Quaternion.Euler(rotation);

            if (args.Length == 2)
            {
                PlayerManager.Instance.RespawnPlayer(position, rot);
                return new[] { $"Player respawned at position {position}, with rotation {rotation}" };
            }

            if (!bool.TryParse(args[2], out bool persistent))
                return new[] { "Unable to parse persistent, format is true/false" };
            
            PlayerManager.Instance.RespawnPlayer(position, rot, persistent);
            return new[] { $"Player respawned at position {position}, with rotation {rotation}, and persistent {persistent}" };
        }
    }
}