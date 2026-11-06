using Domain;
using ECS.Entities;

namespace ECS
{
    public struct Snapshot
    {
        public EntityPool<Player> _players;
        public EntityPool<Enemy> _enemies;
        public EntityPool<Projectile> _projectiles;
        public EntityPool<Door> _doors;
        public EntityPool<Trigger> _triggers;

        public void BuildFrom(Snapshot snapshot)
        {
            snapshot._players.CopyTo(ref _players);
            snapshot._enemies.CopyTo(ref _enemies);
            snapshot._projectiles.CopyTo(ref _projectiles);
            snapshot._doors.CopyTo(ref _doors);
            snapshot._triggers.CopyTo(ref _triggers);
        }

        public void Initialize()
        {
            _players = new EntityPool<Player>(Constants.PlayerCount);
            _enemies = new EntityPool<Enemy>(Constants.EnemyCount);
            _projectiles = new EntityPool<Projectile>(Constants.ProjectileCount);
            _doors = new EntityPool<Door>(Constants.DoorCount);
            _triggers = new EntityPool<Trigger>(Constants.TriggerCount);
        }
    }
}
