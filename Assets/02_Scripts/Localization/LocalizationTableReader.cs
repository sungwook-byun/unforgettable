#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class LocalizationTableReader : AssetPostprocessor
{
	private static readonly string _localizationTablePath = "Assets/09_ExcelData/Resources/StringTable.json";
	private static readonly string _localizationSOPath = "Assets/09_ExcelData/ScriptableObject/StringTableSO.asset";
	
	static LocalizationTableReader()
	{
		// 예외 처리
		// StringTable 스크립터블 오브젝트에 데이터가 없을 경우 다시 json 파일을 읽어온다.
		LocalizationSO asset = LoadOrCreateAsset();
		var table = asset.StringTables;
		if (table == null || table.Count == 0)
		{
			SessionState.SetBool("MakeLocalizationSO", false);
		}
		
		// 에디터를 처음 열었을 때만 실행, 컴파일 시에는 실행하지 않음
		if (SessionState.GetBool("MakeLocalizationSO", false))
			return;
		
		SessionState.SetBool("MakeLocalizationSO", true);
		
		// Unity 내부 시스템 초기화된 후
		// 에디터 한 프레임이 실제로 돌기 시작할 때 실행할 함수 등록
		EditorApplication.update += RefreshScriptableObject;
	}
	
	/// <summary>
	/// ScriptableObject를 지정된 경로에서 로드하고, 없으면 생성 후 저장
	/// </summary>
	private static LocalizationSO LoadOrCreateAsset()
	{
		// 1️⃣ 에셋 로드
		LocalizationSO asset = AssetDatabase.LoadAssetAtPath<LocalizationSO>(_localizationSOPath);

		// 2️⃣ 존재하지 않으면 새로 생성
		if (asset == null)
		{
			asset = ScriptableObject.CreateInstance<LocalizationSO>();

			// 경로가 없다면 폴더 생성
			string folder = System.IO.Path.GetDirectoryName(_localizationSOPath);
			if (!System.IO.Directory.Exists(folder))
				System.IO.Directory.CreateDirectory(folder);

			AssetDatabase.CreateAsset(asset, _localizationSOPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		return asset;
	}

	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
		string[] movedAssets, string[] movedFromAssetPaths)
	{
		// 스트링테이블 json 파일이 생성 또는 수정되었는지 확인
		foreach (var path in importedAssets)
		{
			if (path.Equals(_localizationTablePath))
			{
				EditorApplication.update += RefreshScriptableObject;
				break;
			}
		}
	}

	/// <summary> json 파일을 통해 ScriptableObject 갱신 </summary>
	private static void RefreshScriptableObject()
	{
		// 에디터 첫 프레임이 돌면 이벤트 해제 (1회만 실행하기 위함)
		EditorApplication.update -= RefreshScriptableObject;

		// json 파일 있는지 체크
		if (false == File.Exists(_localizationTablePath))
		{
			SessionState.SetBool("MakeLocalizationSO", false);
			return;
		}
		
		// json 파일을 통해 ScriptableObject 생성
		string allStr = File.ReadAllText(_localizationTablePath);
		var stringTables = JsonConvert.DeserializeObject<List<StringTable>>(allStr);
		LocalizationSO asset = LoadOrCreateAsset();
		asset.StringTables = stringTables;
		
		EditorUtility.SetDirty(asset);
		AssetDatabase.SaveAssets();
	}

	/// <summary> 플레이 모드가 아닐 때 스트링 정보 가져오기 </summary>
	public static string GetString(string key)
	{
		LocalizationSO asset = LoadOrCreateAsset();
		var stringTableList = asset.StringTables;
		StringTable data = stringTableList.Find(d => d.Str_Index == key);
		if (data == null)
		{
			return "-";
		}
		
		return data.KR_str;
	}
}

#endif