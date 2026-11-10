using UnityEngine;

namespace Notifications
{
    public class BombPlacedNotification : NotificationView<ECS.Notification.BombPlacedNotification>
    {
        public override void Notify(ECS.Notification.BombPlacedNotification notification)
        {
            transform.position = notification._position + Random.insideUnitCircle;
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        public override void OnReturn()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }
}
