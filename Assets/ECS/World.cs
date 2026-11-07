using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Systems;
using UnityEngine;

namespace ECS
{
    public enum Direction
    {
        Down, Right
    }

    public class World
    {
        private Snapshot _snapshot;

        public MoveEvent[] _moveEvents;

        private AttackSystem _attackSystem;
        private ProjectileSystem _projectileSystem;
        private DamageSystem _damageSystem;
        private EnemySystem _enemySystem;
        private DoorSystem _doorSystem;
        private HeavySystem _heavySystem;
        private HackerSystem _hackerSystem;
        private DemolitionSystem _demolitionSystem;
        private SniperSystem _sniperSystem;

        public MapDefinition _map;

        public ShootEvent[] _shootEvents;
        public int _numShootEvents;

        public SoundNotification[] _soundEvents;
        public int _numSoundEvents;

        public DoorDestructionNotification[] _doorDestructionNotifications;
        public int _numDoorDestructionNotifications;

        public BlastWaveEvent _blastWaveEvent;
        public bool _hasBlastWaveEvent;

        public HeavyDamageConeEvent _heavyDamageConeEvent;
        public bool _hasHeavyDamageCone;

        public void AddDoorDestructionNotification(DoorDestructionNotification doorDestructionNotification)
        {
            if (_numDoorDestructionNotifications == _doorDestructionNotifications.Length)
                return;

            _doorDestructionNotifications[_numDoorDestructionNotifications] = doorDestructionNotification;
            _numDoorDestructionNotifications++;
        }

        public void AddSoundEvent(SoundNotification soundNotification)
        {
            if (_numSoundEvents == _soundEvents.Length)
                return;

            _soundEvents[_numSoundEvents] = soundNotification;
            _numSoundEvents++;
        }

        public void AddBlastWaveEvent(BlastWaveEvent blastWaveEvent)
        {
            _blastWaveEvent = blastWaveEvent;
            _hasBlastWaveEvent = true;
        }

        public void AddShootEvent(ShootEvent shootEvent)
        {
            if (_numShootEvents == _shootEvents.Length)
                return;

            _shootEvents[_numShootEvents] = shootEvent;
            _numShootEvents++;
        }

        public World(int tickRate)
        {
            _deltaTime = 1f / tickRate;
            _snapshot = new Snapshot();
            _snapshot.Initialize();

            _moveEvents = new MoveEvent[_snapshot._players.GetMaxLength()];
            _shootEvents = new ShootEvent[64];
            _soundEvents = new SoundNotification[64];
            _doorDestructionNotifications = new DoorDestructionNotification[64];

            _attackSystem = new AttackSystem(tickRate);
            _projectileSystem = new ProjectileSystem(tickRate);
            _damageSystem = new DamageSystem();
            _enemySystem = new EnemySystem();
            _heavySystem = new HeavySystem(tickRate);
            _hackerSystem = new HackerSystem(tickRate);
            _demolitionSystem = new DemolitionSystem(tickRate);
            _sniperSystem = new SniperSystem(tickRate);
            _doorSystem = new DoorSystem();
        }

        private float _deltaTime;

        public bool IsBlocked(Vector2 position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= _map._width || position.y >= _map._height)
                return true;

            if (_map._map[(int)position.y * _map._width + (int)position.x] == 0)
                return true;

            for (var i = 0; i < _snapshot._doors.GetNumEntities(); i++)
            {
                var door = _snapshot._doors.GetEntityAt(i);

                if (position.x >= door._leftBottom.x && position.x < door._rightTop.x &&
                    position.y >= door._leftBottom.y && position.y < door._rightTop.y)
                    return true;
            }

            return false;
        }

        public void BuildScenario(ref Snapshot snapshot, MapDefinition mapDefinition)
        {
            // build world
            _map = mapDefinition;

            // First Player
            _currentPlayer = GetCharacter(PlayerType.Heavy);
            _currentPlayer._health = 100;
            _currentPlayer._position = mapDefinition._spawnPoints[0];

            // Generate Doors
            var doorList = mapDefinition._woodenDoors.ToList();

            while (doorList.Count > 0)
            {
                var head = LookupDoor(doorList[0], doorList);

                doorList.RemoveAll(head.Contains);

                var door = new Door();

                if (head[0].x == head[^1].x)
                {
                    door._direction = Direction.Down;
                    door._leftBottom = new Vector2(head[0].x, head[0].y);
                    door._rightTop = new Vector2(head[0].x + 1, head[^1].y + 1);
                    door._health = 3;
                }
                else
                {
                    door._direction = Direction.Right;
                    door._leftBottom = new Vector2(head[0].x, head[0].y);
                    door._rightTop = new Vector2(head[^1].x + 1, head[0].y + 1);
                    door._health = 3;
                }

                snapshot._doors.Spawn(door);
            }

            var triggerPositions = mapDefinition._hackerZone.ToList();

            while (triggerPositions.Count > 0)
            {
                var head = LookupHackerTrigger(triggerPositions[0], triggerPositions);

                var leftBottom = new Vector2(head.Min(n => n.x), head.Min(n => n.y));
                var rightTop = new Vector2(head.Max(n => n.x) + 1, head.Max(n => n.y) + 1);

                var evt = new Trigger()
                {
                    _leftBottom = leftBottom, _rightTop = rightTop, _triggerType = TriggerType.HackerZone
                };

                snapshot._triggers.Spawn(evt);
            }

            var spawnZone = mapDefinition._enemySpawnZone.ToList();

            for (var i = 0; i < 30; i++)
            {
                var index = UnityEngine.Random.Range(0, spawnZone.Count);

                var enemy = new Enemy()
                {
                    _health = 100, _angle = 0, _position = spawnZone[index], _type = EnemyType.Gunman
                };

                snapshot._enemies.Spawn(enemy);
            }

            snapshot._damageProcess = 3;

            // DEBUG
#if UNITY_EDITOR

            // var enemyId = FetchEnemyId(ref snapshot);
            //
            // var enemy = new Enemy(enemyId)
            // {
            //     _health = 100, _angle = 0, _position = new Vector2(3, 0), _type = EnemyType.Gunman
            // };
            //
            // SpawnEnemy(enemy, ref snapshot);
#endif

            // Spawn Enemies and stuff
        }

