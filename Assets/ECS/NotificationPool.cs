namespace ECS
{
    public struct NotificationPool<Type>
    {
        private readonly Type[] _notifications;
        private short _numNotifications;

        public NotificationPool(int notificationCount)
        {
            _notifications = new Type[notificationCount];
            _numNotifications = 0;
        }

        public void Add(Type notification)
        {
            if (_numNotifications == _notifications.Length)
                return;

            _notifications[_numNotifications] = notification;

            _numNotifications++;
        }

        public void Clear()
        {
            for (var i = 0; i < _numNotifications; i++)
                _notifications[i] = default;
            
            _numNotifications = 0;
        }

        public void CopyTo(ref NotificationPool<Type> notificationPool)
        {
            _notifications.CopyTo(notificationPool._notifications, 0);
            notificationPool._numNotifications = _numNotifications;
        }

        public int GetMaxLength() => _notifications.Length;

        public ref Type GetEntityAt(int index) => ref _notifications[index];

        public int GetNumNotifications() => _numNotifications;
    }
}
