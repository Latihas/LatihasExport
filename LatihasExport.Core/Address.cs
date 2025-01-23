using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LatihasExport.Core;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GreyMagic;
using PostNamazu.Common;
using static Utils;

public static class Address {
	internal class RecipeNoteAddress : BaseAddress {
		public RecipeNoteAddress(string n, int rel_offset = 3) : base(n, rel_offset) {
		}

		//
		// internal Dictionary<uint, bool> IsRecipeComplete(List<uint> recipeIds) {
		// 	var p = scanner.ScanText("40 53 48 83 EC 20 8B D9 81 F9");
		// 	Dictionary<uint, bool> res = new();
		// 	var iter = 0;
		// 	foreach (var recipeId in recipeIds.TakeWhile(_ => Main.Alive[0])) {
		// 		if (iter++ % 250 == 0) Main.Log($"Processing:[{iter}/{recipeIds.Count}]");
		// 		res[recipeId] = EasyExecFunc<bool>(p, recipeId);
		// 	}
		// 	return res;
		// }

		protected override void UpdateAll() {
			outStrDic = UpdateAllField();
		}
	}

	internal abstract class BaseAddress {
		internal IntPtr base_addr;
		private readonly ExternalProcessMemory memory;
		private readonly int rel_offset;
		internal Dictionary<string, string> outStrDic = new();
		private readonly SigScanner scanner;
		internal string[] StrFields = { }, BitMaskFields = { };

		internal Dictionary<string, string> UpdateAllField() {
			var res = new Dictionary<string, string>();
			foreach (var obj in json["Vars"]!) {
				var jProperty = obj as JProperty;
				var name = jProperty!.Name;
				var value = GA(name);
				if (StrFields.Contains(name)) res[name] = Encoding.UTF8.GetString(value.ToArray()).TrimEnd('\0');
				else if (BitMaskFields.Contains(name)) res[name] = toMemoryArrayString(GA(name));
				else res[name] = toString(GA(name));
			}
			return res;
		}

		internal dynamic GA(string field, int extend = 0) {
			try {
				var info = json["Vars"]![field]!;
				var len = int.Parse(info["ArraySize"]!.ToString()) + extend;
				var offset = int.Parse(info["Offset"]!.ToString());
				var type = info["Type"]!.ToString();
				return type switch {
					"uint" => getArray<uint>(offset, len),
					"int" => getArray<int>(offset, len),
					"AchievementState" => getArray<int>(offset, len),
					"byte" => getArray<byte>(offset, len),
					"short" => getArray<short>(offset, len),
					"nint" => getArray<nint>(offset, len),
					"ushort" => getArray<ushort>(offset, len),
					"ulong" => getArray<ulong>(offset, len),
					"bool" => getArray<bool>(offset, len),
					_ => throw new Exception($"{type} Not Supported")
				};
			}
			catch (Exception e) {
				Main.Log(e);
			}
			return new List<byte>();
		}

		internal List<byte> GetBitMask(string field) {
			var res = new List<byte>();
			foreach (var b in GA(field, 20)) res.Add(Convert.ToByte(b));
			return res;
		}

		private static string ToMemory(long i) {
			return Convert.ToString(i, 16);
		}

		internal string toMemoryArrayString(int bias, int length, int begin = 0) {
			return toMemoryArrayString(getArray<byte>(bias, length, begin));
		}

		internal static string toMemoryArrayString(List<byte> bytes) {
			var sb = new StringBuilder();
			foreach (var b in bytes)
				sb.Append($"{b:X2} ");
			return sb.Remove(sb.Length - 1, 1).ToString();
		}

		internal string ToMemory(IntPtr i) {
			return ToMemory((long)i);
		}

		internal T EasyExecFunc<T>(IntPtr p, params object[] args) where T : struct {
			var assemblyLock = memory.Executor.AssemblyLock;
			var flag = false;
			try {
				Monitor.Enter(assemblyLock, ref flag);
				return memory.CallInjected64<T>(p, args);
			}
			finally {
				if (flag) Monitor.Exit(assemblyLock);
			}
		}

		internal T EasyExecFunc<T>(string pattern, params object[] args) where T : struct {
			return EasyExecFunc<T>(scanner.ScanText(pattern), args);
		}

		internal readonly JObject json;

		internal BaseAddress(string n, int rel_offset = 3) {
			memory = Main.memory;
			scanner = Main.scanner;
			this.rel_offset = rel_offset;
			json = JObject.Parse(File.ReadAllText($"{Main.GENDIR}{n}.json"));
			base_addr = scanner.ScanText(json.GetValue("Pattern")!.ToString());
		}


