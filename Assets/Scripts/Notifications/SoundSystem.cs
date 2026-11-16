using System.Collections.Generic;
using ECS.Notification;
using UnityEngine;

namespace Notifications
{
    public class SoundSystem : MonoBehaviour
    {
        [SerializeField]
        private Dictionary<SoundType, AK.Wwise.Event> sounds;
        
        [SerializeField]
        private AK.Wwise.Event music;

        [SerializeField]
        private AK.Wwise.Switch[] musicSwitchGroup;
        
        [SerializeField]
        private AK.Wwise.State[] musicStateGroup;
        
        [SerializeField]
        private AK.Wwise.Event stopMusic;
        
        [SerializeField]
        private AK.Wwise.Event introMusic;
        
        public void PlayEvent(SoundNotification notification)
        {
            // if (!string.IsNullOrWhiteSpace(switchGroup))
            //     AkUnitySoundEngine.SetSwitch(switchGroup, parameter, gameObject);
            
            
            if (sounds.TryGetValue(notification._soundType, out var sound))
            {
                sound.Post(gameObject);
            }
        }
        
        public void PlayCharacterMusic(int index, int segmentIndex)
        {
            StopCharacterMusic();
            musicSwitchGroup[index].SetValue(gameObject);
            musicStateGroup[segmentIndex].SetValue();
            music.Post(gameObject);
        }

        public void StopCharacterMusic()
        {
            stopMusic.Post(gameObject);
        }

        public void PlayIntroMusic()
        {
            introMusic.Post(gameObject);
        }
    }
}
