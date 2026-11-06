using UnityEngine;

namespace ECS.Events
{
    public struct MoveEvent
    {
        public Vector2 _movement;
        public float _angle;
        public bool _primary;
        public bool _secondary;
        public bool _action;
    }
}
