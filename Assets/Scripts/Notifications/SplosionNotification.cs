using ECS.Entities;
using UnityEngine;

namespace Notifications
{
    public class SplosionNotification  : NotificationView<ECS.Notification.SplosionNotification>
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AK.Wwise.Event splosionSound;
        
        [SerializeField]
        private AK.Wwise.Event mineSplosionSound;
        
        public void OnAnimationFinished()
        {
            _returnToPool?.Invoke();
        }
        
        public override void Notify(ECS.Notification.SplosionNotification notification)
        {
            transform.position = notification._position;
            
            if (notification._type == SplosionType.Granade)
            splosionSound.Post(gameObject);
            if (notification._type == SplosionType.Mine)
                mineSplosionSound.Post(gameObject);
        }

        public override void OnReturn()
        {
           
        }
    }
}
