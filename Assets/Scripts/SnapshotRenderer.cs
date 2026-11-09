using Actors;
using ECS;
using Notifications;
using UnityEngine;

public class SnapshotRenderer : MonoBehaviour
{
    [SerializeField]
    private Pools pools;

    public void SetSnapshot(Snapshot snapshot, int tick)
    {
        UpdateView(snapshot._players, pools._playerPool, tick);
        UpdateView(snapshot._enemies, pools._enemyPool, tick);
        UpdateView(snapshot._projectiles, pools._projectilePool, tick);
        UpdateView(snapshot._doors, pools._doorPool, tick);
        UpdateView(snapshot._triggers, pools._triggerPool, tick);
    }

    public void RunNotifications(Snapshot snapshot)
    {
        RunNotification(snapshot._enemyDiedNotificationPool, pools._enemyDiedNotificationPool);
        RunNotification(snapshot._snipeShotNotificationPool, pools._snipeShotNotificationPool);
        RunNotification(snapshot._doorDestructionNotificationPool, pools._doorDestructionNotificationPool);
    }

    private void RunNotification<TView, TNotification>(
        NotificationPool<TNotification> ecsPool,
        GameNotificationPool<TView, TNotification> pool)
        where TView : NotificationView<TNotification>
    {
        for (var i = 0; i < ecsPool.GetNumNotifications(); i++)
        {
            var notification = ecsPool.GetEntityAt(i);

            pool.Notify(notification);
        }
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
        return pools._playerPool.Active[activePlayerIndex];
    }
}
