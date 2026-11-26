using Actors;
using Domain;
using ECS.Notification;
using Notifications;
using UnityEngine;
using DoorDestructionNotification = Notifications.DoorDestructionNotification;
using EnemyDiedNotification = Notifications.EnemyDiedNotification;
using PlacedBomb = Actors.PlacedBomb;
using SplosionNotification = Notifications.SplosionNotification;
using TriggerRemoveNotification = Notifications.TriggerRemoveNotification;

public class Pools : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private Enemy enemy;

    [SerializeField]
    private Projectile projectile;

    [SerializeField]
    private Door door;

    [SerializeField]
    private Trigger trigger;

    [SerializeField]
    private EnemyDiedNotification enemyDiedNotification;

    [SerializeField]
    private SnipeShotHitscan snipeShotHitscan;

    [SerializeField]
    private DoorDestructionNotification doorDestructionNotification;

    [SerializeField]
    private PlacedBomb placedBomb;

    [SerializeField]
    private SplosionNotification splosionNotification;
    
    [SerializeField]
    private TriggerRemoveNotification triggerRemoveNotification;

    public GameObjectPool<Enemy, ECS.Entities.Enemy> _enemyPool;
    public GameObjectPool<Projectile, ECS.Entities.Projectile> _projectilePool;
    public GameObjectPool<Player, ECS.Entities.Player> _playerPool;
    public GameObjectPool<Door, ECS.Entities.Door> _doorPool;
    public GameObjectPool<Trigger, ECS.Entities.Trigger> _triggerPool;

    public GameObjectPool<PlacedBomb, ECS.Entities.PlacedBomb>
        _placedBombs;

    public GameNotificationPool<EnemyDiedNotification, ECS.Notification.EnemyDiedNotification>
        _enemyDiedNotificationPool;

    public GameNotificationPool<SnipeShotHitscan, SnipeShotNotification> _snipeShotNotificationPool;

    public GameNotificationPool<DoorDestructionNotification, ECS.Notification.DoorDestructionNotification>
        _doorDestructionNotificationPool;

    public GameNotificationPool<SplosionNotification, ECS.Notification.SplosionNotification>
        _splosionNotificationPool;

    public GameNotificationPool<TriggerRemoveNotification, ECS.Notification.TriggerRemoveNotification>
        _triggerRemoveNotificationPool;

    public void ClearAllNotificationPools()
    {
        _enemyDiedNotificationPool.ReturnAll();
        _snipeShotNotificationPool.ReturnAll();
        _doorDestructionNotificationPool.ReturnAll();
        _splosionNotificationPool.ReturnAll();
        _triggerRemoveNotificationPool.ReturnAll();
    }

    public void ClearAllObjectPools()
    {
        _enemyPool.ReturnAll();
        _projectilePool.ReturnAll();
        _playerPool.ReturnAll();
        _doorPool.ReturnAll();
        _triggerPool.ReturnAll();
        _placedBombs.ReturnAll();
    }

    public void Initialize()
    {
        _enemyPool = new GameObjectPool<Enemy, ECS.Entities.Enemy>(Constants.EnemyCount);
        _projectilePool = new GameObjectPool<Projectile, ECS.Entities.Projectile>(Constants.ProjectileCount);
        _playerPool = new GameObjectPool<Player, ECS.Entities.Player>(Constants.PlayerCount);
        _doorPool = new GameObjectPool<Door, ECS.Entities.Door>(Constants.DoorCount);
        _triggerPool = new GameObjectPool<Trigger, ECS.Entities.Trigger>(Constants.TriggerCount);
        _placedBombs = new GameObjectPool<PlacedBomb, ECS.Entities.PlacedBomb>(Constants.PlacedBombCount);

        _snipeShotNotificationPool =
            new GameNotificationPool<SnipeShotHitscan, SnipeShotNotification>(Constants.SnipeShotNotificationCount);

        _enemyDiedNotificationPool =
            new GameNotificationPool<EnemyDiedNotification, ECS.Notification.EnemyDiedNotification>(Constants
                .EnemyDiedNotificationCount);

        _doorDestructionNotificationPool =
            new GameNotificationPool<DoorDestructionNotification, ECS.Notification.DoorDestructionNotification>(
                Constants
                    .DoorDestructionNotificationCount);

        _splosionNotificationPool =
            new GameNotificationPool<SplosionNotification, ECS.Notification.SplosionNotification>(Constants
                .SplosionNotificationCount);

        _triggerRemoveNotificationPool =
            new GameNotificationPool<TriggerRemoveNotification, ECS.Notification.TriggerRemoveNotification>(
                Constants
                    .TriggerRemoveNotificationCount);

        for (var i = 0; i < Constants.PlayerCount; i++)
            _playerPool.Add(Instantiate(player));

        for (var i = 0; i < Constants.EnemyCount; i++)
            _enemyPool.Add(Instantiate(enemy));

        for (var i = 0; i < Constants.ProjectileCount; i++)
            _projectilePool.Add(Instantiate(projectile));

        for (var i = 0; i < Constants.DoorCount; i++)
            _doorPool.Add(Instantiate(door));

        for (var i = 0; i < Constants.PlacedBombCount; i++)
            _placedBombs.Add(Instantiate(placedBomb));

        for (var i = 0; i < Constants.TriggerCount; i++)
            _triggerPool.Add(Instantiate(trigger));

        for (var i = 0; i < Constants.EnemyDiedNotificationCount; i++)
            _enemyDiedNotificationPool.Add(Instantiate(enemyDiedNotification));

        for (var i = 0; i < Constants.SnipeShotNotificationCount; i++)
            _snipeShotNotificationPool.Add(Instantiate(snipeShotHitscan));

        for (var i = 0; i < Constants.DoorDestructionNotificationCount; i++)
            _doorDestructionNotificationPool.Add(Instantiate(doorDestructionNotification));

        for (var i = 0; i < Constants.SplosionNotificationCount; i++)
            _splosionNotificationPool.Add(Instantiate(splosionNotification));
        
        for (var i = 0; i < Constants.TriggerRemoveNotificationCount; i++)
            _triggerRemoveNotificationPool.Add(Instantiate(triggerRemoveNotification));
    }
}
