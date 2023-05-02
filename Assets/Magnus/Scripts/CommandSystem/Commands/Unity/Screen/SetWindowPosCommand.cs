using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the position of the window.","Screen")]
    public class SetWindowPosCommand : IConsoleCommand
    {
        public string CommandName => "set-window-position";
        public string Syntax => "set-screen-position <x> <y>";

#if UNITY_STANDALONE_WIN
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy,
            int wFlags);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(System.String className, System.String windowName);

        private static void SetPosition(int x, int y, int resX = 0, int resY = 0)
        {
            SetWindowPos(FindWindow(null, Application.productName), 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
        }
#endif

        public string[] Execute(string[] args)
        {
#if !UNITY_STANDALONE_WIN
            return new[] { "Only supported on Windows standalone" };
#else
            if (args.IsNullOrEmpty())
            {
                return new[] { $"Syntax is: {Syntax}" };
            }

            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);
            SetPosition(x, y);
            return new[] { $"set-screen-position {x} {y}" };
#endif
        }
    }
}