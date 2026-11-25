using UnityEngine;
using Door = ECS.Entities.Door;

namespace ECS.Notification
{
    public struct DoorDestructionNotification
    {
        public Door _door;
        public Vector2 _direction;
        public int _player;
    }
}
