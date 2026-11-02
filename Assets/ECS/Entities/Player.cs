using UnityEngine;

namespace ECS.Entities
{
    public struct Player
    {
        public Player(short id)
        {
            _id = id;
            position = Vector2.zero;
            yaw = 0;
            health = 100;
            type = PlayerType.Heavy;
        }
        
        public Vector2 position;
        public float yaw;
        public float health;
        public PlayerType type;
        
        public short _id;
    }
}
