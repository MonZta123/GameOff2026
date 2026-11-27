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
        private AK.Wwise.Event[] deadSounds;

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
        
        [SerializeField]
        private AK.Wwise.Event characterAnnounce;
        
        [SerializeField]
        private AK.Wwise.Event win;

        [SerializeField]
        private AK.Wwise.Event startCutsceneSound;
    
        [SerializeField]
        private AK.Wwise.Event stopCutsceneSound;
    
        [SerializeField]
        private AK.Wwise.Event helicopterEnding;
        
        private readonly List<GameObject> _soundChannels = new();

        private int _nextSoundChannel;

        public void PlayCutsceneSound()
        {
            startCutsceneSound.Post(gameObject);
        }
        
        public void StopCutsceneSound()
        {
            stopCutsceneSound.Post(gameObject);
        }
        
        public void PlayHelicopterEnding()
        {
            helicopterEnding.Post(gameObject);
        }
        
        private void Awake()
        {
            for (var i = 0; i < 16; i++)
            {
                var go = new GameObject($"Sound Channel {i}");
                go.AddComponent<AkGameObj>();
                _soundChannels.Add(go);
                go.transform.SetParent(transform);
            }
        }

        public void PlayEvent(SoundNotification notification)
        {
            if (sounds.TryGetValue(notification._soundType, out var sound))
            {
                var nextGo = _soundChannels[_nextSoundChannel];
                _nextSoundChannel = (_nextSoundChannel + 1) % _soundChannels.Count;
                nextGo.transform.position = notification._position;
                AkUnitySoundEngine.SetObjectPosition(nextGo, nextGo.transform);

                sound.Post(nextGo);
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
            var result = introMusic.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, (_, type, _) =>
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
            stopPauseMusic.Post(gameObject);
        }

        public void PlayDeadSound(int index)
        {
            deadSounds[index].Post(gameObject);
        }

        public void AnnounceCharacter(int index, int segmentIndex)
        {
            musicSwitchGroup[index].SetValue(gameObject);
            musicStateGroup[segmentIndex].SetValue();
            characterAnnounce.Post(gameObject);
        }
        
        public void WinCharacter()
        {
            win.Post(gameObject);
        }
    }
}
