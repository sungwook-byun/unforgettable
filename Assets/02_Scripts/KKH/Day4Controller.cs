using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Day4Controller : MonoBehaviour
{
    private bool _isHouseOn;
    private bool _isWardrobeInteractable;
    private bool _isHouseOff;
    private bool _isSilkwormOn;
    private bool _isDyePotOn;
    private bool _isSpinningWheelOn;
    private bool _isLaughingOn;
    private bool _isFloorOff;
    private bool _isLightEffectOn;
    private bool _isGrandmaOn;
    private bool _isGrandmaUp;
    private bool _isGrandmaGone;
    
    [SerializeField] private GameObject _house;
    [SerializeField] private GameObject _grandmaFirst;
    [SerializeField] private GameObject _wardrobe;
    [SerializeField] private GameObject _silkworm;
    [SerializeField] private GameObject _dyePot;
    [SerializeField] private GameObject _spinningWheel;
    [SerializeField] private GameObject _floor;
    [SerializeField] private GameObject[] _lightEffects;
    [SerializeField] private GameObject _grandmaFinal;
    
    [SerializeField] private AudioSource _laughAudioSource;
    [SerializeField] private Transform _camTarget;
    
    [SerializeField] private Transform _fadeTr;
    [SerializeField] private GameObject _unforgettableUI;
    
    private DialogueEvent _dialogue;

    private void Start()
    {
        _dialogue = GetComponent<DialogueEvent>();
        _dialogue.dialogues[0].Condition = () => true;
        _dialogue.dialogues[0].OnDialogueComplete = () => {
            _unforgettableUI.SetActive(true);
        };        
        StartCoroutine(Day4EventRoutine());
    }

    private IEnumerator Day4EventRoutine()
    {
        yield return new WaitUntil(() => _isHouseOn);
        _house.SetActive(true);
        _grandmaFirst.layer = LayerMask.NameToLayer("Interact");
        
        yield return new WaitUntil(() => _isWardrobeInteractable);
        _wardrobe.tag = "Selectable";
        _wardrobe.layer = LayerMask.NameToLayer("Interact");
        
        yield return new WaitUntil(() => _isHouseOff);
        _house.SetActive(false);
        
        yield return new WaitUntil(() => _isSilkwormOn);
        _silkworm.SetActive(true);
        
        yield return new WaitUntil(() => _isDyePotOn);
        _dyePot.SetActive(true);
        
        yield return new WaitUntil(() => _isSpinningWheelOn);
        _spinningWheel.SetActive(true);
        
        yield return new WaitUntil(() => _isLaughingOn);
        _laughAudioSource.Play();
        var playerController = FindFirstObjectByType<PlayerController_Dream>();
        playerController.enabled = false;
        var camFollow = FindFirstObjectByType<CameraFollow>();
        camFollow.enabled = false;
        
        yield return new WaitForSeconds(5f);
        SetFloorOff();
        
        yield return new WaitUntil(() => _isFloorOff);
        _floor.SetActive(false);
        
        yield return new WaitForSeconds(2f);
        SetLightEffectOn();
        
        yield return new WaitUntil(() => _isLightEffectOn);
        for (int i = 0; i < _lightEffects.Length; i++)
        {
            _lightEffects[i].SetActive(true);
            yield return new WaitForSeconds(1f);
        }
        SetGrandmaOn();
        
        yield return new WaitUntil(() => _isGrandmaOn);
        _grandmaFinal.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        var cam = Camera.main;
        cam.transform.DOMove(_camTarget.position, 1f);
        // cam.transform.DOLookAt(_grandmaFinal.transform.position,1f);
        cam.transform.DORotate(_camTarget.rotation.eulerAngles, 1f);
        
        yield return new WaitForSeconds(3f);
        SetGrandmaUp();
        
        yield return new WaitUntil(() => _isGrandmaUp);
        var up = _grandmaFinal.GetComponent<GrandmaUp>();
        up.Up();

        yield return new WaitForSeconds(5f);
        SetGrandmaGone();
        
        yield return new WaitUntil(() => _isGrandmaGone);
        _grandmaFinal.SetActive(false);
        
        yield return new WaitForSeconds(3f);
        _dialogue.OnInteract();

        if (_fadeTr != null)
        {
            var background = _fadeTr.GetComponent<Image>();
            background.enabled = true;
            var fade = _fadeTr.GetComponent<CanvasGroup>();
            float elapsed = 0f;
            float time = 3f;
            float percent = 0f;
            fade.alpha = 0f;

            while (elapsed < time) {
                elapsed += Time.deltaTime;
                percent = elapsed / time;
                fade.alpha = Mathf.Lerp(0f, 1f, percent);
                yield return null;
            }
            
            fade.alpha = 1f;
        }
    }

    #region 외부 호출

    public void SetHouseOn()
    {
        _isHouseOn = true;
    }

    public void SetWardrobeOn()
    {
        _isWardrobeInteractable = true;
    }

    public void SetHouseOff()
    {
        _isHouseOff = true;
    }

    public void SetSilkwormOn()
    {
        _isSilkwormOn = true;
    }

    public void SetDyePotOn()
    {
        _isDyePotOn = true;
    }

    public void SetSpinningWheelOn()
    {
        _isSpinningWheelOn = true;
    }

    public void SetLaughingOn()
    {
        _isLaughingOn = true;
    }

    public void SetFloorOff()
    {
        _isFloorOff = true;
    }

    public void SetLightEffectOn()
    {
        _isLightEffectOn = true;
    }

    public void SetGrandmaOn()
    {
        _isGrandmaOn = true;
    }

    public void SetGrandmaUp()
    {
        _isGrandmaUp = true;
    }

    public void SetGrandmaGone()
    {
        _isGrandmaGone = true;
    }

    #endregion
    
}
