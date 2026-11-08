using Domain;
using ECS.Entities;
using UnityEngine;

namespace ECS.Systems
{
    public class DamageSystem
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

        private bool DoesDamageToEnemy(Projectile projectile)
        {
            return projectile._type switch
            {
                ProjectileType.Heavy => true,
                _ => false
            };
        }

        private bool DoesDamageToPlayer(Projectile projectile, PlayerType type)
        {
            return projectile._type switch
            {
                ProjectileType.GunMan => true,
                ProjectileType.Heavy => type != PlayerType.Heavy,
                ProjectileType.Hacker => type != PlayerType.Hacker,
                _ => false
            };
        }

        public void Execute(World world)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = snapshot._enemies.GetNumEntities() -1; i >= 0; i--)
            {
                ref var enemy = ref snapshot._enemies.GetEntityAt(i);

                for (var j = snapshot._projectiles.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var projectile = ref snapshot._projectiles.GetEntityAt(j);

                    if (!DoesDamageToEnemy(projectile))
                        continue;

                    if (Vector2.Distance(enemy._position, projectile._position) < Constants.GunManColliderRadius)
                    {
                        enemy._health -= GetProjectileDamage(projectile);

                        snapshot._projectiles.DespawnAt(j); 

                        if (enemy._health <= 0)
                        {
                            enemy._health = 0;
                            snapshot._enemies.DespawnAt(i);
                            break;
                        }
                    }
                }
            }

            for (var i = snapshot._players.GetNumEntities() - 1; i >= 0 ; i--)
            {
                ref var player = ref snapshot._players.GetEntityAt(i);

                for (var j = snapshot._projectiles.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var projectile = ref snapshot._projectiles.GetEntityAt(j);

                    if (!DoesDamageToPlayer(projectile, player._type))
                        continue;

                    if (Vector2.Distance(player._position, projectile._position) < player._colliderRadius)
                    {
                        player._health -= GetProjectileDamage(projectile);

                        snapshot._projectiles.DespawnAt(j);
                        
                        if (player._health <= 0)
                        {
                            player._health = 0;
                           
                            break;
                        }
                    }
                }
            }
        }
    }
}
