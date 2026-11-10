using ECS;
using UnityEngine;

namespace Notifications
{
    public class DoorDestructionNotification : NotificationView<ECS.Notification.DoorDestructionNotification>
    {
        [SerializeField]
        private ParticleSystem particleSystem;

        public override void Notify(ECS.Notification.DoorDestructionNotification notification)
        {
            var direction = notification._direction;
            
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.position = (notification._door._leftBottom + notification._door._rightTop) / 2f;
            
            transform.rotation = Quaternion.Euler(angle, 90, 0);

            particleSystem.Play();
            
            Invoke(nameof(ReturnThatStuff), 12f);
        }

        public void ReturnThatStuff()
        {
            _returnToPool?.Invoke();
        }
        
        public override void OnReturn()
        {

        }
    }
}
