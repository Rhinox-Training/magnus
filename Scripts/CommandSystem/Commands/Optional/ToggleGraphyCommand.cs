
#if USING_GRAPHY
namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Toggles the Graphy counter", "FPS")]
    public class ToggleGraphyCommand : IConsoleCommand
    {
        public string CommandName => "toggle-graphy";
        public string Syntax => CommandName;

        public string[] Execute(string[] args)
        {
            if (GraphyService.Instance == null)
            {
                return new[] { "Graphy service not loaded." };
            }
            
            GraphyService.Instance.ToggleGUI();
            return new[] { "Graphy counters toggled." };
        }
    }
}
#endif