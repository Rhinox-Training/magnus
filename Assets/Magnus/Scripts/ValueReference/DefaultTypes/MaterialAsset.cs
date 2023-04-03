using System;
using UnityEngine;

namespace Rhinox.Magnus.Values
{
    public class MaterialAsset : ValueAsset<Material>
    {
    }

    [Serializable]
    public class Material_Reference : ValueReference<Material, MaterialAsset>
    {
    }
}