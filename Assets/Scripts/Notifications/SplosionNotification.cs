using UnityEngine;

namespace Notifications
{
    public class SplosionNotification  : NotificationView<ECS.Notification.SplosionNotification>
    {
        [SerializeField]
        private Animator animator;
        
        public void OnAnimationFinished()
        {
            _returnToPool?.Invoke();
        }
        
        public override void Notify(ECS.Notification.SplosionNotification notification)
        {
            transform.position = notification._position;
        }

        public override void OnReturn()
        {
           
        }
    }
}
