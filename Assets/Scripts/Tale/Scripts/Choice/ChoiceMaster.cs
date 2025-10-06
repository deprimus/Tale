using UnityEngine;

namespace TaleUtil.Scripts
{
    public abstract class ChoiceMaster : MonoBehaviour
    {
        public abstract void Construct(object args, object choices, TaleUtil.Delegates.ShallowDelegate onEnd);
    }
}