using Actors;
using ECS;
using UnityEngine;

public class SnapshotRenderer : MonoBehaviour
{
    [SerializeField]
    private GameObjectPools gameObjectPools;

    public void SetSnapshot(Snapshot snapshot)
    {
        UpdateView(snapshot._players, gameObjectPools._playerPool);
        UpdateView(snapshot._enemies, gameObjectPools._enemyPool);
        UpdateView(snapshot._projectiles, gameObjectPools._projectilePool);
        UpdateView(snapshot._doors, gameObjectPools._doorPool);
        UpdateView(snapshot._triggers, gameObjectPools._triggerPool);
    }

    private void UpdateView<TView, TEntity>(EntityPool<TEntity> ecsPool, GameObjectPool<TView, TEntity> pool)
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

            obj.SetState(currentEntity);
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
