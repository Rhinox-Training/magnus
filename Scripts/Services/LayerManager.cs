using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class LayerData
    {
        public int BaseLayer;
        public List<int> LayerOverrides = new List<int>();

        public LayerData(int baseLayer)
        {
            BaseLayer = baseLayer;
        }
    }
    
    [ServiceLoader]
    public class LayerManager : AutoService<LayerManager>
    {
        private Dictionary<GameObject, LayerData> _layerInformation = new Dictionary<GameObject, LayerData>();
        
        public void SetLayer(GameObject gameObject, int layer)
        {
            if (!_layerInformation.ContainsKey(gameObject))
                _layerInformation.Add(gameObject, new LayerData(gameObject.layer));
            
            _layerInformation[gameObject].LayerOverrides.Add(layer);
            gameObject.SetLayerRecursively(_layerInformation[gameObject].LayerOverrides.Last());
        }
        
        public void UnsetLayer(GameObject gameObject, int layer)
        {
            if (!_layerInformation.ContainsKey(gameObject))
                return;
            
            _layerInformation[gameObject].LayerOverrides.Remove(layer);

            if (_layerInformation[gameObject].LayerOverrides.Count == 0)
            {
                gameObject.SetLayerRecursively(_layerInformation[gameObject].BaseLayer);
                _layerInformation.Remove(gameObject);
                return;
            }
            
            gameObject.SetLayerRecursively(_layerInformation[gameObject].LayerOverrides.Last());
        }
    }
}