
public static class Contants {
    // 물건을 건드린 상태
    public static bool itemTouched = false;

    // 포스터 이벤트 상태
    public static bool posterTouched = false;        // 포스터 처음 클릭 여부
    public static bool posterEventCompleted = false; // 이벤트 처리 완료 여부
    public static bool posterEventDone = false;

    // 레코드 플레이어 이벤트 상태
    public static bool recordPlayerTouched = false; // 레코드 플레이어 처음 클릭 여부
    public static bool recordPlayerEventStarted = false; // 이벤트 시작 여부
    public static bool recordPlayerEventCompleted = false; // 이벤트 처리 완료 여부
    public static bool recordPlayerHasDisc = false; // 레코드 플레이어에 디스크가 있는지 여부

    // 옷장 이벤트 상태
    public static bool wardrobeTouched = false; // 옷장 처음 열림 여부
    public static bool wardrobeEventCompleted = false; // 이벤트 처리 완료 여부
    public static bool wardrobeHasPencil = false; // 옷장에 연필이 있는지 여부
    public static bool wardrobeEventStarted = false; // 옷장 이벤트 시작 여부

    // 할머니 이벤트 상태
    public static bool grandmaTocuhed = false;
    public static bool grandmaPosterEventStarted = false;
    public static bool grandmaRecordPlayerEventStarted = false;
    public static bool grandmaWardrobeEventStarted = false;
}