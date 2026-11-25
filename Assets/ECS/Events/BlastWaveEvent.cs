using UnityEngine;

namespace ECS.Events
{
    public struct BlastWaveEvent
    {
        public int _damage;
        public Vector2 _position;
        public float _radius;
        public bool _ignoreHeavy;
    }
}
