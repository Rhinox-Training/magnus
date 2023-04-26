using UnityEngine;

public static class ConsoleGUIStyles
{
    private static GUIStyle _boxStyle;

    public static GUIStyle BoxStyle
    {
        get
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle()
                {
                    overflow = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    normal =
                    {
                        background = Texture2D.whiteTexture
                    }
                };
            }

            return _boxStyle;
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
                    padding = new RectOffset(0, 0, 0, 0),
                    normal = { textColor = Color.white }
                };
            }

            return _consoleLabelStyle;
        }
    }
}