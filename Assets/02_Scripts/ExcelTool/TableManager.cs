using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
	private List<StringTable> _stringTableList; // 스트링 테이블
	
	protected override void Awake()
	{
		base.Awake();
		_stringTableList = GetListJson<StringTable>();
	}
	
	/// <summary> key와 Language를 확인하여 스트링 값을 반환</summary>
	public string GetString(string key, Languages languages)
	{
		StringTable data = _stringTableList.Find(d => d.Str_Index == key);
		if (data == null)
		{
			Debug.LogError($"Str_Index [{key}]과(와) 맞는 데이터가 없습니다.");
			return string.Empty;
		}

		switch (languages)
		{
			case Languages.Korean:
				return data.KR_str;
			case Languages.English:
				return data.EN_str;
			case Languages.German:
				return data.DU_str;
			case Languages.Turkish:
				return data.TR_str;
			default:
				return string.Empty;
		}
	}
	
	/// <summary> Json 파일 파싱 </summary>
	private List<T> GetListJson<T>() where T: ExcelRowData
	{
		var type = typeof(T);
		var a =	Resources.Load<TextAsset>(type.Name);
		if (a == null)
		{
			Debug.LogError($"Resources 폴더에서 {type.Name} 파일을 찾을 수 없습니다.");
			return null;
		}
		
		JsonSerializerSettings jsonWriter = new JsonSerializerSettings() {
			NullValueHandling = NullValueHandling.Ignore
		};
		
		var list = JsonConvert.DeserializeObject<List<T>>(a.text, jsonWriter);
		return list;
	}
}
