using UnityEngine;

namespace ECS.Notification
{
    public enum SoundType
    {
        Dash,
        Slam,
        
        DoorDestruction
    }
    
    public struct SoundNotification
    {
        public SoundType _soundType;
        public string _switchGroup;
        public Vector2 _position;
    }
}
