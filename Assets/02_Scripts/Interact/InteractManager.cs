using UnityEngine;

public class InteractManager : MonoBehaviour {
    public static InteractManager Instance { get; private set; }

    [Header("Interact Settings")]
    [SerializeField] private GameObject posterObj;
    [SerializeField] private GameObject wardrobeObj;
    [SerializeField] private GameObject drawerObj;
    [SerializeField] private GameObject deskObj;
    [SerializeField] private GameObject recordPlayerObj;
    [SerializeField] private GameObject journalObj;
    [SerializeField] private GameObject grandmaObj;
    [SerializeField] private GameObject portalObj;
    [SerializeField] private VFXFadeIn interactVFX;
    [SerializeField] private GameObject posterCompletedObj;

    private void Awake() {
        // 씬 한정 싱글톤 구현
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // 중복 방지
            return;
        }
        Instance = this;
        // DontDestroyOnLoad 적용 안 함
    }

    public void ResetTagsAndLayouts() {
        posterObj.tag = "Untagged";
        wardrobeObj.tag = "Untagged";
        drawerObj.tag = "Untagged";
        deskObj.tag = "Untagged";
        recordPlayerObj.tag = "Untagged";
        journalObj.tag = "Selectable";
        portalObj.tag = "Untagged";

        posterObj.layer = LayerMask.NameToLayer("Default");
        wardrobeObj.layer = LayerMask.NameToLayer("Default");
        drawerObj.layer = LayerMask.NameToLayer("Default");
        deskObj.layer = LayerMask.NameToLayer("Default");
        recordPlayerObj.layer = LayerMask.NameToLayer("Default");
        journalObj.layer = LayerMask.NameToLayer("Interact");
        portalObj.layer = LayerMask.NameToLayer("Default");
    }

    public void SetTageAndLayoutAll() {
        posterObj.tag = "Selectable";
        wardrobeObj.tag = "Selectable";
        drawerObj.tag = "Selectable";
        deskObj.tag = "Selectable";
        recordPlayerObj.tag = "Selectable";

        posterObj.layer = LayerMask.NameToLayer("Interact");
        wardrobeObj.layer = LayerMask.NameToLayer("Interact");
        drawerObj.layer = LayerMask.NameToLayer("Interact");
        deskObj.layer = LayerMask.NameToLayer("Interact");
        recordPlayerObj.layer = LayerMask.NameToLayer("Interact");
    }

    public void SetTageAndLayoutPoster() {
        ResetTagsAndLayouts();
        posterObj.tag = "Selectable";
        posterObj.layer = LayerMask.NameToLayer("Interact");
    }

    public void SetNotTageAndLayoutPoster() {
        posterObj.tag = "Untagged";
        posterObj.layer = LayerMask.NameToLayer("Default");
    }

    public void SetTageAndLayoutWardrobe() {
        ResetTagsAndLayouts();
        wardrobeObj.tag = "Selectable";
        wardrobeObj.layer = LayerMask.NameToLayer("Interact");
        deskObj.tag = "Selectable";
        deskObj.layer = LayerMask.NameToLayer("Interact");
    }

    public void SetNotTageAndLayoutWardrobe() {
        wardrobeObj.tag = "Untagged";
        wardrobeObj.layer = LayerMask.NameToLayer("Default");
        deskObj.tag = "Untagged";
        deskObj.layer = LayerMask.NameToLayer("Default");
    }

    public void SetTageAndLayoutDrawer() {
        ResetTagsAndLayouts();
        drawerObj.tag = "Selectable";
        drawerObj.layer = LayerMask.NameToLayer("Interact");
    }

    public void SetTageAndLayoutDesk() {
        ResetTagsAndLayouts();
        deskObj.tag = "Selectable";
        deskObj.layer = LayerMask.NameToLayer("Interact");
    }

    public void SetTageAndLayoutRecordPlayer() {
        ResetTagsAndLayouts();
        recordPlayerObj.tag = "Selectable";
        drawerObj.tag = "Selectable";
        recordPlayerObj.layer = LayerMask.NameToLayer("Interact");
        drawerObj.layer = LayerMask.NameToLayer("Interact");
    }

    public void SetNotTageAndLayoutRecordPlayer() {
        recordPlayerObj.tag = "Untagged";
        drawerObj.tag = "Untagged";
        recordPlayerObj.layer = LayerMask.NameToLayer("Default");
        drawerObj.layer = LayerMask.NameToLayer("Default");
    }

    public void SetTageAndLayoutJournal(bool isInteract) {
        journalObj.tag = isInteract ? "Selectable" : "Untagged";
        journalObj.layer = isInteract ? LayerMask.NameToLayer("Interact") : LayerMask.NameToLayer("Default");
    }

    public void SetTagAndLayoutGrandma(bool isInteract, bool allReset = true) {
        if (allReset) ResetTagsAndLayouts();
        grandmaObj.tag = isInteract ? "Grandma" : "Untagged";
        grandmaObj.layer = isInteract ? LayerMask.NameToLayer("Interact") : LayerMask.NameToLayer("Default");
    }

    public void SetActivePortal() {
        interactVFX.Show();
        portalObj.tag = "Portal";
        portalObj.layer = LayerMask.NameToLayer("Interact");
    }

    public void SetPosterObj() {
        posterCompletedObj.SetActive(true);
        posterObj.SetActive(false);
    }
}
