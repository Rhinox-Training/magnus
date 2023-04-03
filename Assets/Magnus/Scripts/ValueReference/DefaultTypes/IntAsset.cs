using System;

namespace Rhinox.Magnus.Values
{
    public class IntAsset : ValueAsset<int>
    {
    }

    [Serializable]
    public class Int_Reference : ValueReference<int, IntAsset>
    {
    }
}