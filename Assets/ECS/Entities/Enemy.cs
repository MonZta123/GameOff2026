using UnityEngine;

namespace ECS.Entities
{
    public struct Enemy
    {
        public short id;
        public Vector3 position;
        public float angle;
        public float lastAttackAt;
        public EnemyType type;
        public int health;
    }
}
