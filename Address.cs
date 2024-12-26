using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GreyMagic;
using PostNamazu.Common;
using static LatihasExport.Utils;

namespace LatihasExport;

public static class Address {
	private struct AddrPattern {
		internal const string PlayerStateBaseAddress = "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 75 06 F6 43 18 02",
			AchievementBaseAddress = "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 04 30 FF C3",
			RecipeNoteBaseAddress = "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 41 39 86 ?? ?? ?? ??",
			InventoryManagerBaseAddress = "48 8D 0D ?? ?? ?? ?? 81 C2";
	}

	internal abstract class BaseAddress {
		protected readonly IntPtr base_addr;
		private readonly ExternalProcessMemory memory;
		private readonly int rel_offset;
		internal Dictionary<string, string> outStrDic = new();
		internal readonly SigScanner scanner;

		internal T EasyExecFunc<T>(string pattern, params object[] args) where T : struct {
			var p = scanner.ScanText(pattern);
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

		internal BaseAddress(string pattern, SigScanner scanner, ExternalProcessMemory memory, int rel_offset = 3) {
			this.memory = memory;
			this.scanner = scanner;
			this.rel_offset = rel_offset;
			base_addr = scanner.ScanText(pattern);
		}

		protected abstract void UpdateAll();

		// ReSharper disable once UnusedMember.Local
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


		internal string EasyStr<T>(int bias) where T : struct {
			return getValue<T>(bias).ToString();
		}

		internal string EasyStr<T>(int bias, int length, bool isStr = false) where T : struct {
			return isStr
				? Encoding.UTF8.GetString(getArray<byte>(bias, length).ToArray()).TrimEnd('\0')
				: toString(getArray<T>(bias, length));
		}

		// ReSharper disable once UnusedMember.Local
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

	internal class InventoryManagerAddress : BaseAddress {

		internal InventoryManagerAddress(SigScanner scanner, ExternalProcessMemory memory)
			: base(AddrPattern.InventoryManagerBaseAddress, scanner, memory) {
	
		}

		protected override void UpdateAll() {
			outStrDic = new Dictionary<string, string> {
				["WolfMarks"] =EasyExecFunc<uint>("E9 ?? ?? ?? ?? 8B CB E8 ?? ?? ?? ?? 84 C0 74 16").ToString() ,
				// ["RetainerGil"] = EasyStr<uint>(0xB8 + 0x428, 8)
			};
		}
	}

	internal class PlayerStateAddress : BaseAddress {
		internal List<byte> _caughtFishBitmask, _caughtSpearfishBitmask;

		internal PlayerStateAddress(SigScanner scanner, ExternalProcessMemory memory)
			: base(AddrPattern.PlayerStateBaseAddress, scanner, memory) {
			Update();
		}

		private void Update() {
			_caughtFishBitmask = getArray<byte>(0x3EC - 2, 180);
			_caughtSpearfishBitmask = getArray<byte>(0x4B1 - 1, 39);
		}

		protected override void UpdateAll() {
			Update();
			var QuestSpecialFlags = getValue<byte>(0x157);
			outStrDic = new Dictionary<string, string> {
				["IsLoaded"] = EasyStr<byte>(0x00),
				["_characterName"] = EasyStr<byte>(0x01, 64, true),
				["_onlineId"] = EasyStr<byte>(0x41, 17, true),
				["EntityId"] = EasyStr<uint>(0x64),
				["ContentId"] = EasyStr<ulong>(0x68),
				["_penaltyTimestamps"] = EasyStr<int>(0x70, 2),
				["MaxLevel"] = EasyStr<byte>(0x79),
				["MaxExpansion"] = EasyStr<byte>(0x7A),
				["Sex"] = EasyStr<byte>(0x7B),
				["Race"] = EasyStr<byte>(0x7C),
				["Tribe"] = EasyStr<byte>(0x7D),
				["CurrentClassJobId"] = EasyStr<byte>(0x7E),
				["CurrentClassJobRow"] = EasyStr<nint>(0x80),
				["CurrentLevel"] = EasyStr<short>(0x88),
				["_classJobLevels"] = EasyStr<short>(0x8A, 32),
				["_classJobExperience"] = EasyStr<int>(0xCC, 32),
				["SyncedLevel"] = EasyStr<short>(0x14C),
				["IsLevelSynced"] = EasyStr<byte>(0x14E),
				["HasPremiumSaddlebag"] = EasyStr<bool>(0x14F),
				["GuardianDeity"] = EasyStr<byte>(0x152),
				["BirthMonth"] = EasyStr<byte>(0x153), //?
				["BirthDay"] = EasyStr<byte>(0x154),
				["FirstClass"] = EasyStr<byte>(0x155),
				["StartTown"] = EasyStr<byte>(0x156),
				["QuestSpecialFlags"] = QuestSpecialFlags.ToString(),
				["GrandCompany"] = EasyStr<byte>(0x2B0),
				["GCRankMaelstrom"] = EasyStr<byte>(0x2B1),
				["GCRankTwinAdders"] = EasyStr<byte>(0x2B2),
				["GCRankImmortalFlames"] = EasyStr<byte>(0x2B3),
				["HomeAetheryteId"] = EasyStr<ushort>(0x2B4),
				["FavouriteAetheryteCount"] = EasyStr<byte>(0x2B6),
				["_favouriteAetherytes"] = EasyStr<ushort>(0x2B8, 4),
				["FreeAetheryteId"] = EasyStr<ushort>(0x2C0),
				["FreeAetherytePlayStationPlus"] = EasyStr<ushort>(0x2C2),
				["BaseRestedExperience"] = EasyStr<uint>(0x2C4),
				["_unlockedMountsBitmask"] = EasyStr<byte>(0x2DD, 38),
				["_unlockedOrnamentsBitmask"] = EasyStr<byte>(0x303, 7),
				["_unlockedGlassesStylesBitmask"] = EasyStr<byte>(0x30A, 4),
				["NumOwnedMounts"] = EasyStr<ushort>(0x30E),
				["_unlockedSpearfishingNotebookBitmask"] = EasyStr<byte>(0x3C2, 8),
				["_caughtFishBitmask"] = toString(_caughtFishBitmask),
				["NumFishCaught"] = EasyStr<uint>(0x4A0),
				["FishingBait"] = EasyStr<uint>(0x4A4),
				["_caughtSpearfishBitmask"] = toString(_caughtSpearfishBitmask),
				["NumSpearfishCaught"] = EasyStr<uint>(0x4D8),
				["UnknownUnixTimestamp"] = EasyStr<int>(0x4DC),
				["DeliveryLevel"] = EasyStr<byte>(0x5BD),
				["ActiveGcArmyExpedition"] = EasyStr<ushort>(0x5CC),
				["ActiveGcArmyTraining"] = EasyStr<ushort>(0x5CE),
				["MentorVersion"] = EasyStr<ushort>(0x7DC),
				["_desynthesisLevels"] = EasyStr<uint>(0x7E0, 8),
				["IsLegacy"] = ((QuestSpecialFlags & 1) != 0).ToString(),
				["IsWarriorOfLight"] = ((QuestSpecialFlags & 2) != 0).ToString()
			};
		}
	}

	internal class RecipeNoteAddress : BaseAddress {
		internal RecipeNoteAddress(SigScanner scanner, ExternalProcessMemory memory)
			: base(AddrPattern.RecipeNoteBaseAddress, scanner, memory) {
		}


		internal Dictionary<uint, bool> IsRecipeComplete(List<uint> recipeIds) {
			var p = scanner.ScanText("40 53 48 83 EC 20 8B D9 81 F9");
			Dictionary<uint, bool> res = new();
			var iter = 0;
			foreach (var recipeId in recipeIds) {
				if (!Main.Alive[0]) break;
				if (iter++ % 250 == 0) Main.Log($"Processing:[{iter}/{recipeIds.Count}]");
				res[recipeId] = EasyExecFunc<bool>(p, recipeId);
			}
			return res;
		}

		protected override void UpdateAll() {
			outStrDic = new Dictionary<string, string> {
				["_jobs"] = EasyStr<uint>(0x00, 8),
				["SelectedIndex"] = EasyStr<uint>(0xB8 + 0x428, 8)
			};
		}
	}

	internal class AchievementAddress : BaseAddress {
		internal List<byte> _completedAchievements;
		internal AchievementState State;

		internal AchievementAddress(SigScanner scanner, ExternalProcessMemory memory)
			: base(AddrPattern.AchievementBaseAddress, scanner, memory) {
			Update();
		}

		private void Update() {
			_completedAchievements = getArray<byte>(0x0C, 488);
			State = ParseAchievementState(getValue<int>(0x08));
		}

		private static AchievementState ParseAchievementState(int s) {
			return s switch {
				1 => AchievementState.Requested,
				2 => AchievementState.Loaded,
				_ => AchievementState.Invalid
			};
		}

		private string EasyStrA(int bias) {
			return ParseAchievementState(getValue<int>(bias)).ToString();
		}

		protected override void UpdateAll() {
			Update();
			outStrDic = new Dictionary<string, string> {
				["State"] = State.ToString(),
				["_completedAchievements"] = toString(_completedAchievements),
				["ProgressRequestState"] = EasyStrA(0x218),
				["ProgressAchievementId"] = EasyStr<uint>(0x21C),
				["ProgressCurrent"] = EasyStr<uint>(0x220),
				["ProgressMax"] = EasyStr<uint>(0x224)
			};
		}

		internal enum AchievementState {
			Invalid,
			Requested,
			Loaded
		}
	}
}