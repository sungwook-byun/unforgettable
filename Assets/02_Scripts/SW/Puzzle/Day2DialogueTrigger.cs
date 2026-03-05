using System.Collections;
using UnityEngine;

public class Day2DialogueTrigger : MonoBehaviour
{
    [SerializeField] public DialogueScriptable dialogueData;
    //[SerializeField] private string interactPrompt = "E키를 눌러 대화하기";

    private bool isPlayerInRange = false;
    [SerializeField] private DialogueSystem system;
    [SerializeField] Day2Controller day2Controller;
    [SerializeField] PlayerController_Dream playerController;

    private void Start()
    {
        system = FindFirstObjectByType<DialogueSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!system.IsDialogueActive)
                system.StartDialogue(dialogueData, OnDialogueEnd);
            playerController.enabled = false;
            playerController.GetComponentInChildren<Animator>().SetBool("IsMoving", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            day2Controller.readyInteraction = false;
        }
    }

    private void OnDialogueEnd()
    {
        StartCoroutine(ReadyInteraction());
    }
    
    IEnumerator ReadyInteraction()
    {
        yield return null;
        day2Controller.readyInteraction = true;
        playerController.enabled = true;
    }
}
