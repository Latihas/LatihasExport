using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace LatihasExport.Generator;

using ICSharpCode.Decompiler;

internal static class Program {
	private class CLS {
		private record Var(int Offset, string Name, string Type, int ArraySize) {
			internal readonly int Offset = Offset;
			internal readonly string Name = Name;
			internal readonly string Type = Type;
			internal readonly int ArraySize = ArraySize;

			public override string ToString() {
				var sb = new StringBuilder();
				sb.Append('"').Append(Name).Append('"').Append(':');
				sb.Append('{');
				sb.Append("\"Offset\":").Append(Offset).Append(',');
				sb.Append("\"Type\":").Append('"').Append(Type).Append('"').Append(',');
				sb.Append("\"ArraySize\":").Append(ArraySize);
				sb.Append('}');
				return sb.ToString();
			}
		}

		private readonly string FullTypeName;
		internal readonly string Name;

		private readonly string Pattern;
		private readonly List<Var> vars = new();

		internal CLS(CSharpDecompiler decompiler, string ftn) {
			FullTypeName = ftn;
			var rawcode = decompiler.DecompileTypeAsString(new FullTypeName(ftn));
			var code = rawcode.Split("\r\n");
			Name = ftn[(ftn.LastIndexOf('.') + 1)..];
			File.WriteAllText($"{Name}.cs", rawcode);
			for (var i = 0; i < code.Length; i++) {
				var line = code[i];
				if (!line.StartsWith('\t') || line.StartsWith("\t\t")) continue;
				line = line.Trim();
				if (line.Contains(Name + "* Instance()")) {
					line = code[i - 1].Trim();
					Pattern = line[(line.IndexOf('"') + 1)..line.LastIndexOf('"')];
					continue;
				}
				if (line.StartsWith("[FieldOffset(")) {
					var offset = int.Parse(line["[FieldOffset(".Length..line.IndexOf(')')]);
					var isArray = code[++i].Contains('[');
					if (isArray) i++;
					line = code[i].Trim();
					var field = line.Split();
					if (field.Length == 3) {
						var rawtype = field[1];
						var asize = 1;
						if (isArray) {
							asize = int.Parse(rawtype["FixedSizeArray".Length..rawtype.IndexOf('<')]);
							rawtype = rawtype[(rawtype.IndexOf('<') + 1)..rawtype.IndexOf('>')];
						}
						vars.Add(new Var(offset, field[2][..^1], rawtype, asize));
					}
					else { Console.WriteLine("[Info] UnHandled Field: " + line); }
					continue;
				}
			}
		}
		// File.WriteAllText("1.txt", code);


		public override string ToString() {
			var sb = new StringBuilder("{");
			sb.Append("\"FullTypeName\":").Append('"').Append(FullTypeName).Append('"').Append(',');
			sb.Append("\"Name\":").Append('"').Append(Name).Append('"').Append(',');
			sb.Append("\"Pattern\":").Append('"').Append(Pattern).Append('"').Append(',');
			sb.Append("\"Vars\":").Append('{');
			foreach (var v in vars) sb.Append(v).Append(',');
			if (vars.Count != 0) sb.Remove(sb.Length - 1, 1);
			sb.Append("}}");
			return sb.ToString();
		}
	}

	private static void DumpJson(string ftn) {
		var tmp = new CLS(decompiler, ftn);
		File.WriteAllText("../../../LatihasExport/Generated/" + tmp.Name + ".json", tmp.ToString());
	}

	private static CSharpDecompiler decompiler;

	public static void Main() {
		var fp = Environment.GetEnvironmentVariable("userprofile") + "/AppData/Roaming/XIVLauncherCN/addon/Hooks/dev/FFXIVClientStructs.dll";
		decompiler = new CSharpDecompiler(fp, new DecompilerSettings());
		DumpJson("FFXIVClientStructs.FFXIV.Client.Game.UI.PlayerState");
		DumpJson("FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement");
		DumpJson("FFXIVClientStructs.FFXIV.Client.Game.QuestManager");
		DumpJson("FFXIVClientStructs.FFXIV.Client.Game.UI.RecipeNote");
		DumpJson("FFXIVClientStructs.FFXIV.Client.Game.InventoryManager");
	}
}