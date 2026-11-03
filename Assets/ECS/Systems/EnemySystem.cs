using System;
using Domain;
using ECS.Entities;
using UnityEngine;

namespace ECS.Systems
{
    public class EnemySystem
    {
        private float GetProjectileDamage(Projectile projectile)
        {
            switch (projectile._type)
            {
                case ProjectileType.Heavy:
                    return Constants.HeavyDamage;
                default:
                    return 0;
            }
        }
        
        public void Execute(World world, ReadOnlySpan<Intent> intents)
        {
            ref var snapshot = ref world.GetSnapshot();
            
            for (var i = 0; i < snapshot.numEnemies; i++)
            {
                ref var enemy = ref snapshot.enemies[i];

                for (var j = 0; j < snapshot.numProjectiles; j++)
                {
                    ref var projectile = ref snapshot.projectiles[j];

                    if (Vector2.Distance(enemy._position, projectile._position) < Constants.GunManColliderRadius)
                    {
                        var enemyId = enemy._id;
                        
                        enemy._health -= GetProjectileDamage(projectile);
                        
                        world.DespawnProjectile(projectile._id);
                        j--;
                        
                        if (enemy._health <= 0)
                        {
                            enemy._health = 0;
                            world.DespawnEnemy(enemyId);
                            i--;
                            break;
                        }
                    }
                }
            }
        }
    }
}
