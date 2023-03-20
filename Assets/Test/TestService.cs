using Rhinox.Lightspeed;
using Rhinox.Magnus;
using UnityEngine.UI;

namespace DefaultNamespace
{

    [ServiceLoader(-10)]
    public class TestService : AutoService<TestService>
    {
        protected override void Awake()
        {
            base.Awake();
            transform.GetOrAddComponent<Button>();
        }
    }
}