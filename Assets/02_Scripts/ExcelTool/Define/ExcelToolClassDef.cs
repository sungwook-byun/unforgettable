
using System.Collections.Generic;

public class ExcelData // 에디터에서 보여줄 엑셀 파일 정보
{
    public bool check;
    public string excelFullPath;
	
    public ExcelData(bool check, string excelFullPath)
    {
        this.check = check;
        this.excelFullPath = excelFullPath;
    }
}

public class TableData // 엑셀 데이터 정보
{
    public List<List<string>> table;
    public int columnCount;
    public int rowCount;

    public TableData(int columnCount, int rowCount, List<List<string>> table)
    {
        this.columnCount = columnCount;
        this.rowCount = rowCount;
        this.table = table;
    }
	
    public string Get(int row, int column)
    {
        return table[row][column];
    }
}
