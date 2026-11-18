using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "MissionDialogues", menuName = "Mission Objects/Mission Dialogues")]
public class MissionDialogue : ScriptableObject
{
    public MissionTypeDialogue[] missionDialogueDatabase;

    [System.Serializable]
    public class MissionTypeDialogue
    {
        public MissionTypes missionTypes;
        public CharacterSelect character;
        public List<string> dialogue = new List<string>();
    }

    public enum MissionTypes
    {
        combo,
        loot,
        kill
    }

    public enum CharacterSelect
    {
        olga,
        dave,
        tnt,
        renoir,
        general
    }
}
