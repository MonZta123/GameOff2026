using UnityEngine;
using ECS.Entities;

namespace ECS.Events
{
    public struct DoorDestructionNotification
    {
        public Door _door;
        public Vector2 _direction;
    }
}
