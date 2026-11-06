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

        private MoveSystem _moveSystem;
        private AttackSystem _attackSystem;
        private ProjectileSystem _projectileSystem;
        private DamageSystem _damageSystem;
        private EnemySystem _enemySystem;
        private DoorSystem _doorSystem;
        private HeavySystem _heavySystem;

        public BlastWaveEvent[] _blastWaveEvents;
        public int _numBlastWaveEvents;

        private MapDefinition _map;


        public ShootEvent[] _shootEvents;
        public int _numShootEvents;

        public SoundNotification[] _soundEvents;
        public int _numSoundEvents;

        public DoorDestructionNotification[] _doorDestructionNotifications;
        public int _numDoorDestructionNotifications;

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
            if (_numBlastWaveEvents == _blastWaveEvents.Length)
                return;

            _blastWaveEvents[_numBlastWaveEvents] = blastWaveEvent;
            _numBlastWaveEvents++;
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

            _moveSystem = new MoveSystem();
            _attackSystem = new AttackSystem(tickRate);
            _projectileSystem = new ProjectileSystem(tickRate);
            _damageSystem = new DamageSystem();
            _enemySystem = new EnemySystem();
            _heavySystem = new HeavySystem(tickRate);
            _doorSystem = new DoorSystem();
        }

        private float _deltaTime;

        public void BuildScenario(ref Snapshot snapshot, MapDefinition mapDefinition)
        {
            // build world
            _map = mapDefinition;

            // First Player
            _currentPlayer = GetCharacter(PlayerType.Heavy);
            _currentPlayer._position = mapDefinition._spawnPoints[0];

            snapshot._players.Spawn(_currentPlayer);

            // Generate Doors
            var doorList = mapDefinition._woodenDoors.ToList();

            while (doorList.Count > 0)
            {
                var head = LookupDoor(doorList[0], doorList);

                doorList.RemoveAll(head.Contains);

                var door = new Door()
                {
                    _direction = head[0].x == head[^1].x ? Direction.Down : Direction.Right,
                    _length = head.Count,
                    _position = head[0]
                };

                snapshot._doors.Spawn(door);
            }
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

        private List<Vector2Int> LookupDoor(Vector2Int start, List<Vector2Int> doorPositions)
        {
            var head = new List<Vector2Int> { start };

            return GetNext(start, doorPositions, head);
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

                moveEvent._movement = intents[i].movement;
                moveEvent._angle = intents[i].angle;
                moveEvent._primary = intents[i].primary;
                moveEvent._secondary = intents[i].secondary;
                moveEvent._action = intents[i].action;
            }
        }

        public void DoStep(ReadOnlySpan<Intent> intents, int tick)
        {
            BuildSensors();
            Decide(intents);
            System(tick);
            Simulate();

            ResetEventLists();
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

            for (var i = 0; i < _numBlastWaveEvents; i++)
            {
                _blastWaveEvents[i] = default;
            }

            for (var i = 0; i < _numShootEvents; i++)
            {
                _shootEvents[i] = default;
            }

            _numShootEvents = 0;
            _numSoundEvents = 0;
            _numBlastWaveEvents = 0;
        }

        public void BuildSensors()
        {
        }
        
        public void System(int tick)
        {
            // _moveSystem.Execute(this);
            _attackSystem.Execute(this, tick);
            _heavySystem.Execute(this, tick);
            _projectileSystem.Execute(this, tick);
            _damageSystem.Execute(this);
            _enemySystem.Execute(this);
            _doorSystem.Execute(this);
        }

        private float GetMoveSpeed(PlayerType type)
        {
            return type switch
            {
                PlayerType.Heavy => Constants.HeavyMoveSpeed,
                PlayerType.Hacker => Constants.HackerMoveSpeed,
                PlayerType.Demolition => 1.5f,
                PlayerType.Stealth => 1.0f,
                _ => 0.0f
            };
        }

        private float GetMoveSpeed(EnemyType type)
        {
            return type switch
            {
                EnemyType.Gunman => Constants.GunmanMoveSpeed,
                _ => 0.0f
            };
        }

        public void Simulate()
        {
            for (var i = 0; i < _snapshot._players.GetNumEntities(); i++)
            {
                ref var player = ref _snapshot._players.GetEntityAt(i);

                player._position += (player._velocity * (_deltaTime * GetMoveSpeed(player._type)));
                player._angle = _moveEvents[i]._angle;
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
            if (snapshot._players.GetNumEntities() < snapshot._players.GetMaxLength())
            {
                var currentPlayer = (((int)_currentPlayer._type) + 1) % 4;

                if (snapshot._players.IsAlive((short)currentPlayer))
                {
                    _currentPlayer = GetCharacter((PlayerType)(currentPlayer));
                    _currentPlayer._position = _map._spawnPoints[currentPlayer];
                    snapshot._players.Spawn(_currentPlayer);
                }

                _currentPlayer = snapshot._players.GetEntityAt(currentPlayer);
            }
        }
    }
}
