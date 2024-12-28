namespace LatihasExport.Core;

using System.Collections.Generic;
using System.Text;

public static class Utils {
	public static string toString<T>(ICollection<T> list) {
		var sb = new StringBuilder();
		foreach (var v in list) sb.Append(v).Append(',');
		return sb.ToString();
	}
}