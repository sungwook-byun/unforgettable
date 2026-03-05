using System;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 위치를 씬별로 저장하기 위한 클래스
[Serializable]
public class ScenePosition
{
    public string sceneName;
    public Vector3 position;
}

[Serializable]
public class DiaryEntry
{
    public int day;
    public string emotion;
}

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public string localTimeString;
    public List<ScenePosition> scenePositions = new List<ScenePosition>();
    public List<string> clearedEvents = new List<string>();
    public List<string> removedObjects = new List<string>();

    // 미션 관련 추가 데이터
    public bool isRequestActive;            // 현재 미션이 진행 중인지 여부  
    public int currentRequestId;            // 진행 중인 요청의 ID  
    public string currentObjectName;        // 진행 중인 요청 대상 오브젝트 이름  
    public int requestsClearedThisScene;    // 이번 씬에서 완료한 요청 개수 (0~2)

    // 다이어리 관련 정보 저장용
    public bool isDiaryMode; // 현재 다이어리 모드 여부
    public int currentDay = 1; // 현재 날짜

    // Memory World 관련 정보 저장용
    public Vector3 lastMemoryWorldPosition; // Memory World에서 마지막으로 텔레포트 전 위치

    public List<DiaryEntry> diaryEntries = new List<DiaryEntry>(); // Dictionary 대신 List로 감정 저장
}