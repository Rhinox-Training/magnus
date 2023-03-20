using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Magnus.CommandSystem
{
    public class HelpConsoleCommand : IConsoleCommand
    {
        public string CommandName => "help";
        public string[] Execute(string[] args)
        {
            var dict = new Dictionary<string, List<string>>();
            foreach (var command in ConsoleCommandManager.Instance.LoadedCommands)
            {
                if (command == null)
                    continue;
                var type = command.GetType();
                if (type.GetCustomAttribute<HiddenCommandAttribute>() != null)
                    continue;

                var attr = type.GetCustomAttribute<CommandInfoAttribute>();
                if (attr == null)
                {
                    if (!dict.ContainsKey(string.Empty))
                        dict.Add(string.Empty, new List<string>());
                    dict[string.Empty].Add($"    {command.CommandName}");
                    continue;
                }

                string groupKey = attr.GroupName;
                if (string.IsNullOrWhiteSpace(groupKey))
                    groupKey = string.Empty;
                
                if (!dict.ContainsKey(groupKey))
                    dict.Add(groupKey, new List<string>());
                
                dict[groupKey].Add($"    {command.CommandName} - {attr.Description}");
            }

            var lines = new List<string>();
            int keyIndex = 0;
            foreach (var key in dict.Keys)
            {
                if (key.IsNullOrEmpty())
                    lines.Add("System:");
                else
                    lines.Add($"{key.ToUpperInvariant()}:");
                
                foreach (var line in dict[key])
                    lines.Add(line);
                
                if (keyIndex < dict.Count - 1)
                    lines.Add(string.Empty);
                
                keyIndex++;
            }

            return lines.ToArray();
        }
    }
}