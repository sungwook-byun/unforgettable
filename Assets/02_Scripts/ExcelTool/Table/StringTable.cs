//------------------------------------------------------------------------------
//     이 스크립트는 자동으로 생성되었습니다.
//     코드 수정하지 마세요!!
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

	[Serializable]
	public class StringTable : ExcelRowData
	{
		[SerializeField]
		private string _Str_Index;
		public string Str_Index { get { return _Str_Index; } set{_Str_Index=value; } }

		[SerializeField]
		private string _KR_str;
		public string KR_str { get { return _KR_str; } set{_KR_str=value; } }

		[SerializeField]
		private string _EN_str;
		public string EN_str { get { return _EN_str; } set{_EN_str=value; } }

		[SerializeField]
		private string _DU_str;
		public string DU_str { get { return _DU_str; } set{_DU_str=value; } }

		[SerializeField]
		private string _TR_str;
		public string TR_str { get { return _TR_str; } set{_TR_str=value; } }


		public StringTable()
		{
		}

#if UNITY_EDITOR
		public StringTable(List<List<string>> sheet, int row, int column)
		{
			TryParse(sheet[row][column++], out _Str_Index);
			TryParse(sheet[row][column++], out _KR_str);
			TryParse(sheet[row][column++], out _EN_str);
			TryParse(sheet[row][column++], out _DU_str);
			TryParse(sheet[row][column++], out _TR_str);
		}
#endif
	}


