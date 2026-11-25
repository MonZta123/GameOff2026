using ECS.Entities;
using ECS.Events;
using ECS.Notification;
using UnityEngine;

namespace ECS.Systems
{
    public class BlastWaveSystem
    {
        public void Execute(World world)
        {
            ref var snapshot = ref world.GetSnapshot();

            if (world._hasBlastWaveEvent)
            {
                BlastWaveEvent(ref snapshot, world._blastWaveEvent);
                snapshot._splosionNotificationPool.Add(new SplosionNotification()
                {
                    _position = world._blastWaveEvent._position,
                    _type = SplosionType.Heavy
                });
            }

            if (world._hasGranadeExplosionEvent)
            {
                BlastWaveEvent(ref snapshot, world._granadeExplosionEvent);
                snapshot._splosionNotificationPool.Add(new SplosionNotification()
                {
                    _position = world._granadeExplosionEvent._position,
                    _type = SplosionType.Granade
                });
            }
        }

        private void BlastWaveEvent(ref Snapshot snapshot, BlastWaveEvent evt)
        {
            var deadEnemy = false;

            for (var i = snapshot._enemies.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var enemy = ref snapshot._enemies.GetEntityAt(i);

                var distance = Vector2.Distance(enemy._position, evt._position);
                if (distance < evt._radius)
                {
                    enemy._health -= evt._damage;
                    if (enemy._health <= 0)
                    {
                        snapshot._damageProcess--;
                        if (snapshot._damageProcess < 0)
                            snapshot._damageProcess = 0;

                        var directionToPlayer = (enemy._position - evt._position).normalized;

                        snapshot._enemyDiedNotificationPool.Add(new EnemyDiedNotification()
                        {
                            _enemyType = enemy._type,
                            _isKnockback = true,
                            _knockbackDirection = directionToPlayer.normalized,
                            _position = enemy._position
                        });

                        snapshot._enemies.DespawnAt(i);
                        deadEnemy = true;
                    }
                }
            }

            for (var i = snapshot._players.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var player = ref snapshot._players.GetEntityAt(i);

                if (evt._ignoreHeavy && player._type == PlayerType.Heavy)
                    continue;

                var distance = Vector2.Distance(player._position, evt._position);
                if (distance < evt._radius)
                {
                    player._health -= 20;
                }
            }

            if (deadEnemy)
            {
                snapshot._soundNotificationPool.Add(new SoundNotification()
                {
                    _soundType = SoundType.HeavyImpact, _position = evt._position
                });
            }
        }
    }
}
