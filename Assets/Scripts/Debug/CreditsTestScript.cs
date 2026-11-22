using UnityEngine;

namespace Debug
{
    public class CreditsTestScript : MonoBehaviour
    {
        [SerializeField]
        private ReplayCredits replayCredits;
        
        private void Start()
        {
            replayCredits.RunCredits(40);
        }   
    }
}
