using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Magnus
{
    public static class LogUtils
    {
        public static string Format(TimeSpan time)
        {
            return $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}";
        }

        public static string Format(DateTime date, bool includeTime = true)
        {
            var dateStr = $"{date.Year}/{date.Month:D2}/{date.Day:D2}";
            if (!includeTime)
                return dateStr;
            return $"{dateStr} {Format(date.TimeOfDay)}";
        }

        public static string Format(MonoBehaviour behaviour)
        {
            if (behaviour == null || string.IsNullOrWhiteSpace(behaviour.name)) return string.Empty;

            return behaviour.name.Replace("(Clone)", "").Trim();
        }

        public static string Format(GameObject behaviour)
        {
            if (behaviour == null || string.IsNullOrWhiteSpace(behaviour.name))
                return string.Empty;

            return behaviour.name
                .Replace("(Clone)", "")
                .Replace("(AssemblyInteractable)", "")
                .Trim();
        }

        public static string Format(int n) => n.ToString();
        public static string Format(float n) => n.ToString("F");
    }
}