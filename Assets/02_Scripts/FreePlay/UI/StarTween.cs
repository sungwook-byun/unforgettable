using UnityEngine;
using DG.Tweening;

public class StarTween : MonoBehaviour
{
    [SerializeField] RectTransform star, shine;

    Sequence starSeq, shineSeq;

    public void SetEmpty()
    {
        Stop();
        star.gameObject.SetActive(false);
        shine.gameObject.SetActive(false);
    }

    public void SetFilled()
    {
        star.gameObject.SetActive(true);
        shine.gameObject.SetActive(true);
    }

    public void Play()
    {
        if (star == null || shine == null) return;
        SetFilled();

        float shinePos = shine.sizeDelta.x / 2f;

        // 기존 재생 중이던 시퀀스 정리
        DOTween.Kill(star);
        DOTween.Kill(shine);
        starSeq?.Kill();
        shineSeq?.Kill();

        star.localScale = Vector3.zero;
        shine.localPosition = new Vector3(-shinePos, shinePos, 0);

        starSeq = DOTween.Sequence().SetLink(star.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);
        starSeq.Append(star.DOScale(1f, 1f).SetEase(Ease.OutBack)).SetLink(star.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);
        starSeq.Join(star.DOLocalRotate(new Vector3(0, 0, 360f), 1f, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic)).SetLink(star.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);

        // 스타 애니 끝나면 샤인 루프 시작
        starSeq.OnComplete(() => ShineLoop(shinePos));
    }

    void ShineLoop(float shinePos)
    {
        shineSeq?.Kill();
        DOTween.Kill(shine);

        shineSeq = DOTween.Sequence().SetLink(shine.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);

        shineSeq.Append(shine.DOLocalMove(new Vector3(shinePos, -shinePos, 0), 0.8f).From(new Vector3(-shinePos, shinePos, 0))
                .SetEase(Ease.InOutSine).SetLink(shine.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy));

        shineSeq.AppendInterval(1f);
        shineSeq.SetLoops(-1, LoopType.Restart);
    }

    public void Stop()
    {
        starSeq?.Kill();
        shineSeq?.Kill();
        DOTween.Kill(star);
        DOTween.Kill(shine);
        DOTween.Kill(this);
    }

    void OnDestroy() { Stop(); }
}