		protected abstract void UpdateAll();

		// ReSharper disable once UnusedMember.Global
		internal IntPtr LegacyScan(string s, int bias = 4) {
			var x = scanner.ScanText(s);
			return x + rel_offset + bias + memory.Read<int>(x + rel_offset);
		}

		private IntPtr getIntPtr(int bias) {
			return base_addr + rel_offset + 4 + bias + memory.Read<int>(base_addr + rel_offset);
		}

		internal T getValue<T>(int bias) where T : struct {
			return memory.Read<T>(getIntPtr(bias));
		}

		internal List<T> getArray<T>(int bias, int length, int begin = 0) where T : struct {
			var res = new List<T>();
			var sz = Unsafe.SizeOf<T>();
			for (var i = begin * sz; i < length * sz; i += sz)
				res.Add(memory.Read<T>(getIntPtr(bias + i)));
			return res;
		}

		// private IntPtr getIntPtr(int bias, IntPtr base_addr) {
		// 	return base_addr + rel_offset + 4 + bias + memory.Read<int>(base_addr + rel_offset);
		// }
		//
		// internal T getValue<T>(int bias, IntPtr base_addr) where T : struct {
		// 	return memory.Read<T>(getIntPtr(bias, base_addr));
		// }
		// internal List<T> getArray<T>(int bias, int length, IntPtr base_addr, int begin = 0) where T : struct {
		// 	var res = new List<T>();
		// 	var sz = Unsafe.SizeOf<T>();
		// 	for (var i = begin * sz; i < length * sz; i += sz)
		// 		res.Add(memory.Read<T>(getIntPtr(bias + i,base_addr)));
		// 	return res;
		// }

		internal string EasyStr<T>(int bias) where T : struct {
			return getValue<T>(bias).ToString();
		}

		internal string EasyStr<T>(int bias, int length, bool isStr = false) where T : struct {
			return isStr
				? Encoding.UTF8.GetString(getArray<byte>(bias, length).ToArray()).TrimEnd('\0')
				: toString(getArray<T>(bias, length));
		}

		// ReSharper disable once UnusedMember.Global
		internal string EasyStr<T>(int bias, int length, int begin) where T : struct {
			return toString(getArray<T>(bias, length, begin));
		}

		public override string ToString() {
			UpdateAll();
			var sb = new StringBuilder();
			foreach (var kvp in outStrDic)
				sb.Append($"{kvp.Key,-40}: {kvp.Value}\r\n");
			return sb.ToString();
		}
	}


	internal class PlayerStateAddress : BaseAddress {
		internal List<byte> _caughtFishBitmask, _caughtSpearfishBitmask, _unlockedMountsBitmask, _unlockedOrnamentsBitmask, _unlockedGlassesStylesBitmask;


		public PlayerStateAddress(string n, int rel_offset = 3) : base(n, rel_offset) {
			StrFields = new[] { "_characterName", "_onlineId" };
			BitMaskFields = new[] {
				"_caughtFishBitmask", "_caughtSpearfishBitmask", "_unlockedMountsBitmask",
				"_unlockedOrnamentsBitmask", "_unlockedGlassesStylesBitmask"
			};
			Update();
		}


		private void Update() {
			_caughtFishBitmask = GetBitMask("_caughtFishBitmask");
			_caughtSpearfishBitmask = GetBitMask("_caughtSpearfishBitmask");
			_unlockedMountsBitmask = GetBitMask("_unlockedMountsBitmask");
			_unlockedOrnamentsBitmask = GetBitMask("_unlockedOrnamentsBitmask");
			_unlockedGlassesStylesBitmask = GetBitMask("_unlockedGlassesStylesBitmask");
			outStrDic["_caughtFishBitmask"] =toMemoryArrayString(_caughtFishBitmask) ;
			outStrDic["_caughtSpearfishBitmask"] =toMemoryArrayString(_caughtSpearfishBitmask) ;
			outStrDic["_unlockedMountsBitmask"] =toMemoryArrayString(_unlockedMountsBitmask) ;
			outStrDic["_unlockedOrnamentsBitmask"] =toMemoryArrayString(_unlockedOrnamentsBitmask) ;
			outStrDic["_unlockedGlassesStylesBitmask"] =toMemoryArrayString(_unlockedGlassesStylesBitmask) ;

		}

