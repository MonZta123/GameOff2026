using UnityEngine;

namespace ECS
{
    public struct Intent
    {
        public Vector2 movement;
        public float yaw;
        public bool timeShift { get; set; }
    }
}
