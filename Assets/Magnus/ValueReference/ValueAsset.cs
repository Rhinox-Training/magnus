using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Values
{
    public abstract class ValueAsset<T> : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField, HideLabel, DisableInPlayMode] [OnValueChanged(nameof(UpdateValue))]
        protected T _initialValue;

        [ShowInPlayMode] protected T _runtimeValue;

        public T Value
        {
            get => _runtimeValue;
            set => _runtimeValue = value;
        }

        protected virtual void UpdateValue()
        {
            _runtimeValue = _initialValue;
        }

        public virtual void OnAfterDeserialize()
        {
            UpdateValue();
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public static implicit operator T(ValueAsset<T> asset) => asset.Value;
    }
}