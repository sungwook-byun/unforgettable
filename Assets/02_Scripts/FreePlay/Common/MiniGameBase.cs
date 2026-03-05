using System;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    protected Score score;
    protected TimerController timer;
    protected InputReader input;
    protected bool clear;
    public event Action<int, bool> OnFinished; // 점수와 클리어 여부 넘김

    public virtual void Setup(Score s, TimerController t, InputReader i)
    { score = s; timer = t; input = i; }

    public abstract void Begin();
    public virtual void Pause(bool p) { }
    protected void Finish() 
    {
        OnFinished?.Invoke(score.Value, clear);
        timer.StopTimer();
        if (clear) 
        {
            if (PlayerPrefs.GetInt("MiniGameClear_" + MiniGameContext.gameIndex, 0) < MiniGameContext.gameLevel + 1)
                PlayerPrefs.SetInt("MiniGameClear_" + MiniGameContext.gameIndex, MiniGameContext.gameLevel + 1);
        } 
    }

    public void End() => Finish();
}
