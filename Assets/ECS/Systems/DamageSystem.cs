using System;
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

        public void Execute(World world, ReadOnlySpan<Intent> intents)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = 0; i < snapshot.numEnemies; i++)
            {
                ref var enemy = ref snapshot.enemies[i];

                for (var j = 0; j < snapshot.numProjectiles; j++)
                {
                    ref var projectile = ref snapshot.projectiles[j];

                    if (!DoesDamageToEnemy(projectile))
                        continue;

                    if (Vector2.Distance(enemy._position, projectile._position) < Constants.GunManColliderRadius)
                    {
                        enemy._health -= GetProjectileDamage(projectile);

                        world.DespawnProjectile(projectile._id);
                        j--;

                        if (enemy._health <= 0)
                        {
                            enemy._health = 0;
                            world.DespawnEnemy(enemy._id);
                            i--;
                            break;
                        }
                    }
                }
            }

            for (var i = 0; i < snapshot.numPlayers; i++)
            {
                ref var player = ref snapshot.players[i];

                for (var j = 0; j < snapshot.numProjectiles; j++)
                {
                    ref var projectile = ref snapshot.projectiles[j];

                    if (!DoesDamageToPlayer(projectile, player._type))
                        continue;

                    if (Vector2.Distance(player._position, projectile._position) < player._colliderRadius)
                    {
                        player._health -= GetProjectileDamage(projectile);

                        world.DespawnProjectile(projectile._id);
                        j--;

                        if (player._health <= 0)
                        {
                            player._health = 0;
                            // world.DespawnPlayer(player._id);
                            i--;
                            break;
                        }
                    }
                }
            }
        }
    }
}
