using System;
using UnityEngine;

namespace Rhinox.Magnus.Values
{
    public class ColorAsset : ValueAsset<Color>
    {
    }

    [Serializable]
    public class Color_Reference : ValueReference<Color, ColorAsset>
    {
    }
}