        public void PlaceMines(Vector2[] positions, ref Snapshot snapshot)
        {
            var list = positions.ToList();
            
            var minePos = new List<Vector2>();
            
            for (var i = 0; i < Math.Min(8, list.Count); i++)
            {
                var position = UnityEngine.Random.Range(0, list.Count);

                minePos.Add(list[position]);
                
                list.RemoveAt(position);
            }

            for (var i = 0; i < minePos.Count; i++)
            {
                var offsetX = UnityEngine.Random.Range(-2, 2);
                var offsetY = UnityEngine.Random.Range(-2, 2);

                var offset = new Vector2(offsetX, offsetY);

                if (_map._map[((int)minePos[i].y + offsetY) * _map._width + ((int)minePos[i].x + offsetX)] == 0)
                    offset = Vector2.zero;

                var mine = new Trigger()
                {
                    _leftBottom = minePos[i] + offset,
                    _rightTop = new Vector2(minePos[i].x + 1, minePos[i].y + 1) + offset,
                    _triggerType = TriggerType.Mine
                };
                
                snapshot._triggers.Spawn(mine);
            }
        }

        private List<Vector2Int> LookupDoor(Vector2Int start, List<Vector2Int> doorPositions)
        {
            var head = new List<Vector2Int> { start };

            return GetNext(start, doorPositions, head);
        }

        private List<Vector2Int> LookupHackerTrigger(Vector2Int start, List<Vector2Int> triggerPositions)
        {
            var head = new List<Vector2Int> { start };
            triggerPositions.Remove(start);

            return GetNextTriggerStep(start, triggerPositions, head);
        }

        private List<Vector2Int> GetNextTriggerStep(Vector2Int current, List<Vector2Int> triggerPositions,
            List<Vector2Int> head)
        {
            var up = new Vector2Int(current.x, current.y + 1);
            if (triggerPositions.Contains(up))
            {
                triggerPositions.Remove(up);
                head.Add(up);
                GetNextTriggerStep(up, triggerPositions, head);
            }

            var down = new Vector2Int(current.x, current.y - 1);
            if (triggerPositions.Contains(down))
            {
                triggerPositions.Remove(down);
                head.Add(down);
                GetNextTriggerStep(down, triggerPositions, head);
            }

            var right = new Vector2Int(current.x + 1, current.y);
            if (triggerPositions.Contains(right))
            {
                triggerPositions.Remove(right);
                head.Add(right);
                GetNextTriggerStep(right, triggerPositions, head);
            }

            var left = new Vector2Int(current.x - 1, current.y);
            if (triggerPositions.Contains(left))
            {
                triggerPositions.Remove(left);
                head.Add(left);
                GetNextTriggerStep(left, triggerPositions, head);
            }

            return head;
        }

        private List<Vector2Int> GetNext(Vector2Int current, List<Vector2Int> doorPositions, List<Vector2Int> head)
        {
            var list = doorPositions.ToList();
            var head2 = head.ToList();

            var up = new Vector2Int(current.x, current.y + 1);
            if (!head2.Contains(up) && list.Contains(up))
            {
                head2.Add(up);
                return GetNext(up, list, head2);
            }

            var down = new Vector2Int(current.x, current.y - 1);
            if (!head2.Contains(down) && list.Contains(down))
            {
                head2.Add(down);
                return GetNext(down, list, head2);
            }

            var right = new Vector2Int(current.x + 1, current.y);
            if (!head2.Contains(right) && list.Contains(right))
            {
                head2.Add(right);
                return GetNext(right, list, head2);
            }

            var left = new Vector2Int(current.x - 1, current.y);
            if (!head2.Contains(left) && list.Contains(left))
            {
                head2.Add(left);
                return GetNext(left, list, head2);
            }

            return head2;
        }

        public void SetState(Snapshot snapshot)
        {
            _snapshot.BuildFrom(snapshot);
        }

