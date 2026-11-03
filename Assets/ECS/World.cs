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

        public World(int tickRate)
        {
            _snapshot = new Snapshot();
            _snapshot.Initialize();

            _moveEvents = new MoveEvent[_snapshot.players.Length];

            _moveSystem = new MoveSystem();
            _attackSystem = new AttackSystem(tickRate);
            _projectileSystem = new ProjectileSystem(tickRate);
        }

        public void BuildScenario(ref Snapshot snapshot)
        {
            // build world

            var playerId = FetchPlayerId(ref snapshot);

            var player = new Player(playerId);
            // add position

            SpawnPlayer(player, ref snapshot);

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
                _moveEvents[i] = new MoveEvent();
            }
        }

        public void BuildSensors()
        {
        }

        public void System(ReadOnlySpan<Intent> intents, int tick, float deltaTime)
        {
            _moveSystem.Execute(this, intents);

            _attackSystem.Execute(this, intents, tick);
            
            _projectileSystem.Execute(this, intents, tick);
            
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
        }

        public Snapshot GetSnapshot()
        {
            return _snapshot;
        }

        public void SpawnPlayer(Player player, ref Snapshot snapshot)
        {
            snapshot.players[snapshot.numPlayers] = player;
            snapshot.playerIdMap[player._id - 1] = snapshot.numPlayers;
            snapshot.numPlayers++;
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
                var movedPlayer = _snapshot.projectiles[index];
                _snapshot.projectileIdMap[movedPlayer._id - 1] = index;
            }

            _snapshot.playerIdMap[projectileId - 1] = -1;
            _snapshot.freeProjectileIds[^_snapshot.numProjectiles] = projectileId;
            _snapshot.players[_snapshot.numProjectiles - 1] = default;
            _snapshot.numProjectiles--;
        }
    }
}
