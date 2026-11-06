namespace ECS
{
    public struct EntityPool<Type>
    {
        private readonly short[] _idToIndex;
        private readonly short[] _indexToId;
        private readonly short[] _freeIds;
        private readonly Type[] _entities;
        private short _numEntities;

        public EntityPool(int entityCount)
        {
            _idToIndex = new short[entityCount];
            _indexToId = new short[entityCount];
            _entities = new Type[entityCount];
            _freeIds = new short[entityCount];
            _numEntities = 0;

            for (var i = 0; i < entityCount; i++)
            {
                _freeIds[i] = (short)(entityCount - i);
                _idToIndex[i] = -1;
            }
        }

        public void CopyTo(ref EntityPool<Type> snapshotPool)
        {
            _idToIndex.CopyTo(snapshotPool._idToIndex, 0);
            _indexToId.CopyTo(snapshotPool._indexToId, 0);
            _entities.CopyTo(snapshotPool._entities, 0);
            _freeIds.CopyTo(snapshotPool._freeIds, 0);
            snapshotPool._numEntities = _numEntities;
        }

        private short FetchId()
        {
            if (_numEntities == _entities.Length)
                return 0;

            var id = _freeIds[^(_numEntities + 1)];
            _freeIds[^(_numEntities + 1)] = 0;

            return id;
        }

        public short Spawn(Type entity)
        {
            var id = FetchId();
            if (id == 0)
                return 0;

            _idToIndex[id - 1] = _numEntities;
            _entities[_numEntities] = entity;
            _indexToId[_numEntities] = id;

            _numEntities++;

            return id;
        }

        public void DespawnAt(int index) => Despawn(_indexToId[index]);

        public short GetIdAt(int index) => _indexToId[index];

        public int GetIndexAt(short id) => _idToIndex[id - 1];

        // public ref Type GetEntity(short id) => ref _entities[_idToIndex[id - 1]];

        public int GetMaxLength() => _entities.Length;

        public ref Type GetEntityAt(int index) => ref _entities[index];

        public int GetNumEntities() => _numEntities;
        
        public bool IsAlive(short id) => _idToIndex[id - 1] != -1;
        
        public bool IsAliveAt(int index) => _indexToId[index] != 0;
        
        public bool TryGetIndex(short id, out int index) => (index = _idToIndex[id - 1]) != -1;

        public void Despawn(short id)
        {
            if (id <= 0 || _idToIndex[id - 1] == -1)
                return;

            var index = _idToIndex[id - 1]; // position im indexArray holen
            if (index != _numEntities - 1)
            {
                _entities[index] = _entities[_numEntities - 1];
                _indexToId[index] = _indexToId[_numEntities - 1];

                _idToIndex[_indexToId[index] - 1] = index;
            }

            _idToIndex[id - 1] = -1;
            _freeIds[^_numEntities] = id;
            _indexToId[_numEntities - 1] = 0;
            _entities[_numEntities - 1] = default;
            _numEntities--;
        }
    }
}
