using System.ComponentModel;
using UnityEngine;

namespace TaleUtil.Scripts.Choice
{
    public abstract class ChoiceMaster<TArgs, TChoice> : MonoBehaviour
    {
        public abstract void Present(TArgs args, TChoice[] choices, Delegates.ShallowDelegate onEnd);
    }
}