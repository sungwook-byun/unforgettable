using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI Localized Text 커스텀 인스펙터에 사용될 스트링테이블 데이터입니다.
/// 런타임에서는 사용되지 않습니다.
/// </summary>
public class LocalizationSO : ScriptableObject
{
	public List<StringTable> StringTables;
}
