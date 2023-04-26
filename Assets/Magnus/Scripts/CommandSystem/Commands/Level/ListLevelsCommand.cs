using System;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Gets all levels in the current build", "Level")]
    public class ListLevelsCommand : IConsoleCommand
    {
        public string CommandName => "list-levels";
        public string Syntax => "list-levels";

        public string[] Execute(string[] args)
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            string returnString = "";
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                returnString += $"{i}: {sceneName}\n";
            }

            return new[] { returnString };
        }
    }
}