using System;
using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Systems;
using UnityEngine;

namespace ECS
{
    public class World
    {
        private Snapshot _snapshot;

        public MoveEvent[] _moveEvents;

        private MoveSystem _moveSystem;
        private AttackSystem _attackSystem;
        private ProjectileSystem _projectileSystem;
        private EnemySystem _enemySystem;

        private SoundEvent[] _soundEvents;
        private int _numSoundEvents;

        public void AddSoundEvent(SoundEvent soundEvent)
        {
            if (_numSoundEvents == _soundEvents.Length)
                return;

            _soundEvents[_numSoundEvents] = soundEvent;
            _numSoundEvents++;
        }

        public World(int tickRate)
        {
            _snapshot = new Snapshot();
            _snapshot.Initialize();

            _moveEvents = new MoveEvent[_snapshot.players.Length];
            _soundEvents = new SoundEvent[64];

            _moveSystem = new MoveSystem();
            _attackSystem = new AttackSystem(tickRate);
            _projectileSystem = new ProjectileSystem();
            _enemySystem = new EnemySystem();
        }

        public void BuildScenario(ref Snapshot snapshot)
        {
            // build world

            var playerId = FetchPlayerId(ref snapshot);

            var player = new Player(playerId);
            // add position

            SpawnPlayer(player, ref snapshot);

            // DEBUG
#if UNITY_EDITOR

            var enemyId = FetchEnemyId(ref snapshot);

            var enemy = new Enemy(enemyId)
            {
                _health = 100, _angle = 0, _position = new Vector2(3, 0), _type = EnemyType.Gunman
            };

            SpawnEnemy(enemy, ref snapshot);
#endif

            // Spawn Enemies and stuff
        }

        public void SetState(Snapshot snapshot)
        {
            _snapshot.BuildFrom(snapshot);
        }

        public void DoStep(ReadOnlySpan<Intent> intents, int tick, float deltaTime)
        {
            BuildSensors();
            System(intents, tick, deltaTime);
            Simulate(deltaTime);

            ResetEventLists();
        }

        private void ResetEventLists()
        {
            for (var i = 0; i < _snapshot.players.Length; i++)
            {
                _moveEvents[i] = default;
            }

            for (var i = 0; i < _numSoundEvents; i++)
            {
                _soundEvents[i] = default;
            }

            _numSoundEvents = 0;
        }

        public void BuildSensors()
        {
        }

        public void System(ReadOnlySpan<Intent> intents, int tick, float deltaTime)
        {
            _moveSystem.Execute(this, intents);

            _attackSystem.Execute(this, intents, tick);

            _projectileSystem.Execute(this, intents, tick);

            _enemySystem.Execute(this, intents);

            // _debugSpawnSystem.Execute(this, intent);
            // _debugDespawnSystem.Execute(this, intent);
        }

