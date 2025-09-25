using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Achievement = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement;
using Action = System.Action;

namespace LatihasExport;

public class MainWindow() : Window("LatihasExport") {
	private const ImGuiTableFlags ImGuiTableFlag = ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg;

	private static IEnumerable<Item> _lRecipe;
	private static IEnumerable<uint> _lFishCaught, _lsFishCaught;
	private static IEnumerable<FishParameter> _lFishUnCaught;
	private static IEnumerable<SpearfishingItem> _lsFishUnCaught;
	private static IEnumerable<Lumina.Excel.Sheets.Achievement> _lAchievement;
	private static IEnumerable<TripleTriadCard> _lTripleTriadCard;
	private static IEnumerable<Leve> _lLeve;
	private static IEnumerable<Quest> _lQuest;
	private static IEnumerable<Orchestrion> _lOrchestrion;
	private static IEnumerable<Mount> _lMount;
	private static IEnumerable<Ornament> _lOrnament;
	private static IEnumerable<Glasses> _lGlasses;
	private static unsafe PlayerState* _playerStateInstance;
	private static unsafe Achievement* _achievementInstance;
	private static unsafe QuestManager* _questManagerInstance;
	private static bool _achievementLoaded;
	private readonly Configuration _configuration = Plugin.Configuration;

	private void MkDir() {
		try {
			Directory.CreateDirectory(_configuration.SavePath);
		}
		catch (Exception e) {
			Plugin.Log.Error($"创建目录失败: {e.Message}");
		}
	}

	private void WriteFile(string fn, string content) {
		MkDir();
		File.WriteAllText(Path.Combine(_configuration.SavePath, fn), content);
	}


	internal static unsafe void RefreshData() {
		try {
			_lRecipe = Plugin.DataManager.GameData.GetExcelSheet<Recipe>()!
				.Select(x => new {
					x,
					a = x.RowId
				})
				.Where(t => t.a < 30000 && !QuestManager.IsRecipeComplete(t.a))
				.Select(t => t.x.ItemResult.Value)
				.Where(t => t.Name != "");
			_playerStateInstance = PlayerState.Instance();
			_achievementInstance = Achievement.Instance();
			_questManagerInstance = QuestManager.Instance();
			_achievementLoaded = _achievementInstance->IsLoaded();
			var tmp = Plugin.DataManager.GameData.GetExcelSheet<FishParameter>()!
				.Where(i => i.IsInLog && i.Text != "").ToList();
			_lFishCaught = tmp.Where(i => _playerStateInstance->IsFishCaught(i.RowId)).Select(i => i.Item.RowId);
			_lFishUnCaught = tmp.Where(i => !_playerStateInstance->IsFishCaught(i.RowId));
			var tmp2 = Plugin.DataManager.GameData.GetExcelSheet<SpearfishingItem>()!.Where(i => i.IsVisible).ToList();
			_lsFishCaught = tmp2.Where(i => _playerStateInstance->IsSpearfishCaught(i.RowId)).Select(i => i.Item.RowId);
			_lsFishUnCaught = tmp2.Where(i => !_playerStateInstance->IsSpearfishCaught(i.RowId));
			if (_achievementLoaded)
				_lAchievement = Plugin.DataManager.GameData.GetExcelSheet<Lumina.Excel.Sheets.Achievement>()!
					.Where(i => !_achievementInstance->IsComplete((int)i.RowId) && i.Name != "" && !i.AchievementHideCondition.Value.HideAchievement &&
					            i.AchievementCategory.Value.AchievementKind.Value.RowId is not (13 or 8 or 0));
			_lTripleTriadCard = Plugin.DataManager.GameData.GetExcelSheet<TripleTriadCard>()!.Where(i => i.RowId > 0 && i.Name != "" && !UIState.Instance()->IsTripleTriadCardUnlocked((ushort)i.RowId));
			_lLeve = Plugin.DataManager.GameData.GetExcelSheet<Leve>()!.Where(i => !_questManagerInstance->IsLevequestComplete((ushort)i.RowId) && i.AllowanceCost != 0 && i is { RowId: > 0, GilReward: > 0 });
			_lQuest = Plugin.DataManager.GameData.GetExcelSheet<Quest>()!.Where(i =>
				!UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted((ushort)i.RowId + 0x10000u) && i.PlaceName.Value.Name != "" && i.JournalGenre.RowId > 0 && i.JournalGenre.Value.JournalCategory.Value.RowId != 96);
			_lOrchestrion = Plugin.DataManager.GameData.GetExcelSheet<Orchestrion>()!.Where(i =>
				!_playerStateInstance->IsOrchestrionRollUnlocked(i.RowId) && i.Name != "");
			_lMount = Plugin.DataManager.GameData.GetExcelSheet<Mount>()!.Where(i =>
				!_playerStateInstance->IsMountUnlocked(i.RowId) && i.Icon != 0);
			_lOrnament = Plugin.DataManager.GameData.GetExcelSheet<Ornament>()!.Where(i =>
				!_playerStateInstance->IsOrnamentUnlocked(i.RowId) && i.RowId > 0);
			_lGlasses = Plugin.DataManager.GameData.GetExcelSheet<Glasses>()!.Where(i =>
				!_playerStateInstance->IsGlassesUnlocked((ushort)i.RowId) && i.RowId > 0);
		}
		catch (Exception e) {
			Plugin.Log.Error(e.ToString());
		}
	}

