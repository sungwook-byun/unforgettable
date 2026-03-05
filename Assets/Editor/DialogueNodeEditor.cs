using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DialogueNodeEditor : EditorWindow {
    private DialogueScriptable dialogueAsset;
    private Vector2 canvasOffset = Vector2.zero;

    private List<Node> nodes = new List<Node>();
    private Node selectedNode = null;
    private Vector2 dragOffset;
    private bool isResizing = false;
    private bool isPanning = false;
    private Vector2 panStart;

    private const float padding = 10f;
    private const float lineHeight = 20f;
    private const float choiceHeight = 20f;
    private const float portraitSize = 60f;
    private const float gridSnapSize = 10f; // 드래그 시 스냅 간격

    [MenuItem("Tools/Dialogue Node Editor")]
    public static void OpenWindow() => GetWindow<DialogueNodeEditor>("Dialogue Node Editor");

    private void OnGUI() {
        Event e = Event.current;

        ProcessCanvasPan(e);

        // DialogueScriptable 선택
        EditorGUILayout.BeginHorizontal();
        DialogueScriptable newAsset = (DialogueScriptable)EditorGUILayout.ObjectField(
            "Dialogue Script",
            dialogueAsset,
            typeof(DialogueScriptable),
            false);
        if (newAsset != dialogueAsset) {
            dialogueAsset = newAsset;
            LoadNodesFromAsset();
        }
        EditorGUILayout.EndHorizontal();

        if (dialogueAsset == null) {
            EditorGUILayout.HelpBox("대화 스크립트를 선택하거나 생성하세요.", MessageType.Info);
            if (GUILayout.Button("새 Dialogue Script 생성"))
                CreateNewDialogueAsset();
            return;
        }

        DrawToolbar();

        EditorGUILayout.BeginHorizontal();

        // 좌측: Node 편집 캔버스
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.65f));
        DrawNodes(e);
        EditorGUILayout.EndVertical();

        // 우측: Speakers 배열 편집 패널
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.35f));
        DrawSpeakersPanel();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        // Delete / Backspace 키로 노드 삭제
        if (Event.current.type == EventType.KeyDown) {
            if ((Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace) && selectedNode != null) {
                nodes.Remove(selectedNode);
                selectedNode = null;

                AutoArrangeNodes(); // 삭제 후 위치 및 Title 갱신

                GUI.changed = true;
                Event.current.Use();
            }
        }

        if (GUI.changed)
            Repaint();
    }

    #region Toolbar
    private void DrawToolbar() {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Create Node", EditorStyles.toolbarButton)) {
            Node newNode = new Node($"Dialogue {nodes.Count + 1}", new Rect(0, 0, 320, 320));
            nodes.Add(newNode);
            AutoArrangeNodes();
        }

        if (GUILayout.Button("Delete Node", EditorStyles.toolbarButton)) {
            if (selectedNode != null) {
                nodes.Remove(selectedNode);
                selectedNode = null;
                AutoArrangeNodes();
                GUI.changed = true;
            }
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton)) {
            SaveNodesToAsset();
        }

        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region Nodes
    private void DrawNodes(Event e) {
        BeginWindows();

        for (int i = 0; i < nodes.Count; i++) {
            Node node = nodes[i];
            node.rect.height = CalculateNodeHeight(node);
            Rect drawRect = new Rect(node.rect.position + canvasOffset, node.rect.size);

            // 선택된 노드 테두리 표시
            if (selectedNode == node) {
                Color prev = GUI.color;
                GUI.color = Color.cyan;
                GUI.Box(drawRect, "", EditorStyles.helpBox);
                GUI.color = prev;
            } else {
                GUI.Box(drawRect, "", GUIStyle.none);
            }

            GUILayout.BeginArea(drawRect);
            GUILayout.Space(padding);

            EditorGUI.BeginChangeCheck();

            node.title = EditorGUILayout.TextField("Title", node.title);

            // Speaker Index 드롭다운
            string[] speakerNames = dialogueAsset != null && dialogueAsset.speakers != null
                ? System.Array.ConvertAll(dialogueAsset.speakers, s => s.speakerName)
                : new string[] { "None" };
            node.speakerIndex = EditorGUILayout.Popup("Speaker Index", node.speakerIndex, speakerNames);

            // Speaker Portrait & Description
            if (dialogueAsset != null && dialogueAsset.speakers != null && node.speakerIndex >= 0 && node.speakerIndex < dialogueAsset.speakers.Length) {
                node.speakerPortrait = dialogueAsset.speakers[node.speakerIndex].portrait;
                node.speakerDescription = dialogueAsset.speakers[node.speakerIndex].description;
            }

            node.dialogueText = EditorGUILayout.TextArea(node.dialogueText, GUILayout.Height(60));
            node.typingSpeed = EditorGUILayout.FloatField("Typing Speed", node.typingSpeed);
            node.dialoguePortrait = (Sprite)EditorGUILayout.ObjectField("Dialogue Portrait", node.dialoguePortrait, typeof(Sprite), false);

            node.isChoiceOnly = EditorGUILayout.Toggle("Is Choice Only", node.isChoiceOnly);
            node.filterType = (FilterUI.FilterType)EditorGUILayout.EnumPopup("Filter Type", node.filterType);

            // Next Dialogue Index
            string[] nodeTitles = new string[] { "None" }.Concat(nodes.ConvertAll(n => n.title)).ToArray();
            int currentNextIndex = Mathf.Clamp(node.nextDialogueIndex, -1, nodes.Count - 1);
            int selectedNextIndex = EditorGUILayout.Popup("Next Dialogue", currentNextIndex + 1, nodeTitles);
            node.nextDialogueIndex = selectedNextIndex - 1;

            GUILayout.Label("Choices:");
            if (node.choices == null) node.choices = new DialogueChoice[0];

            for (int c = 0; c < node.choices.Length; c++) {
                GUILayout.BeginHorizontal();
                node.choices[c].choiceText = EditorGUILayout.TextField(node.choices[c].choiceText);

                string[] nodeOptions = new string[] { "None" }.Concat(nodes.ConvertAll(n => n.title)).ToArray();
                int currentChoiceIndex = Mathf.Clamp(node.choices[c].nextIndex, -1, nodes.Count - 1);
                int selectedChoice = EditorGUILayout.Popup(currentChoiceIndex + 1, nodeOptions, GUILayout.Width(120));
                node.choices[c].nextIndex = selectedChoice - 1;

                if (GUILayout.Button("X", GUILayout.Width(25))) {
                    RemoveChoice(node, c);
                    GUILayout.EndHorizontal();
                    AutoArrangeNodes();
                    break;
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Choice", GUILayout.Height(choiceHeight))) {
                AddChoice(node);
                AutoArrangeNodes(); 
            }

            if (EditorGUI.EndChangeCheck())
                GUI.changed = true;

            GUILayout.EndArea();

            // Resize 처리
            Rect resizeRect = new Rect(drawRect.xMax - 12, drawRect.yMax - 12, 12, 12);
            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeUpLeft);

            if (e.type == EventType.MouseDown && resizeRect.Contains(e.mousePosition)) {
                selectedNode = node;
                isResizing = true;
                e.Use();
            }

            if (isResizing && selectedNode == node && e.type == EventType.MouseDrag) {
                selectedNode.rect.width = Mathf.Max(150, e.mousePosition.x - selectedNode.rect.x - canvasOffset.x);
                selectedNode.rect.height = Mathf.Max(150, e.mousePosition.y - selectedNode.rect.y - canvasOffset.y);
                GUI.changed = true;
            }

            if (e.type == EventType.MouseUp && isResizing) {
                isResizing = false;
                selectedNode = null;
            }

            // Node 이동 및 선택 처리
            if (e.type == EventType.MouseDown && e.button == 0 && drawRect.Contains(e.mousePosition)) {
                selectedNode = node;
                dragOffset = e.mousePosition - canvasOffset - node.rect.position;
                GUI.FocusControl(null);
                e.Use();
            }

            if (e.type == EventType.MouseDrag && selectedNode == node && !isResizing) {
                Vector2 newPos = e.mousePosition - dragOffset - canvasOffset;
                // 🔹 그리드 스냅 적용
                newPos.x = Mathf.Round(newPos.x / gridSnapSize) * gridSnapSize;
                newPos.y = Mathf.Round(newPos.y / gridSnapSize) * gridSnapSize;
                node.rect.position = newPos;
                GUI.changed = true;
            }
        }

        EndWindows();
    }

    private float CalculateNodeHeight(Node node) {
        float yOffset = padding;
        yOffset += lineHeight * 2;
        yOffset += 60 + 2;
        yOffset += lineHeight + 2 + portraitSize + 5;
        yOffset += lineHeight;
        yOffset += lineHeight;
        if (node.choices != null)
            yOffset += (choiceHeight + 4) * node.choices.Length;
        yOffset += choiceHeight + 10;
        yOffset += padding * 2;
        return Mathf.Max(300, yOffset);
    }
    #endregion

    #region Canvas Pan
    private void ProcessCanvasPan(Event e) {
        if (e.button == 2 || (e.button == 0 && e.modifiers == EventModifiers.Alt)) {
            if (e.type == EventType.MouseDown) {
                isPanning = true;
                panStart = e.mousePosition;
                e.Use();
            } else if (e.type == EventType.MouseDrag && isPanning) {
                canvasOffset += e.delta;
                e.Use();
            } else if (e.type == EventType.MouseUp) {
                isPanning = false;
                e.Use();
            }
        }
    }
    #endregion

    #region Node Management
    private void AutoArrangeNodes() {
        if (nodes.Count == 0) return;

        int col = 0;
        int maxPerRow = 4;
        float xSpacing = 350f;
        float startX = 50f;
        float startY = 50f;

        float y = startY;
        float rowHeight = 0f;

        for (int i = 0; i < nodes.Count; i++) {
            Node node = nodes[i];
            float nodeHeight = CalculateNodeHeight(node);

            node.rect.position = new Vector2(startX + col * xSpacing, y);
            node.title = $"Dialogue {i + 1}";

            rowHeight = Mathf.Max(rowHeight, nodeHeight);
            col++;

            if (col >= maxPerRow) {
                col = 0;
                y += rowHeight + 20f; // 행 사이 여백
                rowHeight = 0f;
            }
        }
    }

    private void LoadNodesFromAsset() {
        nodes.Clear();
        if (dialogueAsset == null || dialogueAsset.dialogues == null) return;

        for (int i = 0; i < dialogueAsset.dialogues.Length; i++) {
            var d = dialogueAsset.dialogues[i];
            Node n = new Node(d);
            nodes.Add(n);
        }
        AutoArrangeNodes(); // 불러오기 후 그리드 배치
    }

    private void AddChoice(Node node) {
        var list = new List<DialogueChoice>(node.choices);
        list.Add(new DialogueChoice() { choiceText = "New Choice", nextIndex = -1 });
        node.choices = list.ToArray();
    }

    private void RemoveChoice(Node node, int index) {
        var list = new List<DialogueChoice>(node.choices);
        list.RemoveAt(index);
        node.choices = list.ToArray();
    }

    private void SaveNodesToAsset() {
        if (dialogueAsset == null) return;
        List<DialogueData> dataList = new List<DialogueData>();
        foreach (var n in nodes) {
            DialogueData data = new DialogueData {
                dialogue = n.dialogueText,
                typingSpeed = n.typingSpeed,
                customPortrait = n.dialoguePortrait,
                choices = n.choices,
                speakerIndex = n.speakerIndex,
                isChoiceOnly = n.isChoiceOnly,
                nextDialogueIndex = n.nextDialogueIndex,
                filterType = n.filterType
            };
            dataList.Add(data);
        }
        dialogueAsset.dialogues = dataList.ToArray();
        EditorUtility.SetDirty(dialogueAsset);
        AssetDatabase.SaveAssets();
        Debug.Log("Dialogue Script saved.");
    }
    #endregion

    #region Speakers Panel
    private void DrawSpeakersPanel() {
        EditorGUILayout.LabelField("Speakers", EditorStyles.boldLabel);

        if (dialogueAsset.speakers == null) dialogueAsset.speakers = new SpeakerData[0];

        for (int i = 0; i < dialogueAsset.speakers.Length; i++) {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Speaker {i}", EditorStyles.boldLabel);

            dialogueAsset.speakers[i].speakerName =
                EditorGUILayout.TextField("Name", dialogueAsset.speakers[i].speakerName);
            dialogueAsset.speakers[i].description =
                EditorGUILayout.TextField("Description", dialogueAsset.speakers[i].description);
            dialogueAsset.speakers[i].portrait =
                (Sprite)EditorGUILayout.ObjectField("Portrait", dialogueAsset.speakers[i].portrait, typeof(Sprite), false);

            if (GUILayout.Button("Remove")) {
                RemoveSpeaker(i);
                break;
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Speaker")) {
            AddSpeaker();
        }
    }

    private void AddSpeaker() {
        List<SpeakerData> list = new List<SpeakerData>(dialogueAsset.speakers);
        list.Add(new SpeakerData() { speakerName = "New Speaker", description = "", portrait = null });
        dialogueAsset.speakers = list.ToArray();
    }

    private void RemoveSpeaker(int index) {
        List<SpeakerData> list = new List<SpeakerData>(dialogueAsset.speakers);
        list.RemoveAt(index);
        dialogueAsset.speakers = list.ToArray();
    }
    #endregion

    #region Create Asset
    private void CreateNewDialogueAsset() {
        string path = EditorUtility.SaveFilePanelInProject("Save Dialogue Script", "NewDialogue", "asset", "Choose location to save.");
        if (!string.IsNullOrEmpty(path)) {
            DialogueScriptable newAsset = ScriptableObject.CreateInstance<DialogueScriptable>();
            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();
            dialogueAsset = newAsset;
        }
    }
    #endregion

    #region Node Class
    [System.Serializable]
    public class Node {
        public Rect rect;
        public string title;
        public int speakerIndex = 0;
        public string speakerDescription;
        public Sprite speakerPortrait;
        public string dialogueText;
        public float typingSpeed = 0.05f;
        public Sprite dialoguePortrait;
        public DialogueChoice[] choices;
        public bool isChoiceOnly = false;
        public int nextDialogueIndex = -1;
        public FilterUI.FilterType filterType = FilterUI.FilterType.None;

        public Node(DialogueData data) {
            title = "Node";
            rect = new Rect(0, 0, 320, 320);
            dialogueText = data.dialogue;
            typingSpeed = data.typingSpeed;
            dialoguePortrait = data.customPortrait;
            choices = data.choices != null ? data.choices : new DialogueChoice[0];
            speakerIndex = data.speakerIndex;
            isChoiceOnly = data.isChoiceOnly;
            nextDialogueIndex = data.nextDialogueIndex;
            filterType = data.filterType;
        }

        public Node(string title, Rect rect) {
            this.title = title;
            this.rect = rect;
            choices = new DialogueChoice[0];
            dialogueText = "";
            nextDialogueIndex = -1;
            filterType = FilterUI.FilterType.None;
        }
    }
    #endregion
}
