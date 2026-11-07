using Actors;
using Domain;
using UnityEngine;

public class GameObjectPools : MonoBehaviour
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

    public GameObjectPool<Enemy, ECS.Entities.Enemy> _enemyPool;
    public GameObjectPool<Projectile, ECS.Entities.Projectile> _projectilePool;
    public GameObjectPool<Player, ECS.Entities.Player> _playerPool;
    public GameObjectPool<Door, ECS.Entities.Door> _doorPool;
    public GameObjectPool<Trigger, ECS.Entities.Trigger> _triggerPool;
    
    public void Initialize()
    {
        _enemyPool = new GameObjectPool<Enemy, ECS.Entities.Enemy>(Constants.EnemyCount);
        _projectilePool = new GameObjectPool<Projectile, ECS.Entities.Projectile>(Constants.ProjectileCount);
        _playerPool = new GameObjectPool<Player, ECS.Entities.Player>(Constants.PlayerCount);
        _doorPool = new GameObjectPool<Door, ECS.Entities.Door>(Constants.DoorCount);
        _triggerPool = new GameObjectPool<Trigger, ECS.Entities.Trigger>(Constants.TriggerCount);

        for (var i = 0; i < Constants.PlayerCount; i++)
            _playerPool.Add(Instantiate(player));

        for (var i = 0; i < Constants.EnemyCount; i++)
            _enemyPool.Add(Instantiate(enemy));

        for (var i = 0; i < Constants.ProjectileCount; i++)
            _projectilePool.Add(Instantiate(projectile));

        for (var i = 0; i < Constants.DoorCount; i++)
            _doorPool.Add(Instantiate(door));

        for (var i = 0; i < Constants.TriggerCount; i++)
            _triggerPool.Add(Instantiate(trigger));
    }
}
