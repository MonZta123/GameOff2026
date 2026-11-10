using System.Collections.Generic;
using ECS.Notification;
using UnityEngine;

namespace Notifications
{
    public class SoundNotification : MonoBehaviour
    {
        [SerializeField]
        private Dictionary<SoundType, AK.Wwise.Event> sounds;

        public void PlayEvent(SoundType soundType, string switchGroup = null, string parameter = null)
        {
            // if (!string.IsNullOrWhiteSpace(switchGroup))
            //     AkUnitySoundEngine.SetSwitch(switchGroup, parameter, gameObject);

            if (sounds.TryGetValue(soundType, out var sound))
            {
                sound.Post(gameObject);
            }
        }
    }
}
