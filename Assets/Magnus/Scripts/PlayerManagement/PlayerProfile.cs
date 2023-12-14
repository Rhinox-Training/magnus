using System;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus
{
    [Serializable]
    public abstract class PlayerProfile
    {
    }

    [Serializable]
    public class AnonymousPlayerProfile : PlayerProfile
    {
        public static readonly AnonymousPlayerProfile Default = new AnonymousPlayerProfile();
    }
}