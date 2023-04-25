using Rhinox.Magnus.CommandSystem;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class QuitCommand : IConsoleCommand
    {
        public string CommandName => "quit";
        public string Syntax => CommandName;

        public string[] Execute(string[] args)
        {
            Application.Quit();
            return new string[] { "Quitting application." };
        }
    }
}