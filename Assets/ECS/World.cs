using ECS.Entities;
using ECS.Events;
using ECS.Systems;

namespace ECS
{
    public class World
    {
        private Snapshot _snapshot;

        public MoveEvent _moveEvent;

        private InputSystem _inputSystem;
        private DebugSpawnSystem _debugSpawnSystem;
        private DebugDespawnSystem _debugDespawnSystem;

        public World()
        {
            _snapshot = new Snapshot();
            _snapshot.Initialize();

            _inputSystem = new InputSystem();
            _debugSpawnSystem = new DebugSpawnSystem();
            _debugDespawnSystem = new DebugDespawnSystem();
        }

        public void SetState(Snapshot snapshot)
        {
            _snapshot.BuildFrom(snapshot);
        }

        public void DoStep(Intent intent, long tick, float deltaTime)
        {
            BuildSensors();
            System(intent, tick, deltaTime);
            Simulate(deltaTime);

            ResetEventLists();
        }

        private void ResetEventLists()
        {
            _moveEvent = new MoveEvent();
        }

        public void BuildSensors()
        {
        }

        public void System(Intent intent, long tick, float deltaTime)
        {
            _inputSystem.Execute(this, intent);

            _debugSpawnSystem.Execute(this, intent);
            _debugDespawnSystem.Execute(this, intent);
        }

        public void Simulate(float deltaTime)
        {
        }

        public Snapshot GetSnapshot()
        {
            return _snapshot;
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
    }
}
