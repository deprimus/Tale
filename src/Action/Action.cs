using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public abstract class Action
    {
        public ulong id;

        public abstract bool Run();
        public abstract Action Clone();
    }
}
