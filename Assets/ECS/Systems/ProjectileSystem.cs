using System;

namespace ECS.Systems
{
    public class ProjectileSystem
    {
        public void Execute(World world, ReadOnlySpan<Intent> intents, int tick)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = 0; i < snapshot.numProjectiles; i++)
            {
                ref var projectile = ref snapshot.projectiles[i];

                if (projectile._spawnTick + projectile._ttl <= tick)
                {
                    world.DespawnProjectile(projectile._id);
                    i--;
                }
            }
        }
    }
}
