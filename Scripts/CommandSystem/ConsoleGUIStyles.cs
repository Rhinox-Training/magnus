using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConsoleGUIStyles
{
    private static GUIStyle _consoleBackgroundStyle;

    public static GUIStyle ConsoleBackgroundStyle
    {
        get
        {
            if (_consoleBackgroundStyle == null)
            {
                _consoleBackgroundStyle = new GUIStyle("Box")
                {
                    overflow = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }
            return _consoleBackgroundStyle;
        }
    }
    
    private static GUIStyle _toolbarButtonStyle;

    public static GUIStyle ToolbarButtonStyle
    {
        get
        {
            if (_toolbarButtonStyle == null)
            {
                _toolbarButtonStyle = new GUIStyle("Button")
                {
                    overflow = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }
            return _toolbarButtonStyle;
            
        }
    }

    private static GUIStyle _consoleLabelStyle;

    public static GUIStyle ConsoleLabelStyle
    {
        get
        {
            if (_consoleLabelStyle == null)
            {
                _consoleLabelStyle = new GUIStyle("Label")
                {
                    overflow = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }

            return _consoleLabelStyle;

        }
    }
}
