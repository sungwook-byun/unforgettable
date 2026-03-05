using UnityEngine;
using System.Collections.Generic;

public class SoundNodeController : MonoBehaviour
{
    [Header("플레이어")]
    public Transform player;

    [Header("사운드 노드들")]
    public List<SoundNode> soundNodes = new List<SoundNode>();

    void Awake()
    {
        // 혹시 인스펙터에 비어 있으면 자동 탐색 (예비용)
        if (soundNodes.Count == 0)
            soundNodes.AddRange(FindObjectsByType<SoundNode>(FindObjectsSortMode.None));
    }

    void Update()
    {
        if (player == null) return;

        foreach (var node in soundNodes)
        {
            if (node != null && node.enabled)
                node.UpdateVolume(player.position);
        }
    }
}
