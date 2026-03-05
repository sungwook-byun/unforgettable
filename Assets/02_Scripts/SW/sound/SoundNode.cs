using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundNode : MonoBehaviour
{
    [Header("사운드 거리 설정")]
    public float activeRange = 10f;  // 소리가 커지는 최대 거리
    public float fadeRange = 35f;    // 소리가 완전히 사라지는 거리

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // 사운드 자동 재생 방지, 자동재생이 필요하다면 인스펙터에서 설정
    }

    // 사운드노드매니저에서 플레이어 위치를 전달받아 거리 기반으로 볼륨 제어
    public void UpdateVolume(Vector3 playerPosition)
    {
        // 플레이어와 사운드 사이의 거리
        float distance = Vector3.Distance(playerPosition, transform.position);

        // 거리 이내면 재생
        if (distance <= fadeRange)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            float volume = 1f - Mathf.Clamp01((distance - activeRange) / (fadeRange - activeRange));
            audioSource.volume = Mathf.Clamp01(volume);
        }
        // 거리 밖이면 끔
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    // 테스를 위한 기즈모 표기

    void OnDrawGizmos() // 오브젝트 누를때만 기즈모를 표기하려면 OnDrawGizmosSelected로 바꾸면 됌
    {
        Gizmos.color = Color.green; // 초록색 (최대볼륨 거리)
        Gizmos.DrawWireSphere(transform.position, activeRange);

        Gizmos.color = Color.red; //  빨간색 (사운드 사라지는 거리)
        Gizmos.DrawWireSphere(transform.position, fadeRange);
    }
}
