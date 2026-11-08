using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Notification;

namespace ECS
{
    public struct Snapshot
    {
        public EntityPool<Player> _players;
        public EntityPool<Enemy> _enemies;
        public EntityPool<Projectile> _projectiles;
        public EntityPool<Door> _doors;
        public EntityPool<Trigger> _triggers;

        public int _damageProcess;
        public int _hackProcess;
        public int _bombsLeft;

        public NotificationPool<DoorDestructionNotification> _doorDestructionNotificationPool;
        public NotificationPool<SoundNotification> _soundNotificationPool;
        public NotificationPool<EnemyDiedNotification> _enemyDiedNotificationPool;

        public void BuildFrom(Snapshot snapshot)
        {
            snapshot._players.CopyTo(ref _players);
            snapshot._enemies.CopyTo(ref _enemies);
            snapshot._projectiles.CopyTo(ref _projectiles);
            snapshot._doors.CopyTo(ref _doors);
            snapshot._triggers.CopyTo(ref _triggers);

            _damageProcess = snapshot._damageProcess;
            _hackProcess = snapshot._hackProcess;
            _bombsLeft = snapshot._bombsLeft;
        }

        public void CopyNotifications(Snapshot snapshot)
        {
            snapshot._doorDestructionNotificationPool.CopyTo(ref _doorDestructionNotificationPool);
            snapshot._soundNotificationPool.CopyTo(ref _soundNotificationPool);
            snapshot._enemyDiedNotificationPool.CopyTo(ref _enemyDiedNotificationPool);
        }

        public void Initialize()
        {
            _players = new EntityPool<Player>(Constants.PlayerCount);
            _enemies = new EntityPool<Enemy>(Constants.EnemyCount);
            _projectiles = new EntityPool<Projectile>(Constants.ProjectileCount);
            _doors = new EntityPool<Door>(Constants.DoorCount);
            _triggers = new EntityPool<Trigger>(Constants.TriggerCount);

            _doorDestructionNotificationPool = new NotificationPool<DoorDestructionNotification>(8);
            _soundNotificationPool = new NotificationPool<SoundNotification>(8);
            _enemyDiedNotificationPool =
                new NotificationPool<EnemyDiedNotification>(Constants.EnemyDiedNotificationCount);
        }
    }
}
