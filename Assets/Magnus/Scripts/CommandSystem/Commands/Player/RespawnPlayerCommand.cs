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
            if (PlayerManager.Instance == null)
                return new[] { "No player manager found" };

            Vector3 position = Vector3.zero;
            Vector3 eulerAngles = Vector3.zero;
            bool persistent = false;
            Quaternion rotation;

            // If there are arguments, attempt to parse them according to the amount of arguments
            // (See Syntax)
            if (args.Length > 0 && !UnityTypeParser.TryParseVector3(args[0], out position))
                return new[] { "Unable to parse position, format is x,y,z" };

            if (args.Length > 1 && !UnityTypeParser.TryParseVector3(args[1], out eulerAngles))
                return new[] { "Unable to parse rotation, format is x,y,z" };
            
            rotation = Quaternion.Euler(eulerAngles);

            if (args.Length > 2 && !bool.TryParse(args[2], out persistent))
                return new[] { "Unable to parse persistent, format is true/false" };

            PlayerManager.Instance.RespawnLocalPlayer(position, rotation, persistent);
            return new[]
            {
                $"Player respawned at position {position}, with rotation {eulerAngles}, and persistent {persistent}"
            };
        }
    }
}