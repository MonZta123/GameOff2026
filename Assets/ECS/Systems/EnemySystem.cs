using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Notification;
using Unity.Mathematics;
using UnityEngine;
using EnemyDiedNotification = ECS.Notification.EnemyDiedNotification;
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

            var heavy = snapshot._players.GetEntityAt(0);
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
                        if (snapshot._players.GetNumEntities() > 1)
                            HandleMeleeChick(ref enemy, ref snapshot, tick);
                        break;
                    case EnemyType.Buttler:
                        if (snapshot._players.GetNumEntities() == 4)
                            HandleButtler(ref enemy, ref snapshot, world, tick);
                        break;
                }
            }

            #region buttler

            var currentButtler = (Enemy)default;

            var buttlerIndex = -1;

            for (var i = 0; i < snapshot._enemies.GetNumEntities(); i++)
            {
                var enemy = snapshot._enemies.GetEntityAt(i);
                if (enemy._type == EnemyType.Buttler)
                {
                    currentButtler = enemy;
                    buttlerIndex = i;
                    break;
                }
            }

            var currentPlayer = snapshot._players.GetLastEntity();

            if (buttlerIndex != -1 &&
                (currentPlayer._position - currentButtler._position).sqrMagnitude >
                Constants.ButtlerRespawnDistanceUnits * Constants.ButtlerRespawnDistanceUnits)
            {
                snapshot._tickButtlerRespawnAt = tick;
                snapshot._enemies.DespawnAt(buttlerIndex);
            }

            if (snapshot._tickButtlerRespawnAt == tick)
            {
                var canSpawn = GetButtlerPosition(random, currentPlayer._position, world, out var buttlerPosition);
                if (canSpawn)
                {
                    var enemy = new Enemy
                    {
                        _type = EnemyType.Buttler,
                        _position = buttlerPosition,
                        _health = 100,
                        _lockedOnAt = -1,
                        _shootsAtTick = -1,
                        _toTickNoShoot = _tickRate,
                        _colliderRadius = Constants.ButtlerColliderRadius,
                    };

                    snapshot._enemies.Spawn(enemy);
                }
                else
                {
                    snapshot._tickButtlerRespawnAt = tick + 1;
                }
            }

            #endregion

            #region melee spawn

            // Spawning melee chicks
            if (tick % (_tickRate * 4) == 0 && tick != 0)
            {
                if (snapshot._enemies.GetNumEntities() < snapshot._enemies.GetMaxLength()
                    && snapshot._players.GetNumEntities() > 1
                   )
                {
                    var heavy2 = snapshot._players.GetEntityAt(0);

                    for (var i = 0; i < 10; i++)
                    {
                        var val = random.NextFloat(0, 1) * 360f * Mathf.Deg2Rad;

                        var vec = new Vector2(Mathf.Cos(val), Mathf.Sin(val));

                        var distanceVariance = random.NextFloat(-0.5f, 0.5f);

                        var targetPos = heavy2._position +
                                        vec * (distanceVariance + Constants.ChickSpawnDistanceAverageUnits);

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

            #endregion


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

        private bool GetButtlerPosition(Random random, Vector2 currentPlayer, World world, out Vector2 spawnPosition)
        {
            var angle = random.NextFloat(0, 360f);

            var maxLength = Mathf.Min(8000, Constants.ButtlerRespawnDistanceUnits * 16);

            for (var i = 0; i < 4; i++)
            {
                var rad = (angle + i * 90) * Mathf.Deg2Rad;

                var vec = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                var result = MoveAndSlide(maxLength, currentPlayer, vec, world);
                if ((result - currentPlayer).sqrMagnitude >
                    Constants.ButtlerRespawnMinDistanceUnits * Constants.ButtlerRespawnMinDistanceUnits)
                {
                    spawnPosition = result;
                    return true;
                }
            }

            spawnPosition = Vector2.zero;
            return false;
        }

        private Vector2 MoveAndSlide(int maxLength, Vector2 currentPlayer, Vector2 direction, World world)
        {
            const float StepSize = 1f / 16f;

            var previousStep = currentPlayer;

            for (var i = 0; i < maxLength; i++)
            {
                var step = previousStep + StepSize * direction;

                var nextStepY = new Vector2(previousStep.x, step.y);
                var nextStepX = new Vector2(step.x, previousStep.y);

                var yBlocked = false;
                if (world.IsBlocked(nextStepY, Constants.ButtlerColliderRadius))
                {
                    step.y = previousStep.y;
                    yBlocked = true;
                }

                var xBlocked = false;
                if (world.IsBlocked(nextStepX, Constants.ButtlerColliderRadius))
                {
                    step.x = previousStep.x;
                    xBlocked = true;
                }

                if (yBlocked && xBlocked || world.IsBlocked(step, Constants.ButtlerColliderRadius))
                    return previousStep;

                previousStep = step;
            }

            return previousStep;
        }


        private void HandleButtler(ref Enemy enemy, ref Snapshot snapshot, World world, int tick)
        {
            if (enemy._toTickNoShoot < tick)
            {
                for (var i = 0; i < snapshot._players.GetNumEntities(); i++)
                {
                    ref var player = ref snapshot._players.GetEntityAt(i);

                    if (HasView(enemy._position, player._position, world, Constants.ButtlerMaxViewDistanceUnits))
                    {
                        var direction = player._position - enemy._position;

                        var angle = Mathf.Atan2(
                            direction.y,
                            direction.x
                        ) * Mathf.Rad2Deg;

                        if (enemy._lockedOnAt == -1 || enemy._lockedOnAt != i)
                        {
                            var lockNewTarget = false;

                            if (enemy._lockedOnAt != -1)
                            {
                                var currentTarget = snapshot._players.GetEntityAt(enemy._lockedOnAt);

                                lockNewTarget = HasView(enemy._position,
                                    currentTarget._position, world,
                                    Constants.ButtlerMaxViewDistanceUnits);
                            }

                            if (!lockNewTarget)
                            {
                                enemy._shootsAtTick = tick + (int)(_tickRate * Constants.ButtlerLockOnSeconds);

                                enemy._lockedOnAt = i;
                            }
                        }

                        enemy._angle = angle;
                        break;
                    }

                    if (i == snapshot._players.GetNumEntities() - 1)
                    {
                        enemy._shootsAtTick = -1;
                        enemy._lockedOnAt = -1;
                    }
                }
            }

            if (enemy._shootsAtTick == tick)
            {
                enemy._toTickNoShoot = tick + (int)(_tickRate * Constants.ButtlerCooldown);
                enemy._shootsAtTick = -1;

                var player = snapshot._players.GetEntityAt(enemy._lockedOnAt);

                var direction = (player._position - enemy._position).normalized;

                var endPoint = ButtlerShoot(enemy._position, direction, world, ref snapshot);

                snapshot._snipeShotNotificationPool.Add(new SnipeShotNotification()
                {
                    _from = enemy._position, _to = endPoint,
                });
            }
        }

        private Vector2 ButtlerShoot(Vector2 from, Vector2 direction, World world, ref Snapshot snapshot)
        {
            var stepSize = 1f / 16f;
            var stepDirection = direction * stepSize;

            var start = from + direction.normalized * (Constants.ButtlerColliderRadius + stepSize);

            var i = 0;
            Vector2 currentLazor;
            do
            {
                currentLazor = start + stepDirection * i;

                for (var j = snapshot._enemies.GetNumEntities() - 1; j >= 0; j--)
                {
                    var enmy = snapshot._enemies.GetEntityAt(j);

                    if ((enmy._position - currentLazor).sqrMagnitude <
                        enmy._colliderRadius * enmy._colliderRadius)
                    {
                        snapshot._enemyDiedNotificationPool.Add(new EnemyDiedNotification()
                        {
                            _enemyType = enmy._type,
                            _isKnockback = true,
                            _knockbackDirection = direction.normalized,
                            _position = enmy._position
                        });

                        snapshot._enemies.DespawnAt(j);
                    }
                }

                for (var j = snapshot._players.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var player = ref snapshot._players.GetEntityAt(j);

                    if ((player._position - currentLazor).sqrMagnitude <
                        player._colliderRadius * player._colliderRadius)
                    {
                        player._health = 0;
                    }
                }

                i++;
            } while (!world.IsBlocked(currentLazor) && i < 8000);

            return currentLazor;
        }

        private bool HasView(Vector2 from, Vector2 to, World world, int maxLengthUnits)
        {
            const float StepSize = 1f / 16f;

            var hitDistance = StepSize * StepSize;

            var direction = (to - from).normalized;

            for (var i = 0; i < maxLengthUnits * 16; i++)
            {
                var step = from + i * StepSize * direction;

                if (world.IsBlocked(step))
                    return false;

                if ((to - step).sqrMagnitude < hitDistance)
                    return true;
            }

            return false;
        }

        private void HandleMeleeChick(ref Enemy enemy, ref Snapshot snapshot, int tick)
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
    }
}
