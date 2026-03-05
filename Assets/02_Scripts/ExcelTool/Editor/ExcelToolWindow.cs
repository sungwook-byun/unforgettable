using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using RedGame.Framework.EditorTools;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ExcelToolWindow : EditorWindow
{
	private static EditorWindow window;

	// 엑셀 리스트 표시
	private SimpleEditorTableView<ExcelData> _excelTableView;
	private static List<ExcelData> excelDataList = new List<ExcelData>();

	// 윈도우 탭
	// private int selectedTab;
	// private string[] tabLabels = { "DB", "Setting" };

	// 폴더 경로
	private string excelPath;
	private static string csSavePath;
	private static string jsonSavePath;

	// 전체 선택 여부
	private bool isCheckAll;

	[MenuItem("Mid Heroes/Excel Importer")]
	public static void ShowWindow()
	{
		window = GetWindow<ExcelToolWindow>("Excel Importer");
		window.maxSize = new Vector2(1280, 720);
		window.minSize = new Vector2(900, 480);
	}

	private void OnEnable()
	{
		SetTableView();

		LoadPathSetting();
		
		Refresh();
	}
	
	private void OnGUI()
	{
		DrawDBTab();

		// 탭 바 그리기
		// selectedTab = GUILayout.Toolbar(selectedTab, tabLabels);
		// GUILayout.Space(10);
		//
		// // 선택된 탭에 따라 다른 내용 그리기
		// switch (selectedTab)
		// {
		// 	case 0:
		// 		DrawDBTab();
		// 		break;
		// 	case 1:
		// 		DrawSettingTab();
		// 		break;
		// }
	}

	// DB 탭 그리기
	private void DrawDBTab()
	{
		GUILayout.Label("Excel DB", EditorStyles.boldLabel);

		GUILayout.BeginVertical(GUI.skin.box);
		DrawExcelImportButtons();
		DrawExcelList();
		GUILayout.EndVertical();
	}

	// Setting 탭 그리기
	// private void DrawSettingTab()
	// {
	// 	GUILayout.Label("Setting", EditorStyles.boldLabel);
	// 
	// 	GUILayout.BeginVertical(GUI.skin.box);
	// 	PathSetting();
	// 	GUILayout.EndVertical();
	// }

	#region Excel DB Tab
	
	// 테이블 뷰 표시
	private SimpleEditorTableView<ExcelData> SetTableView()
	{
		SimpleEditorTableView<ExcelData> tableView = new SimpleEditorTableView<ExcelData>();

		tableView.AddColumn("선택", 40, (rect, item) =>
		{
			rect.xMin += 10;
			item.check = EditorGUI.Toggle(rect, item.check);
		}).SetMaxWidth(40);

		tableView.AddColumn("엑셀 파일 경로", 650, (rect, item) => { EditorGUI.LabelField(rect, item.excelFullPath); });

		tableView.AddColumn("파일 열기", 80, (rect, item) =>
		{
			if (GUI.Button(rect, "Open File"))
			{
				if (!File.Exists(item.excelFullPath))
				{
					Debug.LogError($"파일이 존재하지 않습니다: {item.excelFullPath}");
					return;
				}

				Process.Start(item.excelFullPath);
			}
		}).SetMaxWidth(80).SetTooltip("이 엑셀 파일을 열어보려면 클릭하세요");

		tableView.AddColumn("폴더 열기", 80, (rect, item) =>
		{
			if (GUI.Button(rect, "Open Folder"))
			{
				var path = excelPath;
				if (!Directory.Exists(path))
				{
					Debug.LogError($"폴더가 존재하지 않습니다: {path}");
					return;
				}

#if UNITY_EDITOR_WIN
				Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
				Process.Start("open", path);
#endif
			}
		}).SetMaxWidth(80).SetTooltip("이 엑셀 경로 폴더를 열어보려면 클릭하세요");

		return tableView;
	}
	
	private void DrawExcelImportButtons()
	{
		GUILayout.Label("임포트");

		GUILayout.BeginHorizontal();
		GUI.backgroundColor = Color.green;
		if (GUILayout.Button("테이블 임포트"))
		{
			Import();
		}

		GUI.backgroundColor = Color.red;
		if (GUILayout.Button("업로드 데이터 삭제"))
		{
			Clear();
		}

		GUI.backgroundColor = Color.white;
		GUILayout.EndHorizontal();

		GUILayout.Space(10);
	}

	private void DrawExcelList()
	{
		GUILayout.Label("엑셀 리스트");

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("모두 선택"))
		{
			SelectAll();
		}

		if (GUILayout.Button("엑셀 리스트 업데이트"))
		{
			Refresh();
		}

		GUILayout.EndHorizontal();

		if (_excelTableView == null)
			_excelTableView = SetTableView();

		_excelTableView.DrawTableGUI(excelDataList.ToArray());

		GUILayout.Space(10);
	}

	// 임포트 버튼 클릭
	private void Import()
	{
		// 선택된 엑셀 리스트 확인
		var pathList = new List<string>();
		for (int i = 0; i < excelDataList.Count; i++)
		{
			if (excelDataList[i].check)
			{
				pathList.Add(excelDataList[i].excelFullPath);
			}
		}

		if (pathList.Count == 0) return;

		string[] paths = pathList.ToArray();
		ExcelToolManager.Instance.FileIOController.SaveSelectPath(paths);

		// SaveLoadData.SaveSelectPath(pahts);

		// 알림창 띄우기
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < pathList.Count; i++)
		{
			sb.AppendLine($"{Path.GetFileName(pathList[i]) }");
		}

		sb.Append("해당 Table을 변환할까요?");

		bool decision = EditorUtility.DisplayDialog(
			"알림", // title
			sb.ToString(), // description
			"예", // OK button
			"아니오" // Cancel button
		);

		if (decision)
		{
			ExcelToolManager.Instance.ExcelConverter.GenerateCSharpFiles(csSavePath);
			AssetDatabase.Refresh();
		}
	}

	// 초기화 버튼 클릭
	private void Clear()
	{
		bool decision = EditorUtility.DisplayDialog(
			"알림",
			"DB를 모두 삭제할까요?",
			"예",
			"아니오"
		);
		if (decision)
		{
			ExcelToolManager.Instance.FileIOController.ClearDirectory(csSavePath);
			ExcelToolManager.Instance.FileIOController.ClearDirectory(jsonSavePath);
			EditorPrefs.SetBool(ExcelToolStrDef.Key_MakeJson, false);
			AssetDatabase.Refresh();
			
			Debug.Log("모든 데이터 삭제 완료");
		}
	}

	// 모두 선택 버튼 클릭
	private void SelectAll()
	{
		if (isCheckAll)
		{
			isCheckAll = false;
			for (int i = 0; i < excelDataList.Count; i++)
			{
				excelDataList[i].check = false;
			}
		}
		else
		{
			isCheckAll = true;
			for (int i = 0; i < excelDataList.Count; i++)
			{
				excelDataList[i].check = true;
			}
		}
	}

	// 엑셀 리스트 업데이트 버튼 클릭
	private void Refresh()
	{
		excelDataList = ExcelToolManager.Instance.FileIOController.GetExcelFileList(excelPath);
	}

	[DidReloadScripts(1)]
	private static void OnScriptsReloaded()
	{

		var makeJson = EditorPrefs.GetBool(ExcelToolStrDef.Key_MakeJson, false);
		if (makeJson == false)
			return;

		EditorPrefs.SetBool(ExcelToolStrDef.Key_MakeJson, false);
		ExcelToolManager.Instance.ExcelConverter.GenerateJsonFiles(jsonSavePath, csSavePath);
	}

	#endregion

	#region Setting Tab

	public void PathSetting()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("엑셀 파일 저장 경로", GUILayout.Width(120));
		excelPath = EditorGUILayout.TextField(excelPath);
		if (GUILayout.Button("경로 선택", GUILayout.Width(80)))
		{
			var path = EditorUtility.OpenFolderPanel("엑셀 파일을 저장할 폴더를 선택하세요", excelPath, "");
			if (string.IsNullOrEmpty(path) == false)
			{
				excelPath = path + "/";
				SaveSetting();
			}
		}

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("CS 파일 저장 경로", GUILayout.Width(120));
		csSavePath = EditorGUILayout.TextField(csSavePath);
		if (GUILayout.Button("경로 선택", GUILayout.Width(80)))
		{
			var path = EditorUtility.OpenFolderPanel("CS 파일을 저장할 폴더를 선택하세요", excelPath, "");
			if (string.IsNullOrEmpty(path) == false)
			{
				csSavePath = path + "/";
				SaveSetting();
			}
		}

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Json 파일 저장 경로", GUILayout.Width(120));
		jsonSavePath = EditorGUILayout.TextField(jsonSavePath);
		if (GUILayout.Button("경로 선택", GUILayout.Width(80)))
		{
			var path = EditorUtility.OpenFolderPanel("Json 파일을 저장할 폴더를 선택하세요", excelPath, "");
			if (string.IsNullOrEmpty(path) == false)
			{
				jsonSavePath = path + "/";
				SaveSetting();
			}
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("경로 세팅 저장"))
		{
			SaveSetting();
		}
		GUI.backgroundColor = Color.red;
		if (GUILayout.Button("경로 초기화"))
		{
			ResetSetting();
		}
		GUI.backgroundColor = Color.white;
		GUILayout.EndHorizontal();

	}
	
	// 경로 세팅 파일 저장
	public void SaveSetting()
	{
		var dir = Application.persistentDataPath;
		string fileName = ExcelToolStrDef.FileName_PathSetting;
		
		var saveDic = new Dictionary<string, string>();
		saveDic.Add(ExcelToolStrDef.Key_ExcelFolderPath, excelPath);
		saveDic.Add(ExcelToolStrDef.Key_CsFolderPath, csSavePath);
		saveDic.Add(ExcelToolStrDef.Key_JsonFolderPath, jsonSavePath);
			
		var json = JsonConvert.SerializeObject(saveDic, Formatting.Indented);

		ExcelToolManager.Instance.FileIOController.WriteFile(json, dir, fileName);
	}
		
	// 경로 세팅 파일 로드
	public void LoadPathSetting()
	{
		var dir = Application.persistentDataPath;
		string fileName = ExcelToolStrDef.FileName_PathSetting;
		string savedPath = dir + "/" + fileName;
		
		if (!File.Exists(savedPath))
		{
			excelPath = Environment.CurrentDirectory + "/" + ExcelToolStrDef.DefaultExcelFolderPath;
			csSavePath = Environment.CurrentDirectory + "/" + ExcelToolStrDef.DefaultCsFolderPath;
			jsonSavePath = Environment.CurrentDirectory + "/" + ExcelToolStrDef.DefaultJsonFolderPath;
			SaveSetting();
		}
		
		var allStr = ExcelToolManager.Instance.FileIOController.ReadFile(dir, fileName);
		Dictionary<string, string> loadedDic = JsonConvert.DeserializeObject<Dictionary<string, String>>(allStr);
			
		loadedDic.TryGetValue(ExcelToolStrDef.Key_ExcelFolderPath, out string e);
		loadedDic.TryGetValue(ExcelToolStrDef.Key_CsFolderPath, out string c);
		loadedDic.TryGetValue(ExcelToolStrDef.Key_JsonFolderPath, out string j);
		
		excelPath = string.IsNullOrEmpty(e) ? Environment.CurrentDirectory + "/" + ExcelToolStrDef.DefaultExcelFolderPath : e;
		csSavePath = string.IsNullOrEmpty(c) ?  Environment.CurrentDirectory + "/" + ExcelToolStrDef.DefaultCsFolderPath : c;
		jsonSavePath = string.IsNullOrEmpty(j) ? Environment.CurrentDirectory + "/" + ExcelToolStrDef.DefaultJsonFolderPath : j;
	}
	
	// 경로 세팅 초기화 버튼 클릭
	public void ResetSetting()
	{
		bool decision = EditorUtility.DisplayDialog(
			"알림",
			"모든 설정을 기본값으로 되돌릴까요?",
			"예",
			"아니오"
		);
		if (decision)
		{
			string dir = Application.persistentDataPath;
			string fileName = ExcelToolStrDef.FileName_PathSetting;
			
			ExcelToolManager.Instance.FileIOController.ClearFile(dir, fileName);

			LoadPathSetting();

			EditorUtility.DisplayDialog("알림", "설정이 초기화 되었습니다.", "확인");
		}
	}
	
	#endregion

}