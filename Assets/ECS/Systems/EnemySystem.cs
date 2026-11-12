using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Notification;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ECS.Systems
{
    public class EnemySystem
    {
        private readonly float _minDot = math.cos(Constants.HeavyDamageConeAngle * Mathf.Deg2Rad);

        public EnemySystem(int tickRate)
        {
            _tickRate = tickRate;
        }

        private readonly int _tickRate;

        public void Execute(World world, int tick)
        {
            ref var snapshot = ref world.GetSnapshot();

            var nextAngle = 0;

            var random = Random.CreateFromIndex((uint)tick);

            for (var i = snapshot._enemies.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var enemy = ref snapshot._enemies.GetEntityAt(i);

                switch (enemy._type)
                {
                    case EnemyType.RedHairGuy:
                        HandleRedHairGuy(ref enemy, ref snapshot, ref nextAngle, tick, world);
                        break;
                    case EnemyType.MeleeChick:
                        if (snapshot._players.GetNumEntities() >= 1)
                            HandleMeleeChick(ref enemy, ref snapshot, ref nextAngle, tick, world);
                        break;
                }
            }

            // Spawning melee chicks
            if (tick % (_tickRate * 4) == 0 && tick != 0)
            {
                if (snapshot._enemies.GetNumEntities() < snapshot._enemies.GetMaxLength()
                    // && snapshot._players.GetNumEntities() >= 2
                   )
                {
                    var heavy2 = snapshot._players.GetEntityAt(0);

                    for (var i = 0; i < 10; i++)
                    {
                        var val = random.NextFloat(0, 1) * 360f * Mathf.Deg2Rad;

                        var vec = new Vector2(Mathf.Cos(val), Mathf.Sin(val));

                        var distanceVariance = random.NextFloat(-0.5f, 0.5f);

                        var targetPos = heavy2._position +
                                        vec * (distanceVariance + Constants.ChickSpawnDistanceAverage);

                        if (targetPos.x >= 0 &&
                            targetPos.y >= 0 &&
                            targetPos.x < world._map._width &&
                            targetPos.y < world._map._height &&
                            !world.IsBlocked(targetPos, Constants.ChickColliderRadius))
                        {
                            var enemy = new Enemy()
                            {
                                _angle = 0,
                                _colliderRadius = Constants.ChickColliderRadius,
                                _health = 3,
                                _type = EnemyType.MeleeChick,
                                _position = targetPos
                            };

                            snapshot._enemies.Spawn(enemy);

                            break;
                        }
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

        private bool HasPlayerInView(Snapshot snapshot, Enemy enemy)
        {
            var startPos = enemy._position;
            var stepSize = 1f / 16f;
            for (var i = 0; i < snapshot._players.GetNumEntities(); i++)
            {
                ref var player = ref snapshot._players.GetEntityAt(i);

                var direction = (player._position - startPos).normalized;
                for (var j = 0; j < 800; j++)
                {
                    var step = player._position - (startPos + j * stepSize * direction);

                    if (step.sqrMagnitude < 1f * 1f)
                        return true;
                }
            }

            return false;
        }

        private void HandleMeleeChick(ref Enemy enemy, ref Snapshot snapshot, ref int nextAngle, int tick, World world)
        {
            ref var heavy = ref snapshot._players.GetEntityAt(0);

            var direction = heavy._position - enemy._position;

            var distanceToHeavy = direction.sqrMagnitude;

            if (distanceToHeavy <= Constants.ChickRange * Constants.ChickRange)
            {
                if (enemy._toTickNoShoot <= tick)
                {
                    enemy._toTickNoShoot = tick + (int)(Constants.ChickCooldown * _tickRate);

                    heavy._health -= Constants.ChickDamage;
                }

                enemy._linearVelocity = Vector2.zero;
            }
            else
            {
                enemy._linearVelocity = direction.normalized;
            }
        }

        private void HandleRedHairGuy(ref Enemy enemy, ref Snapshot snapshot, ref int nextAngle, int tick, World world)
        {
            if (enemy._toTickThisDirection <= tick && HasPlayerInView(snapshot, enemy))
            {
                enemy._toTickThisDirection = tick + _tickRate * 1;

                var rad = nextAngle * Mathf.Deg2Rad;

                var direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                enemy._linearVelocity = direction;

                nextAngle += 135;
            }

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

            if (enemy._toTickNoShoot <= tick)
            {
                enemy._toTickNoShoot = tick + _tickRate * 4;

                world.AddShootEvent(new ShootEvent()
                {
                    _angle = enemy._angle,
                    _position = enemy._position,
                    _type = ProjectileType.RedHairGuy,
                    _ttl = _tickRate * 5
                });
            }
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
