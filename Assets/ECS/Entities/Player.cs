using UnityEngine;

namespace ECS.Entities
{
    public struct Player
    {
        public Player(short id)
        {
            _id = id;
            position = Vector2.zero;
            angle = 0;
            health = 100;
            type = PlayerType.Heavy;
            lastAttackTick = 0;
        }
        
        public Vector2 position;
        public float angle;
        public float health;
        public PlayerType type;
        public int lastAttackTick;
        
        public short _id;
    }
}
