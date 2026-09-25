using System;
using UnityEngine;

namespace MirrorNetTest.UI
{
    public abstract class WidgetBase : MonoBehaviour, IDisposable
    {
        void Awake()
        {
            RemoveListeners();
            RegisterListeners();
        }

        protected virtual void RegisterListeners()
        {
            // no-op
        }

        protected virtual void RemoveListeners()
        {
            // no-op
        }

        public void Dispose()
        {
            RemoveListeners();
        }
    }
}