using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Delegates
    {
        public delegate void             ShallowDelegate();
        public delegate float            InterpolationDelegate(float t);
        public delegate void             CallbackDelegate<T>(T data);
        public delegate TaleUtil.Action  BranchDelegate<T>(T data);
        public delegate R                MapDelegate<T, R>(T data);
        public delegate float            DeltaDelegate();
    }
}