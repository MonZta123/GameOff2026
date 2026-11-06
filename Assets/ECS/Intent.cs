using UnityEngine;

namespace ECS
{
    public struct Intent
    {
        public Vector2 movement;
        public float angle;
        public bool primary;
        public bool secondary;
        public bool action;
    }
}
