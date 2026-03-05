#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class ExcelConverter
{
	// private enum DBType
	// {
	// 	None = 0,
	// 	Visible
	// }
	
	private const int nameRow = 0; // 칼럼 이름
	private const int typeRow = 1; // 칼럼 자료형
	// private const int enumRow = 1; // 칼럼 변환처리 타입
	private const int valueStartRow = 2; // 데이터 시작 행
	
	public void GenerateCSharpFiles(string csPath)
	{
		string[] selectedExcelPaths = ExcelToolManager.Instance.FileIOController.LoadSelectPath();
		
		for (int i = 0; i < selectedExcelPaths.Length; i++)
		{
			if (i + 1 < selectedExcelPaths.Length)
				UpdateProgressBar(i + 1, selectedExcelPaths.Length, "");
			else
				ClearProgressBar();

			var excelFilePath = selectedExcelPaths[i];
			if (!IsExcelFileSupported(excelFilePath))
				continue;

			string fileName = Path.GetFileNameWithoutExtension(excelFilePath);
			TableData tableData = ExcelToolManager.Instance.FileIOController.GetTableData(excelFilePath);
			// TableData sortedTableData = SortTableColumn(tableData);

			string content = ToCSharp(tableData, fileName);
			
			var cSharpFileName = Path.GetFileNameWithoutExtension(fileName) + ".cs";
			var cSharpMetaFileName = Path.GetFileNameWithoutExtension(fileName) + ".cs.meta";
			
			ExcelToolManager.Instance.FileIOController.ClearFile(csPath, cSharpFileName);
			ExcelToolManager.Instance.FileIOController.ClearFile(csPath, cSharpMetaFileName);
			ExcelToolManager.Instance.FileIOController.WriteFile(content, csPath, cSharpFileName);
		}

		EditorPrefs.SetBool(ExcelToolStrDef.Key_MakeJson, true);
		bool isDone = EditorUtility.DisplayDialog("알림", "Table Class가 모두 업데이트 되었습니다.\n 컴파일 진행 및 Json생성을 시작합니다", "확인");
		if (isDone)
			AssetDatabase.Refresh();
	}

	private string ToCSharp(TableData tableData, string fileName)
	{
		try
		{
			var rowDataClassName = fileName;
			var csFile = new StringBuilder(2048);
			csFile.Append("//------------------------------------------------------------------------------\n");
			csFile.Append("//     이 스크립트는 자동으로 생성되었습니다.\n");
			csFile.Append("//     코드 수정하지 마세요!!\n");
			csFile.Append("//------------------------------------------------------------------------------");
			csFile.Append("\nusing System;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n");
			csFile.Append("\t[Serializable]\n");
			csFile.Append($"\tpublic class {rowDataClassName} : ExcelRowData\n");
			csFile.Append("\t{\n");

			var columnCount = tableData.columnCount;

			for (var col = 0; col < columnCount; col++)
			{
				//칼럼이름
				var rowColumnName = tableData.Get(nameRow, col);
				var rowColumnType = tableData.Get(typeRow, col);
				rowColumnType = Regex.Replace(rowColumnType, @"\d", "");
				if (rowColumnType == "str")
					rowColumnType = "string";

				var fieldName = "_" + rowColumnName;
				var fieldType = rowColumnType.Trim();

				csFile.Append("\t\t[SerializeField]\n");
				csFile.AppendFormat("\t\tprivate {0} {1};\n", fieldType, fieldName);
				csFile.AppendFormat("\t\tpublic {0} {1} {{ get {{ return {2}; }} set{{{2}=value; }} }}\n\n", fieldType,
					rowColumnName, fieldName);
			}

			csFile.AppendFormat("\n\t\tpublic {0}()\n", rowDataClassName);
			csFile.Append("\t\t{\n");
			csFile.Append("\t\t}\n");
			csFile.Append("\n#if UNITY_EDITOR\n");
			csFile.AppendFormat("\t\tpublic {0}(List<List<string>> sheet, int row, int column)\n", rowDataClassName);
			csFile.Append("\t\t{\n");

			for (var col = 0; col < columnCount; col++)
			{
				//칼럼이름
				var rawColumnName = tableData.Get(nameRow, col);
				var fieldName = "_" + rawColumnName;
				csFile.Append("\t\t\tTryParse(sheet[row][column++], out " + fieldName + ");\n");
			}

			csFile.Append("\t\t}\n#endif\n");
			csFile.Append("\t}\n\n");

			return csFile.ToString();
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
		}

		return "";
	}

	public void GenerateJsonFiles(string jsonPath, string csPath)
	{
		string[] selectedExcelPaths = ExcelToolManager.Instance.FileIOController.LoadSelectPath();

		var count = 0;
		for (var i = 0; i < selectedExcelPaths.Length; ++i)
		{
			var filePath = selectedExcelPaths[i];
			

			EditorUtility.DisplayCancelableProgressBar("Json 생성", $"{filePath} Json 생성중", 100);
			if (ToJson(filePath, jsonPath, csPath))
				count++;
		}

		EditorUtility.DisplayDialog("알림", $"{count}개의 Json 파일이 모두 생성되었습니다.", "확인");
		EditorApplication.delayCall += () => AssetDatabase.Refresh();
	}

	private bool ToJson(string excelFilePath, string jsonPath, string csPath)
	{
		string fileName = Path.GetFileNameWithoutExtension(excelFilePath);
		TableData tableData = ExcelToolManager.Instance.FileIOController.GetTableData(excelFilePath);
		// TableData sortedTableData = SortTableColumn(tableData);

		var csFileName = fileName + ".cs";
		if (!File.Exists(csPath + "/" + csFileName))
		{
			Debug.Log($"{csFileName} 스크립트가 존재하지 않아서 Json을 생성하지 않았습니다");
			return false;
		}

		var dataCollect = new List<ExcelRowData>();
		var className = fileName;
		var dataType = Type.GetType(className);
		if (dataType == null)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				dataType = assembly.GetType(className);
				if (dataType != null)
					break;
			}
		}

		if (dataType == null)
		{
			Debug.LogError($"{className} 클래스가 존재하지 않아서 Json을 생성하지 않았습니다");
			return false;
		}

		var dataCtor = dataType.GetConstructor(new[] { typeof(List<List<string>>), typeof(int), typeof(int) });
		if (dataCtor == null)
		{
			Debug.LogError($"{dataType}의 생성자가 존재하지 않아서 Json을 생성하지 않았습니다");
			return false;
		}

		for (var row = valueStartRow; row < tableData.rowCount; ++row)
		{
			var inst = dataCtor.Invoke(new object[] { tableData.table, row, 0 }) as ExcelRowData;
			if (inst == null)
				continue;

			dataCollect.Add(inst);
		}

		CreateJson(fileName, dataCollect, jsonPath);
		return true;
	}

	private static void CreateJson(string fileName, List<ExcelRowData> dataCollect, string jsonPath)
	{
		var json = JsonConvert.SerializeObject(dataCollect, Formatting.Indented);

		ExcelToolManager.Instance.FileIOController.WriteFile(json, jsonPath, fileName + ".json");
	}


	private bool isDisplayingProgress;

	private void UpdateProgressBar(int progress, int progressMax, string desc)
	{
		var title = "cs 스크립트 생성중... [" + progress + " / " + progressMax + "]";
		var value = progress / (float)progressMax;
		EditorUtility.DisplayProgressBar(title, desc, value);
		isDisplayingProgress = true;
	}

	private void ClearProgressBar()
	{
		if (!isDisplayingProgress) return;
		try
		{
			EditorUtility.ClearProgressBar();
		}
		catch (Exception)
		{
		}

		isDisplayingProgress = false;
	}

	public bool IsExcelFileSupported(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
			return false;
		var fileName = Path.GetFileName(filePath);
		if (fileName.Contains("~$"))
			return false;
		var lower = Path.GetExtension(filePath).ToLower();
		return lower == ".xlsx" || lower == ".xls" || lower == ".xlsm";
	}

	// private TableData SortTableColumn(TableData tableData)
	// {
	// 	TableData tempTableData = tableData;
	// 	
	// 	for (int i = tempTableData.columnCount - 1; i >= 0; i--)
	// 	{
	// 		var typeStr = tableData.table[enumRow][i];
	// 		var typeIndex = int.Parse(typeStr);
	// 		var type = (DBType)typeIndex;
	// 		if (type == DBType.None)
	// 		{
	// 			for (int j = tempTableData.rowCount - 1; j >= 0; j--)
	// 			{
	// 				tempTableData.table[j].RemoveAt(i);
	// 			}
	// 			
	// 			tempTableData.columnCount--;
	// 		}
	// 	}
	//
	// 	return tempTableData;
	// }
}

#endif