        private float GetMoveSpeed(PlayerType type)
        {
            return type switch
            {
                PlayerType.Heavy => Constants.HeavyMoveSpeed,
                PlayerType.Hacker => 2.0f,
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

        public void Simulate(float deltaTime)
        {
            for (var i = 0; i < _snapshot.numPlayers; i++)
            {
                ref var player = ref _snapshot.players[i];

                player.position += _moveEvents[i]._movement * (deltaTime * GetMoveSpeed(player.type));
                player.angle = _moveEvents[i]._angle;
            }

            for (var i = 0; i < _snapshot.numProjectiles; i++)
            {
                ref var projectile = ref _snapshot.projectiles[i];

                var radians = projectile._angle * Mathf.Deg2Rad;

                var direction = new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

                var movement = direction * (Constants.HeavyProjectileSpeed * deltaTime);

                projectile._position += movement;
            }

            for (var i = 0; i < _snapshot.numEnemies; i++)
            {
                ref var enemy = ref _snapshot.enemies[i];

                enemy._position += enemy._linearVelocity * (GetMoveSpeed(enemy._type) * deltaTime);
            }
        }

        public ref Snapshot GetSnapshot()
        {
            return ref _snapshot;
        }

        public void SpawnPlayer(Player player, ref Snapshot snapshot)
        {
            snapshot.players[snapshot.numPlayers] = player;
            snapshot.playerIdMap[player._id - 1] = snapshot.numPlayers;
            snapshot.numPlayers++;
        }

        public void SpawnEnemy(Enemy enemy, ref Snapshot snapshot)
        {
            snapshot.enemies[snapshot.numEnemies] = enemy;
            snapshot.enemyIdMap[enemy._id - 1] = snapshot.numEnemies;
            snapshot.numEnemies++;
        }

        public void SpawnEnemy(Enemy enemy)
        {
            _snapshot.enemies[_snapshot.numEnemies] = enemy;
            _snapshot.enemyIdMap[enemy._id - 1] = _snapshot.numEnemies;
            _snapshot.numEnemies++;
        }

        public void SpawnPlayer(Player player)
        {
            _snapshot.players[_snapshot.numPlayers] = player;
            _snapshot.playerIdMap[player._id - 1] = _snapshot.numPlayers;
            _snapshot.numPlayers++;
        }

        public void DespawnPlayer(short playerId)
        {
            if (_snapshot.playerIdMap[playerId - 1] == -1)
                return;

            var index = _snapshot.playerIdMap[playerId - 1];
            if (index != _snapshot.numPlayers - 1)
            {
                _snapshot.players[index] = _snapshot.players[_snapshot.numPlayers - 1];
                var movedPlayer = _snapshot.players[index];
                _snapshot.playerIdMap[movedPlayer._id - 1] = index;
            }

            _snapshot.playerIdMap[playerId - 1] = -1;
            _snapshot.freePlayerIds[^_snapshot.numPlayers] = playerId;
            _snapshot.players[_snapshot.numPlayers - 1] = default;
            _snapshot.numPlayers--;
        }

        public short FetchPlayerId(ref Snapshot snapshot)
        {
            if (snapshot.numPlayers == snapshot.players.Length)
                return 0;

            var id = snapshot.freePlayerIds[^(snapshot.numPlayers + 1)];
            snapshot.freePlayerIds[^(snapshot.numPlayers + 1)] = 0;

            return id;
        }

        public short FetchEnemyId(ref Snapshot snapshot)
        {
            if (snapshot.numEnemies == snapshot.enemies.Length)
                return 0;

            var id = snapshot.freeEnemyIds[^(snapshot.numEnemies + 1)];
            snapshot.freeEnemyIds[^(snapshot.numEnemies + 1)] = 0;

            return id;
        }

        public short FetchPlayerId()
        {
            if (_snapshot.numPlayers == _snapshot.players.Length)
                return 0;

            var id = _snapshot.freePlayerIds[^(_snapshot.numPlayers + 1)];
            _snapshot.freePlayerIds[^(_snapshot.numPlayers + 1)] = 0;

            return id;
        }

        public void ReturnPlayerId(short id)
        {
            _snapshot.freePlayerIds[^(_snapshot.numPlayers + 1)] = id;
        }

        public void SetNextPlayer(ref Snapshot snapshot)
        {
            if (snapshot.numPlayers < snapshot.players.Length)
            {
                var id = FetchPlayerId(ref snapshot);
                var player = new Player(id);
                SpawnPlayer(player, ref snapshot);
            }
        }

        public short FetchProjectileId()
        {
            if (_snapshot.numProjectiles == _snapshot.projectiles.Length)
                return 0;

            var id = _snapshot.freeProjectileIds[^(_snapshot.numProjectiles + 1)];
            _snapshot.freeProjectileIds[^(_snapshot.numProjectiles + 1)] = 0;

            return id;
        }

        public void SpawnProjectile(Projectile projectile)
        {
            _snapshot.projectiles[_snapshot.numProjectiles] = projectile;
            _snapshot.projectileIdMap[projectile._id - 1] = _snapshot.numProjectiles;
            _snapshot.numProjectiles++;
        }

        public void DespawnProjectile(short projectileId)
        {
            if (_snapshot.projectileIdMap[projectileId - 1] == -1)
                return;

            var index = _snapshot.projectileIdMap[projectileId - 1];
            if (index != _snapshot.numProjectiles - 1)
            {
                _snapshot.projectiles[index] = _snapshot.projectiles[_snapshot.numProjectiles - 1];
                var movedProjectile = _snapshot.projectiles[index];
                _snapshot.projectileIdMap[movedProjectile._id - 1] = index;
            }

            _snapshot.projectileIdMap[projectileId - 1] = -1;
            _snapshot.freeProjectileIds[^_snapshot.numProjectiles] = projectileId;
            _snapshot.projectiles[_snapshot.numProjectiles - 1] = default;
            _snapshot.numProjectiles--;
        }

        public void DespawnEnemy(short enemyId)
        {
            if (_snapshot.enemyIdMap[enemyId - 1] == -1)
                return;

            var index = _snapshot.enemyIdMap[enemyId - 1];
            if (index != _snapshot.numEnemies - 1)
            {
                _snapshot.enemies[index] = _snapshot.enemies[_snapshot.numEnemies - 1];
                var movedEnemy = _snapshot.enemies[index];
                _snapshot.enemyIdMap[movedEnemy._id - 1] = index;
            }

            _snapshot.enemyIdMap[enemyId - 1] = -1;
            _snapshot.freeEnemyIds[^_snapshot.numEnemies] = enemyId;
            _snapshot.enemies[_snapshot.numEnemies - 1] = default;
            _snapshot.numEnemies--;
        }
    }
}
