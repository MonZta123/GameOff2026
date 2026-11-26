using ECS.Entities;
using ECS.Notification;

namespace ECS.Systems
{
    public class MoneySystem
    {
        public void Execute(World world)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = snapshot._triggers.GetNumEntities() - 1; i >= 0; i--)
            {
                var trigger = snapshot._triggers.GetEntityAt(i);
                if (trigger._triggerType != TriggerType.Money)
                    continue;

                for (var j = 0; j < snapshot._players.GetNumEntities(); j++)
                {
                    var player = snapshot._players.GetEntityAt(j);
                    
                    if (trigger.IsInCircle(player._position, 0.5f, player._colliderRadius, out _))
                    {
                        snapshot._soundNotificationPool.Add(new SoundNotification
                        {
                            _position = player._position, _soundType = SoundType.CashIn
                        });
                        
                        snapshot._triggerRemoveNotificationPool.Add(new TriggerRemoveNotification
                        {
                            _type = TriggerType.Money
                        });
                        
                        snapshot._triggers.DespawnAt(i);
                        break;
                    }
                }
            }
        }
    }
}
