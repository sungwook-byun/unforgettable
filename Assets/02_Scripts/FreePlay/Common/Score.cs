using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] TMP_Text scoreText;
    [SerializeField] AudioClip scoreClip, comboClip;
    public int Value { get; private set; }
    int combo = 0;

    public void Add(int basePoint, bool comboable = true, bool playAudio = false)
    {
        if(playAudio)
        {
            if (comboable && combo > 0)
            {
                AudioManager.Instance.PlaySFX(comboClip);
            }
            else
            {
                AudioManager.Instance.PlaySFX(scoreClip);
            }
        }

        if (comboable) combo++;
        Value += basePoint * Mathf.Max(1, combo);
        UpdateHUD();
    }
    public void Miss() { combo = 0; }
    public void Reset() { Value = 0; combo = 0; UpdateHUD(); }
    void UpdateHUD() { scoreText.text = "점수 : " + Value.ToString(); }
}
