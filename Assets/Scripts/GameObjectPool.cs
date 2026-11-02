using System.Collections.Generic;
using Actors;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private int maxPlayers = 4;
        
    [SerializeField]
    private Enemy enemy;

    [SerializeField]
    private int maxEnemies = 100;
        
    [SerializeField]
    private Projectile projectile;
        
    [SerializeField]
    private int maxProjectiles = 500;
        
    private readonly Stack<Player> _playerPool = new(); 
    private readonly Stack<Enemy> _enemyPool = new();
    private readonly Stack<Projectile> _projectilePool = new();
        
    public void Prewarm()
    {
        for (var i = 0; i < maxPlayers; i++)
        {
            var obj = Instantiate(player);
            obj.gameObject.SetActive(false);
            _playerPool.Push(obj);
        }
            
        for (var i = 0; i < maxEnemies; i++)
        {
            var obj = Instantiate(enemy);
            obj.gameObject.SetActive(false);
            _enemyPool.Push(obj);
        }
            
        for (var i = 0; i < maxProjectiles; i++)
        {
            var obj = Instantiate(projectile);
            obj.gameObject.SetActive(false);
            _projectilePool.Push(obj);
        }
    }

    public Player GetPlayer(ECS.Entities.Player entity)
    {
        var obj = _playerPool.Pop();
        obj.Init(entity);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnPlayer(Player obj)
    {
        obj.Release();
        obj.gameObject.SetActive(false);
        _playerPool.Push(obj);
    }
        
    public void ReturnEnemy(Enemy obj)
    {
        obj.Release();
        obj.gameObject.SetActive(false);
        _enemyPool.Push(obj);
    }
        
    public void ReturnProjectile(Projectile obj)
    {
        obj.Release();
        obj.gameObject.SetActive(false);
        _projectilePool.Push(obj);
    }
}
