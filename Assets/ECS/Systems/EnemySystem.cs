using Domain;
using ECS.Events;
using ECS.Notification;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Systems
{
    public class EnemySystem
    {
        private readonly float _minDot = math.cos(Constants.HeavyDamageConeAngle * Mathf.Deg2Rad);

        public void Execute(World world, int tick)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = snapshot._enemies.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var enemy = ref snapshot._enemies.GetEntityAt(i);

                // closest anschauen
                var closestId = -1;
                var closestDistance = float.MaxValue;
                var closestPosition = Vector2.zero;
                for (var j = snapshot._players.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var player = ref snapshot._players.GetEntityAt(j);

                    var distance = Vector2.Distance(player._position, enemy._position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestId = snapshot._players.GetIdAt(j);
                        closestPosition = player._position;
                    }
                }

                if (closestId != -1)
                {
                    var direction = closestPosition - enemy._position;

                    enemy._angle = Mathf.Atan2(
                        direction.y,
                        direction.x
                    ) * Mathf.Rad2Deg;
                }
            }

            for (var i = snapshot._projectiles.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var projectile = ref snapshot._projectiles.GetEntityAt(i);
                for (var j = snapshot._enemies.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var enemy = ref snapshot._enemies.GetEntityAt(j);

                    var distance = projectile._position - enemy._position;

                    if (distance.sqrMagnitude - Constants.GunManColliderRadius <= 1f)
                    {
                        snapshot._projectiles.DespawnAt(i);
                        enemy._health -= 30;
                        if (enemy._health <= 0)
                        {
                            enemy._health = 0;
                            snapshot._enemies.DespawnAt(j);
                        }

                        break;
                    }
                }
            }

            #region heavy

            if (world._hasHeavyDamageCone)
            {
                var radians = world._heavyDamageConeEvent._angle * Mathf.Deg2Rad;

                var direction = new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

                for (var i = snapshot._enemies.GetNumEntities() - 1; i >= 0; i--)
                {
                    ref var enemy = ref snapshot._enemies.GetEntityAt(i);

                    var directionToEnemy = enemy._position - world._heavyDamageConeEvent._position;

                    var distance = directionToEnemy.magnitude;

                    if (distance < 0.1f)
                    {
                        enemy._health -= 10;
                        if (enemy._health <= 0f)
                        {
                            snapshot._damageProcess--;
                            if (snapshot._damageProcess < 0)
                                snapshot._damageProcess = 0;

                            snapshot._enemyDiedNotificationPool.Add(new EnemyDiedNotification()
                            {
                                _enemyType = enemy._type,
                                _isKnockback = true,
                                _knockbackDirection = directionToEnemy.normalized,
                                _position = enemy._position
                            });

                            snapshot._enemies.DespawnAt(i);
                        }

                        continue;
                    }

                    if (distance < Constants.HeavyDamageConeRadius)
                    {
                        var dirNormalized = directionToEnemy.normalized;

                        var dot = math.dot(direction, dirNormalized);

                        if (dot > _minDot)
                        {
                            enemy._health -= 10;
                            if (enemy._health <= 0f)
                            {
                                snapshot._damageProcess--;
                                if (snapshot._damageProcess < 0)
                                    snapshot._damageProcess = 0;

                                snapshot._enemyDiedNotificationPool.Add(new EnemyDiedNotification()
                                {
                                    _enemyType = enemy._type,
                                    _isKnockback = true,
                                    _knockbackDirection = dirNormalized.normalized,
                                    _position = enemy._position
                                });

                                snapshot._enemies.DespawnAt(i);
                            }
                        }
                    }
                }
            }

            if (world._hasBlastWaveEvent)
                BlastWaveEvent(ref snapshot, world._blastWaveEvent);

            if (world._hasGranadeExplosionEvent)
                BlastWaveEvent(ref snapshot, world._granadeExplosionEvent);

            var heavy = snapshot._players.GetEntityAt(0);
            if (heavy._secondaryActivatedTo > tick)
            {
                for (var i = snapshot._enemies.GetNumEntities() - 1; i >= 0; i--)
                {
                    ref var enemy = ref snapshot._enemies.GetEntityAt(i);
                    if (Vector2.Distance(heavy._position, enemy._position) < 1.0f)
                    {
                        enemy._health -= 100;
                        if (enemy._health <= 0)
                        {
                            snapshot._damageProcess--;
                            if (snapshot._damageProcess < 0)
                                snapshot._damageProcess = 0;

                            var directionToPlayer = (enemy._position - heavy._position).normalized;

                            snapshot._enemyDiedNotificationPool.Add(new EnemyDiedNotification()
                            {
                                _enemyType = enemy._type,
                                _isKnockback = true,
                                _knockbackDirection = directionToPlayer.normalized,
                                _position = enemy._position
                            });

                            snapshot._enemies.DespawnAt(i);
                        }
                    }
                }
            }

            #endregion
        }

        private void BlastWaveEvent(ref Snapshot snapshot, BlastWaveEvent evt)
        {
            for (var i = snapshot._enemies.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var enemy = ref snapshot._enemies.GetEntityAt(i);

                var distance = Vector2.Distance(enemy._position, evt._position);
                if (distance < evt._radius)
                {
                    enemy._health -= 100;
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
                    }
                }
            }
        }
    }
}
