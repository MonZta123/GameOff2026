using UnityEngine;

namespace ECS.Notification
{
    public enum SoundType
    {
        Dash,
        Slam,
        
        HackerDodge,
        
        DoorDestruction,
        
        MineExplosion,
        Rewind,
        MineDefuse,
        
        CashIn
    }
    
    public struct SoundNotification
    {
        public SoundType _soundType;
        public Vector2 _position;
    }
}
