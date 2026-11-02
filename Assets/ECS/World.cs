using System;
using ECS.Entities;
using ECS.Events;
using ECS.Systems;

namespace ECS
{
    public class World
    {
        private Snapshot _snapshot;

        public MoveEvent[] _moveEvents;

        private InputSystem _moveSystem;

        public World()
        {
            _snapshot = new Snapshot();
            _snapshot.Initialize();
            
            _moveEvents = new MoveEvent[_snapshot.players.Length];

            _moveSystem = new InputSystem();
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

        public void DoStep(ReadOnlySpan<Intent> intents, long tick, float deltaTime)
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

        public void System(ReadOnlySpan<Intent> intents, long tick, float deltaTime)
        {
            _moveSystem.Execute(this, intents);

            // _debugSpawnSystem.Execute(this, intent);
            // _debugDespawnSystem.Execute(this, intent);
        }

        public void Simulate(float deltaTime)
        {
            if (_snapshot.numPlayers > 0)
            {
                for (var i = 0; i < _snapshot.numPlayers; i++)
                {
                    ref var player = ref _snapshot.players[i];
                    
                    player.position += _moveEvents[i].movement * (deltaTime * 2.5f);
                    player.yaw = _moveEvents[i].yaw;
                }
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
    }
}
