using ECS.Entities;
using ECS.Events;

namespace ECS.Systems
{
    public class ProjectileSystem
    {
        public void Execute(World world, int tick)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = world._numShootEvents - 1; i >= 0; i--)
            {
                var shootEvent = world._shootEvents[i];

                snapshot._projectiles.Spawn(new Projectile()
                {
                    _type = shootEvent._type,
                    _position = shootEvent._position,
                    _angle = shootEvent._angle,
                    _spawnTick = tick,
                    _ttl = shootEvent._ttl
                });
            }

            for (var i = snapshot._projectiles.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var projectile = ref snapshot._projectiles.GetEntityAt(i);

                if (projectile._spawnTick + projectile._ttl <= tick)
                {
                    if (projectile._type == ProjectileType.Demolition)
                    {
                        world.AddGranadeExplosionEvent(new BlastWaveEvent()
                        {
                            _position = projectile._position, _radius = 3
                        });
                    }

                    snapshot._projectiles.DespawnAt(i);
                    continue;
                }

                var deleted = false;
                for (var j = snapshot._doors.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var door = ref snapshot._doors.GetEntityAt(j);

                    if (projectile._type == ProjectileType.Demolition &&
                        projectile._position.x >= door._leftBottom.x && projectile._position.x <= door._rightTop.x &&
                        projectile._position.y >= door._leftBottom.y && projectile._position.y <= door._rightTop.y)
                    {
                        world.AddGranadeExplosionEvent(new BlastWaveEvent()
                        {
                            _position = projectile._position, _radius = 3
                        });
                        
                        snapshot._projectiles.DespawnAt(i);
                        
                        deleted = true;
                        break;
                    }
                }

                if (deleted)
                    continue;

                if (projectile._position.x < 0 || projectile._position.x > world._map._width - 1 ||
                    projectile._position.y < 0 || projectile._position.y > world._map._height - 1 ||
                    world._map._map[(int)projectile._position.y * world._map._width + (int)projectile._position.x] == 0)
                {
                    if (projectile._type == ProjectileType.Demolition)
                    {
                        world.AddGranadeExplosionEvent(new BlastWaveEvent()
                        {
                            _position = projectile._position, _radius = 3
                        });
                    }

                    snapshot._projectiles.DespawnAt(i);
                }
            }
        }
    }
}
