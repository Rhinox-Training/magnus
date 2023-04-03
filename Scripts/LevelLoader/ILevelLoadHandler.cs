using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus
{
    [AssignableTypeFilter(Expanded = true)]
    public interface ILevelLoadHandler
    {
        int LoadOrder { get; }
        
        IEnumerator<float> OnLevelLoad();
    }
    
    public static class LevelLoadOrder
    {
        public const int TASK_LOADING = 0;
        public const int AUTOCOMPLETE_LOADING = 10;
    }
}