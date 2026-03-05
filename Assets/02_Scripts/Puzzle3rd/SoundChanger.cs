using UnityEngine;
using System.Collections.Generic;

public class SoundChanger : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform player; // 플레이어의 Transform

    [Header("사운드 노드 목록")]
    
    [Tooltip("씬에 있는 모든 SoundNode 오브젝트들을 여기에 연결하세요.")]
    [SerializeField] private List<SoundNode> soundNodes; 

    void Update()
    {
        if (player == null || soundNodes == null) 
        {
            // 플레이어나 노드 목록이 없으면 아무것도 하지 않음
            return;
        }

        // 매 프레임 모든 사운드 노드들의 UpdateVolume을 호출
        foreach (var node in soundNodes)
        {
            // 노드가 존재하고 활성화 상태일 때만 볼륨 업데이트
            if (node != null && node.gameObject.activeInHierarchy)
            {
                node.UpdateVolume(player.position);
            }
        }
    }
}