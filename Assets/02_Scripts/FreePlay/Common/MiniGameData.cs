using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameData", menuName = "MiniGames/MiniGameData")]
public class MiniGameData : ScriptableObject
{
    public GameObject gamePrefab;
    public string title;
    public string description;
    public string how;
    public Sprite sprite;
    public float timeLimit = 60f;
    public float countDown = 3f;
}