		private static readonly string[] ClassJobs = {
			"冒险", "剑术", "格斗", "斧术", "枪术", "弓术", "幻术", "咒术",
			"刻木", "锻铁", "铸甲", "雕金", "制革", "裁衣", "炼金", "烹调", "采矿", "园艺", "捕鱼",
			"骑士", "武僧", "战士", "龙骑", "诗人", "白魔", "黑魔",
			"秘术", "召唤", "学者",
			"双剑", "忍者",
			"机工", "黑骑", "占星",
			"武士", "赤魔", "青魔",
			"绝枪", "舞者",
			"钐镰", "贤者",
			"蝰蛇", "绘灵"
		};
		private static readonly Dictionary<string, int> ClassJobLevelMap = new() {
			["格斗"] = 0,
			["武僧"] = 0,
			["剑术"] = 1,
			["骑士"] = 1,
			["斧术"] = 2,
			["战士"] = 2,
			["弓术"] = 3,
			["诗人"] = 3,
			["枪术"] = 4,
			["龙骑"] = 4,
			["咒术"] = 5,
			["黑魔"] = 5,
			["幻术"] = 6,
			["白魔"] = 6,
			["刻木"] = 7,
			["锻铁"] = 8,
			["铸甲"] = 9,
			["雕金"] = 10,
			["制革"] = 11,
			["裁衣"] = 12,
			["炼金"] = 13,
			["烹调"] = 14,
			["采矿"] = 15,
			["园艺"] = 16,
			["捕鱼"] = 17,
			["秘术"] = 18,
			["召唤"] = 18,
			["学者"] = 18,
			["双剑"] = 19,
			["忍者"] = 19,
			["机工"] = 20,
			["黑骑"] = 21,
			["占星"] = 22,
			["武士"] = 23,
			["赤魔"] = 24,
			["青魔"] = 25,
			["绝枪"] = 26,
			["舞者"] = 27,
			["钐镰"] = 28,
			["贤者"] = 29,
			["蝰蛇"] = 30,
			["绘灵"] = 31,
		};

		protected override void UpdateAll() {
			outStrDic = UpdateAllField();
			Update();
			var _classJobLevels = GA("_classJobLevels");
			var _desynthesisLevels = GA("_desynthesisLevels");
			for (var i = 0; i < ClassJobs.Length; i++) {
				var detail = new StringBuilder();
				var name = ClassJobs[i];
				if (ClassJobLevelMap.TryGetValue(name, out var value))
					detail.Append("等级[").Append(_classJobLevels[value]).Append("], ");
				if (i is >= 8 and <= 15) {
					var d = _desynthesisLevels[i - 8];
					detail.Append("分解等级[").Append(d / 100).Append('.').Append(d % 100).Append("], ");
				}
				outStrDic[name] = detail.ToString();
			}
		}
	}

	internal class QuestManagerAddress : BaseAddress {
		public QuestManagerAddress(string n, int rel_offset = 3) : base(n, rel_offset) {
			Update();
		}

		private void Update() {
			_completedRecipesBitmask = new List<byte>();
			foreach (var b in getArray<byte>(0x8FC, 800)) {
				//Wait For Update
				var tmp = b;
				byte result = 0;
				for (var i = 0; i < 8; i++) {
					result = (byte)(result << 1 | tmp & 1);
					tmp >>= 1;
				}
				_completedRecipesBitmask.Add(result);
			}
			outStrDic["_completedRecipesBitmask"] = toMemoryArrayString(_completedRecipesBitmask);
		}

		internal List<byte> _completedRecipesBitmask;

		protected override void UpdateAll() {
			outStrDic = UpdateAllField();
			Update();
		}
	}

	internal class AchievementAddress : BaseAddress {
		internal List<byte> _completedAchievements;

		public AchievementAddress(string n, int rel_offset = 3) : base(n, rel_offset) {
			BitMaskFields = new[] { "_completedAchievements" };
			Update();
		}

		private void Update() {
			_completedAchievements = GetBitMask("_completedAchievements");
		}

		protected override void UpdateAll() {
			outStrDic = UpdateAllField();
			outStrDic["StateParsed"] = outStrDic["State"] switch {
				"1" => "Requested",
				"2" => "Loaded",
				_ => "Invalid"
			};
			Update();
		}
	}

	internal class InventoryManagerAddress : BaseAddress {
		public InventoryManagerAddress(string n, int rel_offset = 3) : base(n, rel_offset) {
		}

		private void Update() {
		}

		protected override void UpdateAll() {
			outStrDic = UpdateAllField();
			Update();
		}
	}
}