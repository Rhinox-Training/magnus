using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Values
{
    /// <summary>
    /// Base class to represent a value where you wish to provide the choice to work from either a const value or an asset value.
    /// </summary>
    [InlinePropertyAlt]
    public abstract class ValueReference<TValue, TAsset> where TAsset : ValueAsset<TValue>
    {
        [VerticalGroup("H/V")]
        [HorizontalGroup("H")]
        [HideIfGroup("H/V/Val", MemberName = nameof(_useReference), Animate = false)]
        [HideIf("_useReference", Animate = false)]
        [HorizontalGroup("H/V/Val/H"), HideLabel]
        [SerializeField]
        protected TValue _value;

        [ShowIfGroup("H/V/Ref", MemberName = nameof(_useReference), Animate = false)]
        [VerticalGroup("H/V/Ref/H/V"), HideLabel]
        [SerializeField]
        protected TAsset _assetReference;

        [PropertySpace(2)]
        [ShowInInspector, ShowIf("@_assetReference != null && _showReferenceEditor", Animate = false)]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [OrderAfter(nameof(_assetReference))]
        protected TAsset _assetEditor => _assetReference;

        [ShowIf("@_assetReference != null", Animate = false)] [HorizontalGroup("H/V/Ref/H", 14), IconToggle("Pen")] [SerializeField]
        protected bool _showReferenceEditor;

        [HorizontalGroup("H", 14), IconToggle("StarPointer")] [SerializeField]
        protected bool _useReference;

        public TValue Value
        {
            get
            {
                if (!_useReference || _assetReference == null)
                    return _value;

                return _assetReference.Value;
            }
            set
            {
                if (!_useReference || _assetReference == null)
                    _value = value;
                else _assetReference.Value = value;
            }
        }

        public static implicit operator TValue(ValueReference<TValue, TAsset> valueRef) => valueRef.Value;
    }
}