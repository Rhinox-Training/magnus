using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Toggle Full Screen", "Screen")]
    public class ToggleFullScreenCommand : IConsoleCommand
    {
        public string CommandName => "toggle-full-screen";

        public string[] Execute(string[] args)
        {
            Screen.fullScreen = !Screen.fullScreen;
            if (Screen.fullScreen)
                return new[] { "Changed to full screen." };
            else
                return new[] { "Changed to windowed." };
        }
    }
}