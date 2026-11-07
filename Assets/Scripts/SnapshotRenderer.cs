using Actors;
using ECS;
using UnityEngine;

public class SnapshotRenderer : MonoBehaviour
{
    [SerializeField]
    private GameObjectPools gameObjectPools;

    public void SetSnapshot(Snapshot snapshot, int tick)
    {
        UpdateView(snapshot._players, gameObjectPools._playerPool, tick);
        UpdateView(snapshot._enemies, gameObjectPools._enemyPool, tick);
        UpdateView(snapshot._projectiles, gameObjectPools._projectilePool, tick);
        UpdateView(snapshot._doors, gameObjectPools._doorPool, tick);
        UpdateView(snapshot._triggers, gameObjectPools._triggerPool, tick);
    }

    private void UpdateView<TView, TEntity>(EntityPool<TEntity> ecsPool, GameObjectPool<TView, TEntity> pool, int tick)
        where TView : MonoBehaviour, IEntityView<TEntity>
    {
        for (var i = 0; i < ecsPool.GetNumEntities(); i++)
        {
            var currentEntity = ecsPool.GetEntityAt(i);
            var id = ecsPool.GetIdAt(i);

            var obj = pool.Active[id - 1];
            if (!obj)
            {
                obj = pool.Get(currentEntity);
                pool.Active[id - 1] = obj;
            }

            obj.SetState(currentEntity, tick);
        }

        for (var i = 0; i < pool.Active.Length; i++)
        {
            if (!pool.Active[i])
                continue;

            if (!ecsPool.IsAlive((short)(i + 1)))
            {
                pool.Return(pool.Active[i]);
                pool.Active[i] = null;
            }
        }
    }

    public Player GetPlayerByIndex(int activePlayerIndex)
    {
        return gameObjectPools._playerPool.Active[activePlayerIndex];
    }
}
