using System;
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

        [SerializeField]
        private AK.Wwise.Event stopSlowdown;
        
        [SerializeField]
        private AK.Wwise.Event pauseMusic;
        
        [SerializeField]
        private AK.Wwise.Event stopPauseMusic;

        public void PlayEvent(SoundNotification notification)
        {
            // if (!string.IsNullOrWhiteSpace(switchGroup))
            //     AkUnitySoundEngine.SetSwitch(switchGroup, parameter, gameObject);


            if (sounds.TryGetValue(notification._soundType, out var sound))
            {
                sound.Post(gameObject);
            }
        }

        public void PauseMusic()
        {
            music.ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Pause, 0,
                AkCurveInterpolation.AkCurveInterpolation_Constant);
            introMusic.ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Pause, 0,
                AkCurveInterpolation.AkCurveInterpolation_Constant);
        }

        public void ResumeMusic()
        {
            music.ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Resume, 0,
                AkCurveInterpolation.AkCurveInterpolation_Constant);
            introMusic.ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Resume, 0,
                AkCurveInterpolation.AkCurveInterpolation_Constant);
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

        public void PlayIntroMusic(Action cont)
        {
            var result = introMusic.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, (cookie, type, info) =>
            {
                if (type == AkCallbackType.AK_EndOfEvent)
                    cont();
            });
            
            if (result == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
            {
                cont();
            }
        }

        public void PlayFailMusic()
        {
            stopSlowdown.Post(gameObject);
            PlayPauseMusic();
        }

        public void PlayPauseMusic()
        {
            pauseMusic.Post(gameObject);
        }
        
        public void StopPauseMusic()
        {
            pauseMusic.Post(gameObject);
        }
    }
}
