using System;

namespace Rhinox.Magnus.Values
{
    public class FloatAsset : ValueAsset<float>
    {
    }

    [Serializable]
    public class Float_Reference : ValueReference<float, FloatAsset>
    {
    }
}