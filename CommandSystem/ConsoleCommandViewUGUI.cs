using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using TMPro;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class ConsoleCommandViewUGUI : MonoBehaviour
    {
        public TextMeshProUGUI Input;
        public TextMeshProUGUI Suggestion;
        
        private bool _visible;
        public bool IsVisible => _visible;
        private string[] _historyTexts;
        private string _currentCommand;
        private bool _justOpened;


        private LimitedQueue<string> _outputHistory;
        private const int HISTORY_LIMIT_LINES = 500;
        private const int HISTORY_VIEW_SIZE = 10;
        
        private int _viewOffset = 0;
        private Vector2 _scrollPosition;
        private LimitedQueue<string> _commandHistory;
        private const int COMMAND_LIMIT_HISTORY = 20;

        private int _pickPreviousCommand;
        private int _renderedCountOutput;

        private Canvas _UiRoot;

        private void Awake()
        {
            _outputHistory = new LimitedQueue<string>(HISTORY_LIMIT_LINES);
            _commandHistory = new LimitedQueue<string>(COMMAND_LIMIT_HISTORY);
            _pickPreviousCommand = -1;
            _renderedCountOutput = -1;
            _currentCommand = string.Empty;
            
        }

        private void Start()
        {
            _visible = true;
            _justOpened = true;
        }

        public void Toggle()
        {
            _visible = !_visible;
            if (_visible)
                _justOpened = true;
        }
        
        private void SubmitInputValue()
        {
            // do not proceed if empty
            if (string.IsNullOrWhiteSpace(_currentCommand))
                return;
            
            var sendCommand = _currentCommand.Trim();
            ConsoleCommandManager.Instance.ExecuteCommand(sendCommand);
            
            _commandHistory.Enqueue(sendCommand);

            ResetElements();
        }

        private void ResetElements()
        {
            _pickPreviousCommand = -1;
            _currentCommand = string.Empty;
        }

        public void PrintOutput(string output)
        {
            _outputHistory.Enqueue(output);
            _scrollPosition.y = _outputHistory.Count * 40; // Forces almost to end
        }
        
        private void UpdateView(bool forceToEnd = false)
        {
            var historyTexts = new List<string>();
            
            for (int i = HISTORY_VIEW_SIZE - 1; i >= 0; --i)
            {
                int readIndex = _viewOffset + i;
                if (readIndex < 0 || readIndex >= _outputHistory.Count)
                    continue;
                string text = _outputHistory.ElementAt(readIndex);
                historyTexts.Add(text);
            }

            _historyTexts = historyTexts.ToArray();

            if (forceToEnd)
                _scrollPosition.y = _outputHistory.Count * 40; // Forces almost to end
        }

        // ======
        // COMMAND HISTORY
        private void GetPreviousCommandUp()
        {
            if (_pickPreviousCommand == _commandHistory.Count - 1)
                return;
            _pickPreviousCommand++;
            _pickPreviousCommand = Math.Min(_pickPreviousCommand, _commandHistory.Count - 1);
            RefreshInputBox();
        }

        private void GetPreviousCommandDown()
        {
            if (_pickPreviousCommand == -1 || _pickPreviousCommand == 0)
                return;

            _pickPreviousCommand--;
            _pickPreviousCommand = Math.Max(_pickPreviousCommand, 0);
            RefreshInputBox();
        }

        private void RefreshInputBox()
        {
            if (_pickPreviousCommand != -1)
                _currentCommand = _commandHistory.ElementAt(_commandHistory.Count - 1 - _pickPreviousCommand);
        }
    }
}
