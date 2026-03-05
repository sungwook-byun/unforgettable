using UnityEngine;

public class ExcelRowData
{
    protected bool TryParse(string raw, out string ret)
    {
        ret = raw;
        return true;
    }
		
    protected bool TryParse(string raw, out int ret)
    {
        return int.TryParse(raw, out ret);
    }
		
    protected bool TryParse(string raw, out float ret)
    {
        return float.TryParse(raw, out ret);
    }
		
    protected bool TryParse(string raw, out double ret)
    {
        return double.TryParse(raw, out ret);
    }
		
    protected bool TryParse(string raw, out bool ret)
    {
        return bool.TryParse(raw, out ret);
    }
		
    protected bool TryParse(string raw, out long ret)
    {
        return long.TryParse(raw, out ret);
    }
}
