using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UILocalizedText : MonoBehaviour
{
	public string key;
	
	private TextMeshProUGUI _text;

	private void Awake()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		LocalizationManager.OnLanguageChanged += OnLanguageChanged; // 언어 변경 이벤트 등록

		if (key.Equals(string.Empty))
		{
			Debug.LogError($"{gameObject.name} 오브젝트에 스트링 키 값을 입력해주세요");
			return;
		}
		
		_text.text = LocalizationManager.Instance.GetString(key);
	}

	private void OnDisable()
	{
		LocalizationManager.OnLanguageChanged -= OnLanguageChanged; // 언어 변경 이벤트 해제
	}

	private void OnLanguageChanged()
	{
		_text.text = LocalizationManager.Instance.GetString(key);
	}
}
