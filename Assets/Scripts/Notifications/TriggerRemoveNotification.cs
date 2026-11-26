using ECS.Entities;
using Trigger = Actors.Trigger;

namespace Notifications
{
    public class TriggerRemoveNotification : NotificationView<ECS.Notification.TriggerRemoveNotification>
    {
        public override void Notify(ECS.Notification.TriggerRemoveNotification notification)
        {
            if (notification._type == TriggerType.Mine)
                Trigger.MineDefused = true;
            
            if (notification._type == TriggerType.Money)
                Trigger.MoneyCollected = true;
            
            _returnToPool?.Invoke();
        }

        public override void OnReturn()
        {
        }
    }
}
