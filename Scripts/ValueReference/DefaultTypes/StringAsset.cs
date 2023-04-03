using System;

namespace Rhinox.Magnus.Values
{
    public class StringAsset : ValueAsset<string>
    {
    }

    [Serializable]
    public class String_Reference : ValueReference<string, StringAsset>
    {

    }
}