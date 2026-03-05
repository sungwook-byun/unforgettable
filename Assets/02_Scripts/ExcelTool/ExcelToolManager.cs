#if UNITY_EDITOR

using UnityEngine;

public class ExcelToolManager
{
	private static ExcelToolManager _instance = new ExcelToolManager();
	public static ExcelToolManager Instance => _instance;
	
	private FileIOController _fileIOController = new FileIOController();
	public FileIOController FileIOController => _fileIOController;
	private ExcelConverter _excelConverter = new ExcelConverter();
	public ExcelConverter ExcelConverter => _excelConverter;
}
#endif