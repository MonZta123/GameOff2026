using System;
using UnityEngine;

namespace Notifications
{
    public abstract class NotificationView<TEntity> : MonoBehaviour
    {
        protected Action _returnToPool;

        public virtual void Init(Action returnAction)
        {
            _returnToPool = returnAction;
        }

        public abstract void Notify(TEntity notification);

        public abstract void OnReturn();
    }
}