	private static void Start(string cmd) => Process.Start(new ProcessStartInfo(cmd) {
		UseShellExecute = true
	});

	private static void NewTable<T>(string[] header, IEnumerable<T> data, Action<T>[] acts) {
		if (ImGui.BeginTable("Table", acts.Length, ImGuiTableFlag)) {
			foreach (var item in header) {
				if (item == "") ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 24);
				else if (item.Contains("序号")) ImGui.TableSetupColumn(item, ImGuiTableColumnFlags.WidthFixed, 96);
				else ImGui.TableSetupColumn(item);
			}
			ImGui.TableHeadersRow();
			foreach (var res in data) {
				ImGui.TableNextRow();
				for (var i = 0; i < acts.Length; i++) {
					ImGui.TableSetColumnIndex(i);
					acts[i](res);
				}
			}
			ImGui.EndTable();
		}
	}

	private static void NewTab(string tabname, Action act) {
		if (ImGui.BeginTabItem(tabname)) {
			act();
			ImGui.EndTabItem();
		}
	}

	private static void RenderIcon(int id) {
		if (Plugin.TextureProvider.TryGetFromGameIcon((uint)id, out var tex) && tex.TryGetWrap(out var icon, out _))
			ImGui.Image(icon.Handle, new Vector2(24, 24));
	}

	public override void Draw() {
		if (ImGui.InputText("保存路径", ref _configuration.SavePath, 114514)) _configuration.Save();
		ImGui.SameLine();
		if (ImGui.Button("打开输出目录")) {
			MkDir();
			Start(_configuration.SavePath);
		}
		if (ImGui.Button("刷新")) RefreshData();
		if (ImGui.BeginTabBar("tab")) {
			NewTab("制作笔记", () =>
				NewTable(["序号", "", "名称"], _lRecipe, [
					res => ImGui.Text(res.RowId.ToString()),
					res => RenderIcon(res.Icon),
					res => ImGui.Text(res.Name.ToString())
				]));
			NewTab("钓鱼", () => {
				if (ImGui.Button("导出到鱼糕")) {
					var sb = new StringBuilder("{\"completed\":[");
					foreach (var res in _lFishCaught) sb.Append(res).Append(',');
					foreach (var res in _lsFishCaught) sb.Append(res).Append(',');
					sb.Remove(sb.Length - 1, 1).Append("]}");
					WriteFile("fish.json", sb.ToString());
					Plugin.Log.Info("导完了");
				}
				ImGui.SameLine();
				if (ImGui.Button("打开鱼糕")) Start("https://fish.ffmomola.com/ng/#/wiki/fishing");
				NewTable(["序号", "鱼糕序号", "名称", "地点"], _lFishUnCaught, [
					res => ImGui.Text(res.RowId.ToString()),
					res => ImGui.Text(res.Item.RowId.ToString()),
					res => ImGui.Text(Plugin.DataManager.GetExcelSheet<Item>(Plugin.DataManager.Language).GetRowOrDefault(res.Item.RowId)?.Name.ToString()),
					res => ImGui.Text(res.FishingSpot.Value.PlaceName.Value.Name.ToString())
				]);
				NewTable(["序号", "鱼糕序号", "名称", "地点"], _lsFishUnCaught, [
					res => ImGui.Text(res.RowId.ToString()),
					res => ImGui.Text(res.Item.RowId.ToString()),
					res => ImGui.Text(Plugin.DataManager.GetExcelSheet<Item>(Plugin.DataManager.Language).GetRowOrDefault(res.Item.RowId)?.Name.ToString())
				]);
			});
			NewTab("成就", () => {
				if (_achievementLoaded)
					NewTable(["序号", "", "名称", "成就点", "分类"], _lAchievement, [
						res => ImGui.Text(res.RowId.ToString()),
						res => RenderIcon(res.Icon),
						res => ImGui.Text(res.Name.ToString()),
						res => ImGui.Text(res.Points.ToString()),
						res => ImGui.Text(res.AchievementCategory.Value.Name.ToString())
					]);
				else ImGui.Text("打开一次成就界面以刷新");
			});
			NewTab("幻卡", () => {
				if (ImGui.Button("导出到arrtripletriad.com")) {
					var sb = new StringBuilder("[false,");
					unsafe {
						var bitArray = new BitArray(UIState.Instance()->UnlockedTripleTriadCardsBitmask.ToArray());
						for (var l = 1; l < bitArray.Count; l++) sb.Append(bitArray[l - 1].ToString().ToLower()).Append(',');
					}
					sb.Remove(sb.Length - 1, 1).Append(']');
					WriteFile("ttc.json", sb.ToString());
					Plugin.Log.Info("导完了");
				}
				ImGui.SameLine();
				if (ImGui.Button("打开arrtripletriad.com")) Start("https://arrtripletriad.com/cn/huan-ka-yi-lan");
				NewTable(["序号", "名称"], _lTripleTriadCard, [
					res => ImGui.Text(res.RowId.ToString()),
					res => ImGui.Text(res.Name.ToString())
				]);
			});
			NewTab("理符", () =>
				NewTable(["序号", "职业", "等级", "名称", "NPC"], _lLeve, [
					res => ImGui.Text(res.RowId.ToString()),
					res => ImGui.Text(res.ClassJobCategory.Value.Name.ToString()),
					res => ImGui.Text(res.ClassJobLevel.ToString()),
					res => ImGui.Text(res.Name.ToString()),
					res => ImGui.Text(res.LeveClient.Value.Name.ToString())
				]));
			NewTab("任务", () =>
				NewTable(["序号", "等级", "", "名称", "地点", "分类1", "分类2", "分类3"], _lQuest, [
					res => ImGui.Text(res.RowId.ToString()),
					res => ImGui.Text(res.ClassJobLevel[0].ToString()),
					res => RenderIcon(res.JournalGenre.Value.Icon),
					res => ImGui.Text(res.Name.ToString()),
					res => ImGui.Text(res.PlaceName.Value.Name.ToString()),
					res => ImGui.Text(res.JournalGenre.Value.Name.ToString()),
					res => ImGui.Text(res.JournalGenre.Value.JournalCategory.Value.Name.ToString()),
					res => ImGui.Text(res.JournalGenre.Value.JournalCategory.Value.JournalSection.Value.Name.ToString())
				]));
			NewTab("乐谱", () =>
				NewTable(["序号", "名称"],
					_lOrchestrion, [
						res => ImGui.Text(res.RowId.ToString()),
						res => ImGui.Text(res.Name.ToString())
					]));
			NewTab("坐骑", () =>
				NewTable(["序号", "名称"], _lMount, [
					res => ImGui.Text(res.RowId.ToString()),
					res => RenderIcon(res.Icon),
					res => ImGui.Text(res.Singular.ToString())
				]));
			NewTab("装饰", () =>
				NewTable(["序号", "名称"], _lOrnament, [
					res => ImGui.Text(res.RowId.ToString()),
					res => RenderIcon(res.Icon),
					res => ImGui.Text(res.Singular.ToString())
				]));
			NewTab("眼镜", () =>
				NewTable(["序号", "名称"], _lGlasses, [
					res => ImGui.Text(res.RowId.ToString()),
					res => RenderIcon(res.Icon),
					res => ImGui.Text(res.Singular.ToString())
				]));
			ImGui.EndTabBar();
		}
	}
}