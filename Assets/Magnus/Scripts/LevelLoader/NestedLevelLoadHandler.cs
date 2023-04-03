using System;
using System.Collections;
using System.Collections.Generic;

namespace Rhinox.Magnus
{
    public abstract class NestedLevelLoadHandler : ILevelLoadHandler
    {
        public virtual int LoadOrder => LevelLoadOrder.TASK_LOADING;

        protected virtual bool Initialize()
        {
            return true;
        }

        protected abstract ILevelLoadHandler[] CreateHandlers();
        
        public IEnumerator<float> OnLevelLoad()
        {
            if (!Initialize())
                yield break;
            
            var childHandlers = CreateHandlers();
            
            for(int i = 0 ; i < childHandlers.Length; ++i)
            {
                var method = childHandlers[i].OnLevelLoad();
                yield return method.Current;
                
                while(method.MoveNext())
                    yield return method.Current;
            }
        }
    }
}