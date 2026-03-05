#if UNITY_EDITOR

using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using Newtonsoft.Json;
using UnityEngine;

public class FileIOController
{
	
	public void ClearDirectory(string directory)
	{
		if (Directory.Exists(directory))
		{
			Directory.Delete(directory, true);
		}
	}
	
	public void ClearFile(string filePath, string fileName)
	{
		string path = filePath + "/" + fileName;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}
	
	public string ReadFile(string filePath, string fileName)
	{
		var allStr = File.ReadAllText(filePath + "/" + fileName);
		return allStr;
	}
	
	public void WriteFile(string content, string filePath, string fileName)
	{
		if (!Directory.Exists(filePath))
			Directory.CreateDirectory(filePath);
		
		using (StreamWriter sw = File.CreateText(filePath + "/" + fileName))
			sw.WriteLine(content);
	}
	
	public TableData GetTableData(string excelFilePath)
	{
		using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
		using (var reader = ExcelReaderFactory.CreateReader(stream))
		{
			var result = reader.AsDataSet();
			var table = result.Tables[0];
			
			TableData tableData = ConvertToTable(table);
			
			return tableData;
		}
	}
	
	private TableData ConvertToTable(DataTable dataTable)
	{
		int columnCount = dataTable.Columns.Count;
		int rowCount = dataTable.Rows.Count;
		
		List<List<string>> table = new List<List<string>>();
	
		foreach (DataRow row in dataTable.Rows)
		{
			List<string> rowData = new List<string>();
			foreach (var cell in row.ItemArray)
			{
				rowData.Add(cell?.ToString() ?? string.Empty); // null 방지
			}
			table.Add(rowData);
		}
		
		TableData tableData = new TableData(columnCount, rowCount, table);
		return tableData;
	}

	public List<ExcelData> GetExcelFileList(string folderPath)
	{
		if (!Directory.Exists(folderPath))
		{
			Directory.CreateDirectory(folderPath);
		}
		
		var paths = Directory.GetFiles(folderPath);
		List<ExcelData> list = new List<ExcelData>();
		
		for (int i = 0; i < paths.Length; i++)
		{
			if (paths[i].Contains(".meta")) continue;
			if (paths[i].Contains("~$")) continue;
#if UNITY_EDITOR_OSX
			if (paths[i].Contains(".DS_Store")) continue;
#endif
			list.Add(new ExcelData(false, paths[i]));
		}
			
		return list;
	}

	public void SaveSelectPath(string[] paths)
	{
		var json = JsonConvert.SerializeObject(paths, Formatting.Indented);
		var dir = Application.persistentDataPath;
		string fileName = ExcelToolStrDef.FileName_ExcelSelect;
		ExcelToolManager.Instance.FileIOController.WriteFile(json, dir, fileName);
	}
	
	public string[] LoadSelectPath()
	{
		var dir = Application.persistentDataPath;
		string fileName = ExcelToolStrDef.FileName_ExcelSelect;
		var allStr = ExcelToolManager.Instance.FileIOController.ReadFile(dir, fileName);
		var data = JsonConvert.DeserializeObject<string[]>(allStr);
		return data;
	}
}

#endif