        public void Decide(ReadOnlySpan<Intent> intents)
        {
            for (var i = 0; i < intents.Length; i++)
            {
                ref var moveEvent = ref _moveEvents[i];

                moveEvent._movement = intents[i]._movement;
                moveEvent._angle = intents[i]._angle;
                moveEvent._primary = intents[i]._primary;
                moveEvent._secondary = intents[i]._secondary;
                moveEvent._action = intents[i]._action;
            }
        }

        public void DoStep(ReadOnlySpan<Intent> intents, int tick)
        {
            ResetEventLists();
            BuildSensors();
            Decide(intents);
            System(tick);
            Simulate();
        }

        private void ResetEventLists()
        {
            for (var i = 0; i < _snapshot._players.GetMaxLength(); i++)
            {
                _moveEvents[i] = default;
            }

            for (var i = 0; i < _numSoundEvents; i++)
            {
                _soundEvents[i] = default;
            }

            for (var i = 0; i < _numShootEvents; i++)
            {
                _shootEvents[i] = default;
            }

            for (var i = 0; i < _numDoorDestructionNotifications; i++)
            {
                _doorDestructionNotifications[i] = default;
            }

            _heavyDamageConeEvent = default;
            _blastWaveEvent = default;

            _hasHeavyDamageCone = false;
            _hasBlastWaveEvent = false;
            _numDoorDestructionNotifications = 0;
            _numShootEvents = 0;
            _numSoundEvents = 0;
        }

        public void BuildSensors()
        {
        }

        public void System(int tick)
        {
            _attackSystem.Execute(this, tick);
            _heavySystem.Execute(this, tick);
            _hackerSystem.Execute(this, tick);
            _demolitionSystem.Execute(this, tick);
            _sniperSystem.Execute(this, tick);

            _projectileSystem.Execute(this, tick);
            _damageSystem.Execute(this);
            _enemySystem.Execute(this, tick);
            _doorSystem.Execute(this, tick);
        }

        private float GetMoveSpeed(EnemyType type)
        {
            return type switch
            {
                EnemyType.Gunman => Constants.GunmanMoveSpeed,
                _ => 0.0f
            };
        }

        private const float Pushout = 0.01f;

        public void Simulate()
        {
            for (var i = 0; i < _snapshot._players.GetNumEntities(); i++)
            {
                ref var player = ref _snapshot._players.GetEntityAt(i);

                player._angle = _moveEvents[i]._angle;

                var pushout = player._velocity.normalized * Pushout;
                var nextPos = player._position + player._velocity * _deltaTime + pushout;

                var nextX = new Vector2(nextPos.x, player._position.y);

                if (IsBlocked(nextX))
                {
                    if (nextPos.x < player._position.x)
                        nextPos.x = MathF.Ceiling(nextPos.x) + Pushout;
                    else
                        nextPos.x = MathF.Floor(nextPos.x) - Pushout;
                }

                var nextY = new Vector2(player._position.x, nextPos.y);

                if (IsBlocked(nextY))
                {
                    if (nextPos.y < player._position.y)
                        nextPos.y = MathF.Ceiling(nextPos.y) + Pushout;
                    else
                        nextPos.y = MathF.Floor(nextPos.y) - Pushout;
                }

                player._position = nextPos - pushout;
            }

            for (var i = 0; i < _snapshot._projectiles.GetNumEntities(); i++)
            {
                ref var projectile = ref _snapshot._projectiles.GetEntityAt(i);

                var radians = projectile._angle * Mathf.Deg2Rad;

                var direction = new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

                var movement = direction * (Constants.HeavyProjectileSpeed * _deltaTime);

                projectile._position += movement;
            }

            for (var i = 0; i < _snapshot._enemies.GetNumEntities(); i++)
            {
                ref var enemy = ref _snapshot._enemies.GetEntityAt(i);

                enemy._position += enemy._linearVelocity * (GetMoveSpeed(enemy._type) * _deltaTime);
            }
        }

        public ref Snapshot GetSnapshot() => ref _snapshot;

        private Player GetCharacter(PlayerType type)
        {
            var player = new Player();

            switch (type)
            {
                case PlayerType.Heavy:
                    player._type = PlayerType.Heavy;
                    break;
                case PlayerType.Hacker:
                    player._type = PlayerType.Hacker;
                    break;
                case PlayerType.Demolition:
                    player._type = PlayerType.Demolition;
                    break;
                case PlayerType.Stealth:
                    player._type = PlayerType.Stealth;
                    break;
            }

            return player;
        }

        private Player _currentPlayer;

        public void SetNextPlayer(ref Snapshot snapshot)
        {
            var currentPlayer = snapshot._players.GetNumEntities();

            _currentPlayer = GetCharacter((PlayerType)(currentPlayer));
            _currentPlayer._position = _map._spawnPoints[currentPlayer];
            _currentPlayer._health = 100;
            snapshot._players.Spawn(_currentPlayer);
        }

        public void AddHeavyDamageCone(HeavyDamageConeEvent heavyDamageConeEvent)
        {
            _heavyDamageConeEvent = heavyDamageConeEvent;
            _hasHeavyDamageCone = true;
        }
    }
}
