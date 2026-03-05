using UnityEngine;
using UnityEngine.EventSystems;

public class ChoiceButton : MonoBehaviour, IPointerEnterHandler {
    private ChoiceUI choiceUI;
    private int buttonIndex;

    public void Init(ChoiceUI ui, int index) {
        choiceUI = ui;
        buttonIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        choiceUI.HighlightButton(buttonIndex);
    }
}
