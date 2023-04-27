using Rhinox.Lightspeed;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Loads a scene using the LevelLoader", "Level")]
    public class LoadLevelCommand:IConsoleCommand
    {
        public string CommandName => "load-level";
        public string Syntax => "load-level <scene-index>";
        public string[] Execute(string[] args)
        {
            if(args.IsNullOrEmpty())
                return new[] { $"Syntax is: {Syntax}" };
            
            if(!int.TryParse(args[0], out int sceneIndex))
                return new[] { $"Scene index must be an integer: {args[0]}" };

            LevelLoader loader = LevelLoader.Instance;
            if(loader == null)
                return new[] { "No level loader found" };

            loader.LoadScene(sceneIndex);
            return new[] { $"Attempting to load scene {sceneIndex}" };
        }
    }
}