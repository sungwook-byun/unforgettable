using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class TimerController : MonoBehaviour
{
    [SerializeField] GameObject timer;
    [SerializeField] Image timerBar, timerIcon;
    [SerializeField] Gradient timerGradient;

    float duration = 60f; // 기본 제한 시간
    float alertTime = 10f;
    bool alerting;
    Tween alertTween;


    public float Remaining { get; private set; }
    public float Elapsed => duration - Remaining;
    public bool Running { get; private set; }
    public bool Paused { get; private set; }

    public event Action OnTimeUp; // 시간이 다 됐을 때 알림 (GameRunner가 구독)

    void Awake()
    {
        Remaining = duration;
        UpdateHUD();
    }

    void Update()
    {
        if (!Running || Paused) return;

        Remaining -= Time.deltaTime;
        if (Remaining < 0f) Remaining = 0f;

        UpdateHUD();

        if (Remaining <= 0f)
        {
            Running = false;
            OnTimeUp?.Invoke();
        }

        if (Running && !alerting && Remaining <= alertTime)
        {
            alerting = true;
            StartAlertTween();
        }
    }

    void StartAlertTween()
    {
        alertTween = timerIcon.rectTransform
            .DOScale(0.85f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StopAlertTween()
    {
        if (alertTween != null && alertTween.IsActive())
        {
            
            alertTween.Kill();
            timerIcon.rectTransform.localScale = Vector3.one;
        }
        alerting = false;

    }

    // GameRunner에서 호출
    public void StartTimer(float limitSec)
    {
        duration = limitSec;
        Remaining = duration;
        Running = true;
        Paused = false;
        UpdateHUD();
    }

    public void StopTimer()
    {
        Running = false;
        StopAlertTween();
    }

    public void SetPaused(bool pause, bool playing = true)
    {
        Paused = pause;
        if (playing) Time.timeScale = pause ? 0 : 1;

        //if (pause)
        //    alertTween.Pause(); 
        //else
        //    alertTween.Play();
    }

    public void SetStop(bool stop)
    {
        Running = !stop;
    }

    void UpdateHUD()
    {
        timerBar.fillAmount = 1 - (Remaining / duration);
        timerBar.color = timerGradient.Evaluate(timerBar.fillAmount);
    }

    // HUD를 숨기고 싶은 경우
    public void HideHUD()
    {
        timer.SetActive(false);
    }
}
