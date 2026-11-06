using UnityEngine;

namespace ECS.Entities
{
    public struct Player
    {
        public PlayerType _type;
        
        public Vector2 _position;
        public Vector2 _velocity;
        public float _angle;
        public float _health;

        public float _colliderRadius;

        public int _primaryReadyAt;
        public int _secondaryReadyAt;
        public int _secondaryActivatedTo;
    }

    public struct Door
    { 
        public Direction _direction;
        public Vector2Int _position;
        public int _length;
    }

    public enum TriggerType
    {
        Mine,
        Computer,
        DemolitionDoor,
    }

    public struct Trigger
    {
        public float _radius;
        public int _ticksUntilFinished;
        public TriggerType _triggerType;
    }
}
