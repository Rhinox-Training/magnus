using System;
using System.Data;
using UnityEngine;

using static Rhinox.Magnus.LogUtils;

namespace Rhinox.Magnus
{
    public struct ColumnData
    {
        public string Name;
        public string Value;

        public ColumnData(string name, string value)
        {
            Name = name;
            Value = value;
        }
        
        public ColumnData(string name, int value) : this(name, Format(value)) { }
        public ColumnData(string name, float value) : this(name, Format(value)) { }
        public ColumnData(string name, TimeSpan value) : this(name, Format(value)) { }
        public ColumnData(string name, DateTime value, bool includeTime = true): this(name, Format(value, includeTime)) { }
        public ColumnData(string name, MonoBehaviour value) : this(name, Format(value)) { }
        public ColumnData(string name, GameObject value) : this(name, Format(value)) { }
        public ColumnData(string name, Enum value) : this(name, value.ToString()) { }
        
        public static implicit operator ColumnData((string name, int value) t) => new ColumnData(t.name, t.value);
        public static implicit operator ColumnData((string name, float value) t) => new ColumnData(t.name, t.value);
        public static implicit operator ColumnData((string name, string value) t) => new ColumnData(t.name, t.value);
        public static implicit operator ColumnData((string name, TimeSpan value) t) => new ColumnData(t.name, t.value);
        public static implicit operator ColumnData((string name, DateTime value) t) => new ColumnData(t.name, t.value);
        public static implicit operator ColumnData((string name, DateTime value, bool includeTime) t) => new ColumnData(t.name, t.value, t.includeTime);
        public static implicit operator ColumnData((string name, MonoBehaviour value) t) => new ColumnData(t.name, t.value);
        public static implicit operator ColumnData((string name, GameObject value) t) => new ColumnData(t.name, t.value);
        public static implicit operator ColumnData((string name, Enum value) t) => new ColumnData(t.name, t.value);
    }
}