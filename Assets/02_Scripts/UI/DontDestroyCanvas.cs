using UnityEngine;

public class DontDestroyCanvas : MonoBehaviour {
    [SerializeField] private Canvas canvas; // Inspector에서 캔버스 연결

    void Awake() {
        if (canvas != null) {
            DontDestroyOnLoad(canvas.gameObject);
            canvas.sortingOrder = 1000; // 다른 UI보다 높은 값으로 설정
        }
    }
}
