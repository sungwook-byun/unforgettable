using UnityEditor;
using UnityEngine;

/// <summary>다국어 지원 개발 편의를 위한 커스텀 인스펙터 스크립트 </summary>
[CanEditMultipleObjects, CustomEditor(typeof(UILocalizedText))]
public class UILocalizedTextInspector : Editor 
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		UILocalizedText text = (UILocalizedText)target;
		
		// 인스펙터에 입력된 스트링 키를 통해 스트링을 가져옴
		string value = LocalizationTableReader.GetString(text.key);
		
		// 받아온 스트링을 인스펙터에 표시
		GUILayout.Label($"{value}", EditorStyles.boldLabel);
	}
}
