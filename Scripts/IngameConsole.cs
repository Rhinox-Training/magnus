using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.Text;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;

namespace Rhinox.Magnus
{
    public class ConsoleMessage
    {
        public readonly DateTime Time;
        public readonly string Message;
        public readonly string StackTrace;
        public readonly LogType Type;

        public ConsoleMessage(string message, string stackTrace, LogType type)
        {
            Time = DateTime.Now;
            Message = $"{Time:HH:mm:ss -} {message}";
            StackTrace = stackTrace;
            Type = type;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return $"<color=red>{Message}</color>\n";

                case LogType.Warning:
                    return $"<color=yellow>{Message}</color>\n";

                default:
                    return $"<color=white>{Message}</color>\n";
            }
        }
    }

    public class IngameConsole : MonoBehaviour
    {
        public int MaxDisplayedEntries = 23;
        public bool ShowLogs = true;
        public bool ShowWarnings = true;
        public bool ShowErrors = true;

        LimitedQueue<ConsoleMessage> _entries = new LimitedQueue<ConsoleMessage>(1000);
        TextMeshProUGUI _consoleText;
        StringBuilder _sb = new StringBuilder();

        void Awake()
        {
            _consoleText = GetComponentInChildren<TextMeshProUGUI>();
            _consoleText.text = string.Empty;
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            var entry = new ConsoleMessage(message, stackTrace, type);
            _entries.Enqueue(entry);

            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    if (ShowErrors) RenderEntries();
                    break;

                case LogType.Warning:
                    if (ShowWarnings) RenderEntries();
                    break;

                default:
                    if (ShowLogs) RenderEntries();
                    break;
            }
        }

        private void RenderEntries()
        {
            _sb.Clear();

            // Get active entries
            var activeEntries = _entries.Where(x => IsActive(x.Type)).ToList();

            // Append last n amount of entries
            foreach (var displayItem in activeEntries.Skip(Math.Max(0, activeEntries.Count - MaxDisplayedEntries)))
                _sb.Append(displayItem);

            _consoleText.text = _sb.ToString();
        }

        private bool IsActive(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return ShowErrors;

                case LogType.Warning:
                    return ShowWarnings;

                default:
                    return ShowLogs;
            }
        }

        public void ToggleType(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    ShowErrors = !ShowErrors;
                    break;

                case LogType.Warning:
                    ShowWarnings = !ShowWarnings;
                    break;

                default:
                    ShowLogs = !ShowLogs;
                    break;
            }

            RenderEntries();
        }

        public void ToggleWarnings()
        {
            ToggleType(LogType.Warning);
        }

        public void ToggleErrors()
        {
            ToggleType(LogType.Error);
        }

        public void ToggleLogs()
        {
            ToggleType(LogType.Log);
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}