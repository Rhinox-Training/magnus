using Rhinox.Lightspeed;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Gets the path of a scene with the given name", "Level")]
    public class GetLevelPath : IConsoleCommand
    {
        public string CommandName => "get-level-path";
        public string Syntax => "get-level-path <scene-name>";
        public string[] Execute(string[] args)
        {
            if(args.IsNullOrEmpty())
                return new[] { $"Syntax is: {Syntax}" };
            
            string returnString = LevelLoader.GetScenePathByName(args[0]);
            return returnString == "" ? new[] { $"Scene '{args[0]}' was not found..." } : new[] { returnString };
        }
    }
}