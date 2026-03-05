using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class VFXFadeIn : MonoBehaviour {
    private VisualEffect vfx;

    void Awake() {
        vfx = GetComponent<VisualEffect>();
    }

    public void Show() {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        vfx.Play();
        Debug.Log("VFX Fade In started.");
    }
}
