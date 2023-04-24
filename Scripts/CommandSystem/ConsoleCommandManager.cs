using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.IO;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [ServiceLoader(-10000)]
    public class ConsoleCommandManager : AutoService<ConsoleCommandManager>
    {
        private Dictionary<string, IConsoleCommand> _loadedCommands;

        public IReadOnlyCollection<IConsoleCommand> LoadedCommands => _loadedCommands != null
            ? (IReadOnlyCollection<IConsoleCommand>)_loadedCommands.Values
            : Array.Empty<IConsoleCommand>();

        public const KeyCode OPEN_KEY = KeyCode.BackQuote;

        private ConsoleCommandViewIMGUI _commandViewIMGUI;
        private bool _guiAccessEnabled;

        private const string COMMAND_FILE_NAME = "commands.ini";

        protected override void OnInitialize()
        {
            base.OnInitialize();
            LoadCommands();
            LoadCommandFiles();
        }

        private void LoadCommands()
        {
            var commandTypes = AppDomain.CurrentDomain.GetDefinedTypesOfType<IConsoleCommand>();
            foreach (var type in commandTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(type) as IConsoleCommand;
                    if (instance == null)
                        continue;

                    if (!RegisterCommand(instance))
                        PLog.Error<MagnusLogger>($"Failed to load command of type {type.FullName}");
                }
                catch (Exception e)
                {
                    PLog.Error<MagnusLogger>($"Failed to create type {type.FullName}, reason: {e.ToString()}");
                    continue;
                }
            }
        }

        private void LoadCommandFiles()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            string developerDefaultPath =
                Path.Combine("%userprofile%\\AppData\\Local\\Unity\\config", COMMAND_FILE_NAME);
            LoadAndExecuteCommands(developerDefaultPath);
#endif
            string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, COMMAND_FILE_NAME);
            LoadAndExecuteCommands(streamingAssetsPath);
        }

        private void LoadAndExecuteCommands(string filePath)
        {
            filePath = Environment.ExpandEnvironmentVariables(filePath);
            filePath = Path.GetFullPath(filePath);
            if (!FileHelper.Exists(filePath))
                return;

            string[] lines = FileHelper.ReadAllLines(filePath);
            if (lines == null)
                return;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWithOneOf("#", "//", "REM "))
                    continue;
                ExecuteCommand(trimmedLine);
            }
        }

        private bool RegisterCommand(IConsoleCommand command)
        {
            if (command == null)
                return false;

            if (!ValidateCommandName(command.CommandName))
            {
                PLog.Trace<MagnusLogger>(
                    $"Cannot register {command}, CommandName {command.CommandName} can only contain letters, numbers, dashes and/or underscores.");
                return false;
            }

            if (_loadedCommands == null)
                _loadedCommands = new Dictionary<string, IConsoleCommand>();

            if (_loadedCommands.ContainsKey(command.CommandName))
            {
                PLog.Trace<MagnusLogger>(
                    $"Cannot register {command}, CommandName {command.CommandName} already taken.");
                return false;
            }

            _loadedCommands.Add(command.CommandName, command);
            return true;
        }

        private bool ValidateCommandName(string commandName)
        {
            Regex r = new Regex("[aA-zZ0-9_-]+");
            return commandName.All(x => r.IsMatch(x.ToString()));
        }

        public bool ExecuteCommand(string commandString)
        {
            PrintOutput($"> {commandString.Trim()}");
            if (!FindCommand(commandString, out IConsoleCommand command, out string[] args))
            {
                PrintOutput($"Did not find command for '{commandString}'");
                return false;
            }

            string[] result = command.Execute(args);
            foreach (var line in result)
                PrintOutput(line);
            return true;
        }

        private bool FindCommand(string commandStr, out IConsoleCommand command, out string[] args)
        {
            command = null;
            args = null;
            if (string.IsNullOrWhiteSpace(commandStr))
                return false;
            commandStr = commandStr.Trim();
            var commandParts = Tokenize(commandStr);
            if (commandParts.Length <= 0)
                return false;

            var commandName = commandParts[0];
            if (!ValidateCommandName(commandName))
                return false;

            if (_loadedCommands == null || !_loadedCommands.ContainsKey(commandName))
                return false;

            command = _loadedCommands[commandName];
            args = commandParts.Skip(1).ToArray();
            return true;
        }

        public ICollection<IConsoleCommand> GetSuggestions(string commandStr)
        {
            if (string.IsNullOrWhiteSpace(commandStr))
                return Array.Empty<IConsoleCommand>();

            var commandParts = Tokenize(commandStr);
            var command = commandParts[0];
            var currentSuggestions = _loadedCommands
                .Where(kvp => kvp.Key.StartsWith(command))
                .Select(kvp => kvp.Value)
                .ToList();
            
            return currentSuggestions;
        }

        private string[] Tokenize(string commandStr)
        {
            if (!commandStr.Contains("\""))
                return commandStr.Split(' ');

            bool cachingToken = false;
            int startToken = 0;
            var tokenLookup = new Dictionary<string, string>();
            for (int i = 0; i < commandStr.Length; ++i)
            {
                char c = commandStr[i];
                if (c == '\"')
                {
                    if (cachingToken)
                    {
                        // Close
                        cachingToken = false;

                        var token = commandStr.Substring(startToken, i - startToken);
                        startToken = -1;
                        string tokenKey = $"Token{tokenLookup.Count}";
                        int difference = token.Length + 2 - tokenKey.Length;
                        i -= difference;
                        commandStr = commandStr.ReplaceFirst($"\"{token}\"", tokenKey);
                        tokenLookup.Add(tokenKey, token);
                    }
                    else
                    {
                        startToken = i + 1;
                        cachingToken = true;
                    }
                }
            }

            var arr = commandStr.Split(' ');
            for (int i = 0; i < arr.Length; i++)
            {
                var str = arr[i];
                if (tokenLookup.ContainsKey(str))
                    arr[i] = tokenLookup[str];
            }

            return arr;
        }

        private void PrintOutput(string output)
        {
            PLog.Trace<MagnusLogger>(output);
            if (_commandViewIMGUI != null)
                _commandViewIMGUI.PrintOutput(output);
        }

        protected override void Update()
        {
            base.Update();
#if !UNITY_EDITOR
            if (!_guiAccessEnabled)
                return;
#endif
            if (Input.GetKeyDown(OPEN_KEY))
            {
                if (_commandViewIMGUI == null)
                    _commandViewIMGUI = transform.GetOrAddComponent<ConsoleCommandViewIMGUI>();
                else if (!_commandViewIMGUI.IsVisible)
                    _commandViewIMGUI.Toggle();
            }
        }

        internal void EnableGUIAccess()
        {
            if (_guiAccessEnabled)
                return;
            _guiAccessEnabled = true;
        }
    }
}