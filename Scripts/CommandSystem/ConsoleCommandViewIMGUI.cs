using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class ConsoleCommandViewIMGUI : MonoBehaviour
    {
        private bool _visible;
        public bool IsVisible => _visible;
        private string[] _historyTexts;
        private string _currentCommand;
        private bool _justOpened;

        private ICollection<IConsoleCommand> _suggestions = new List<IConsoleCommand>();
        private const int SUGGESTIONS_SIZE_LIMIT = 4;
        private const int SUGGESTION_WINDOW_ID = 150151;
        private Vector2 _suggestionScrollPosition;

        private const int WINDOW_WIDTH = 450;
        private const int WINDOW_HEIGHT = 300;

        private LimitedQueue<string> _outputHistory;
        private const int HISTORY_LIMIT_LINES = 500;
        private const int HISTORY_VIEW_SIZE = 10;

        private int _viewOffset = 0;
        private Vector2 _scrollPosition;
        private LimitedQueue<string> _commandHistory;
        private const int COMMAND_LIMIT_HISTORY = 20;

        private const int UNIQUE_WINDOW_ID = 1597368;
        private const string INPUT_FIELD_NAME = "input";

        private int _pickPreviousCommand;
        private int _renderedCountOutput;
        private float _labelHeight;

        private void Awake()
        {
            _outputHistory = new LimitedQueue<string>(HISTORY_LIMIT_LINES);
            _commandHistory = new LimitedQueue<string>(COMMAND_LIMIT_HISTORY);
            _pickPreviousCommand = -1;
            _renderedCountOutput = -1;
            _currentCommand = string.Empty;

            GUIStyle defaultLabelStyle = ConsoleGUIStyles.ConsoleLabelStyle;
            _labelHeight =
                defaultLabelStyle.CalcHeight(new GUIContent("Sample Label"), EditorGUIUtility.currentViewWidth);
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

        private void OnGUI()
        {
            if (_visible)
            {
                var backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.gray;

                // draw dialog
                GUILayout.Window(UNIQUE_WINDOW_ID,
                    new Rect(0, Screen.height - WINDOW_HEIGHT, WINDOW_WIDTH, WINDOW_HEIGHT),
                    OnDrawWindow, GUIContent.none, ConsoleGUIStyles.BoxStyle);

                // If there are no suggestions, return
                if (_suggestions.IsNullOrEmpty())
                {
                    //Reset the background color
                    GUI.backgroundColor = backgroundColor;
                    return;
                }

                // Calculate the appropriate height of the suggestion window
                float height;
                if (_suggestions.Count <= SUGGESTIONS_SIZE_LIMIT)
                    height = _suggestions.Count * _labelHeight;
                else
                    height = SUGGESTIONS_SIZE_LIMIT * _labelHeight;

                GUI.backgroundColor = Color.black;
                
                // draw suggestions
                GUILayout.Window(SUGGESTION_WINDOW_ID,
                    new Rect(0, Screen.height - WINDOW_HEIGHT, WINDOW_WIDTH, height),
                    OnDrawSuggestionWindow, GUIContent.none, ConsoleGUIStyles.BoxStyle);

                //Reset the background color
                GUI.backgroundColor = backgroundColor;
            }
        }

        private void OnDrawWindow(int windowID)
        {
            // check for keydown event
            if (Event.current.type == EventType.KeyDown)
            {
                // no matter where, but if Escape was pushed, close the dialog
                if ( /*Event.current.keyCode == KeyCode.Escape || */ // TODO: escape gives weird open close behaviour
                    Event.current.keyCode == ConsoleCommandManager.OPEN_KEY)
                {
                    GUI.FocusControl(string.Empty);
                    _visible = false;
                    return; // no point in continuing if closed
                }

                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    GetPreviousCommandUp();
                    return; // Swallow
                }
                else if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    GetPreviousCommandDown();
                    return; // Swallow
                }

                // we look if the event occured while the focus was in our input element
                if (GUI.GetNameOfFocusedControl() == INPUT_FIELD_NAME && Event.current.keyCode == KeyCode.Return)
                {
                    _suggestions.Clear();
                    SubmitInputValue();
                    return;
                }
            }


            //else
            {
                GUILayout.BeginVertical(ConsoleGUIStyles.ToolbarButtonStyle);
                {
                    GUILayout.FlexibleSpace();
                    if (_outputHistory != null)
                    {
                        if (_renderedCountOutput != _outputHistory.Count)
                            _scrollPosition.y =
                                _outputHistory.Count * 40; // Forces to end fully while history is not at capacity
                        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true,
                            GUILayout.Height(WINDOW_HEIGHT - 40), GUILayout.ExpandHeight(true));
                        GUILayout.BeginVertical();
                        {
                            try
                            {
                                foreach (var entry in _outputHistory)
                                    GUILayout.Label(entry, ConsoleGUIStyles.ConsoleLabelStyle);
                                _renderedCountOutput = _outputHistory.Count;
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.ToString());
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.EndScrollView();
                    }

                    GUI.SetNextControlName(INPUT_FIELD_NAME);
                    var newCommandText = GUILayout.TextField(_currentCommand);
                    if (newCommandText != _currentCommand)
                        _suggestions = (ConsoleCommandManager.Instance.GetSuggestions(newCommandText));

                    _currentCommand = newCommandText;

                    if (_justOpened && _currentCommand.StartsWith("`"))
                    {
                        _currentCommand = _currentCommand.Substring(1);
                        _justOpened = false;
                    }
                }
                GUILayout.EndVertical();
            }

            // in case nothing else if focused, focus our input
            if (GUI.GetNameOfFocusedControl() == string.Empty)
                GUI.FocusControl(INPUT_FIELD_NAME);
        }

        private void OnDrawSuggestionWindow(int windowID)
        {
            GUILayout.BeginVertical();

            int limit = (_suggestions.Count > SUGGESTIONS_SIZE_LIMIT) ? SUGGESTIONS_SIZE_LIMIT : _suggestions.Count;
            for (int i = 0; i < limit; ++i)
            {
                var element = _suggestions.ElementAt(i);
                GUILayout.Label(element.Syntax, ConsoleGUIStyles.ConsoleLabelStyle);
            }

            GUILayout.EndVertical();
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

        #region RuntimeGUIStyles

        #endregion

        public void Clear()
        {
            _commandHistory.Clear();
            _outputHistory.Clear();
            _suggestions.Clear();
        }
    }
}