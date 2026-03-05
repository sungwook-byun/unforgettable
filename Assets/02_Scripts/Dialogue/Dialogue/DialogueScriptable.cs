using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Script")]
public class DialogueScriptable : ScriptableObject {
    public SpeakerData[] speakers;
    public DialogueData[] dialogues;
}
