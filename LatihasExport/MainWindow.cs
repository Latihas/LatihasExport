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
using Lumina.Excel.Sheets;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Achievement = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement;

namespace LatihasExport;

public class MainWindow() : Window("LatihasExport") {
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

	private static IEnumerable<Item> LRecipe;
	private static IEnumerable<uint> LFishCaught, LSFishCaught;
	private static IEnumerable<FishParameter> LFishUnCaught;
	private static IEnumerable<SpearfishingItem> LSFishUnCaught;
	private static IEnumerable<Lumina.Excel.Sheets.Achievement> LAchievement;
	private static IEnumerable<TripleTriadCard> LTripleTriadCard;
	private static IEnumerable<Leve> LLeve;


	internal unsafe static void RefreshData() {
		LRecipe = Plugin.DataManager.GameData.GetExcelSheet<Recipe>()
			.Select(x => new {
				x,
				a = x.RowId
			})
			.Where(t => t.a < 30000 && !QuestManager.IsRecipeComplete(t.a))
			.Select(t => t.x.ItemResult.Value)
			.Where(t => t.Name != "");
		var PlayerStateInstance = PlayerState.Instance();
		var tmp = Plugin.DataManager.GameData.GetExcelSheet<FishParameter>()
			.Where(i => i.IsInLog && i.Text != "").ToList();
		LFishCaught = tmp.Where(i => PlayerStateInstance->IsFishCaught(i.RowId)).Select(i => i.Item.RowId);
		LFishUnCaught = tmp.Where(i => !PlayerStateInstance->IsFishCaught(i.RowId));
		var tmp2 = Plugin.DataManager.GameData.GetExcelSheet<SpearfishingItem>().Where(i => i.IsVisible).ToList();
		LSFishCaught = tmp2.Where(i => PlayerStateInstance->IsSpearfishCaught(i.RowId)).Select(i => i.Item.RowId);
		LSFishUnCaught = tmp2.Where(i => !PlayerStateInstance->IsSpearfishCaught(i.RowId));
		if (Achievement.Instance()->IsLoaded())
			LAchievement = Plugin.DataManager.GameData.GetExcelSheet<Lumina.Excel.Sheets.Achievement>()
				.Where(i => !Achievement.Instance()->IsComplete((int)i.RowId) && i.Name != "" && !i.AchievementHideCondition.Value.HideAchievement &&
				            i.AchievementCategory.Value.AchievementKind.Value.RowId is not (13 or 8 or 0));
		LTripleTriadCard = Plugin.DataManager.GameData.GetExcelSheet<TripleTriadCard>().Where(i => i.RowId > 0 && i.Name != "" && !UIState.Instance()->IsTripleTriadCardUnlocked((ushort)i.RowId));
		LLeve = Plugin.DataManager.GameData.GetExcelSheet<Leve>().Where(i => !QuestManager.Instance()->IsLevequestComplete((ushort)i.RowId) && i.AllowanceCost != 0 && i is { RowId: > 0, GilReward: > 0 });
	}

	private static void Start(string cmd) => Process.Start(new ProcessStartInfo(cmd) {
		UseShellExecute = true
	});

	private const ImGuiTableFlags ImGuiTableFlag = ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg;

	public override unsafe void Draw() {
		if (ImGui.InputText("保存路径", ref _configuration.SavePath, 114514)) _configuration.Save();
		ImGui.SameLine();
		if (ImGui.Button("打开输出目录")) {
			MkDir();
			Start(_configuration.SavePath);
		}
		if (ImGui.Button("刷新")) RefreshData();
		if (ImGui.BeginTabBar("tab")) {
			if (ImGui.BeginTabItem("制作笔记")) {
				if (ImGui.BeginTable("Table1", 3, ImGuiTableFlag)) {
					ImGui.TableSetupColumn("序号", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 24);
					ImGui.TableSetupColumn("名称");
					ImGui.TableHeadersRow();
					foreach (var res in LRecipe) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						if (Plugin.TextureProvider.TryGetFromGameIcon((uint)res.Icon, out var tex) && tex.TryGetWrap(out var icon, out _))
							ImGui.Image(icon.Handle, new Vector2(24, 24));
						ImGui.TableSetColumnIndex(2);
						ImGui.Text(res.Name.ToString());
					}
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("钓鱼")) {
				if (ImGui.Button("导出到鱼糕")) {
					var sb = new StringBuilder("{\"completed\":[");
					foreach (var res in LFishCaught) sb.Append(res).Append(',');
					foreach (var res in LSFishCaught) sb.Append(res).Append(',');
					sb.Remove(sb.Length - 1, 1).Append("]}");
					WriteFile("fish.json", sb.ToString());
				}
				ImGui.SameLine();
				if (ImGui.Button("打开鱼糕")) Start("https://fish.ffmomola.com/ng/#/wiki/fishing");
				if (ImGui.BeginTable("Table2", 4, ImGuiTableFlag)) {
					ImGui.TableSetupColumn("序号", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("鱼糕序号", ImGuiTableColumnFlags.WidthFixed, 96);
					ImGui.TableSetupColumn("名称");
					ImGui.TableSetupColumn("地点");
					ImGui.TableHeadersRow();
					foreach (var res in LFishUnCaught) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(res.Item.RowId.ToString());
						ImGui.TableSetColumnIndex(2);
						ImGui.Text(Plugin.DataManager.GetExcelSheet<Item>(Plugin.DataManager.Language).GetRowOrDefault(res.Item.RowId)?.Name.ToString());
						ImGui.TableSetColumnIndex(3);
						ImGui.Text(res.FishingSpot.Value.PlaceName.Value.Name.ToString());
					}
					foreach (var res in LSFishUnCaught) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(res.Item.RowId.ToString());
						ImGui.TableSetColumnIndex(2);
						ImGui.Text(Plugin.DataManager.GetExcelSheet<Item>(Plugin.DataManager.Language).GetRowOrDefault(res.Item.RowId)?.Name.ToString());
					}
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("成就")) {
				if (Achievement.Instance()->IsLoaded() && LAchievement != null) {
					if (ImGui.BeginTable("Table4", 5, ImGuiTableFlag)) {
						ImGui.TableSetupColumn("序号", ImGuiTableColumnFlags.WidthFixed, 64);
						ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 24);
						ImGui.TableSetupColumn("名称");
						ImGui.TableSetupColumn("成就点");
						ImGui.TableSetupColumn("分类");
						ImGui.TableHeadersRow();
						foreach (var res in LAchievement) {
							ImGui.TableNextRow();
							ImGui.TableSetColumnIndex(0);
							ImGui.Text(res.RowId.ToString());
							ImGui.TableSetColumnIndex(1);
							if (Plugin.TextureProvider.TryGetFromGameIcon((uint)res.Icon, out var tex) && tex.TryGetWrap(out var icon, out _))
								ImGui.Image(icon.Handle, new Vector2(24, 24));
							ImGui.TableSetColumnIndex(2);
							ImGui.Text(res.Name.ToString());
							ImGui.TableSetColumnIndex(3);
							ImGui.Text(res.Points.ToString());
							ImGui.TableSetColumnIndex(4);
							ImGui.Text(res.AchievementCategory.Value.Name.ToString());
						}
						ImGui.EndTable();
					}
				}
				else ImGui.Text("打开一次成就界面以刷新");
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("幻卡")) {
				if (ImGui.Button("导出到arrtripletriad.com")) {
					var sb = new StringBuilder("[false,");
					var bitArray = new BitArray(UIState.Instance()->UnlockedTripleTriadCardsBitmask.ToArray());
					for (var l = 1; l < bitArray.Count; l++) sb.Append(bitArray[l - 1].ToString().ToLower()).Append(',');
					sb.Remove(sb.Length - 1, 1).Append(']');
					WriteFile("ttc.json", sb.ToString());
				}
				ImGui.SameLine();
				if (ImGui.Button("打开arrtripletriad.com")) Start("https://arrtripletriad.com/cn/huan-ka-yi-lan");
				if (ImGui.BeginTable("Table4", 2, ImGuiTableFlag)) {
					ImGui.TableSetupColumn("序号", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("名称");
					ImGui.TableHeadersRow();
					foreach (var res in LTripleTriadCard) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(res.Name.ToString());
					}
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("理符")) {
				if (ImGui.BeginTable("Table5", 5, ImGuiTableFlag)) {
					ImGui.TableSetupColumn("序号", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("职业", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("等级", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("名称");
					ImGui.TableSetupColumn("发布人");
					ImGui.TableHeadersRow();
					foreach (var res in LLeve) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(res.ClassJobCategory.Value.Name.ToString());
						ImGui.TableSetColumnIndex(2);
						ImGui.Text(res.ClassJobLevel.ToString());
						ImGui.TableSetColumnIndex(3);
						ImGui.Text(res.Name.ToString());
						ImGui.TableSetColumnIndex(4);
						ImGui.Text(res.LeveClient.Value.Name.ToString());
					}
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}
			ImGui.EndTabBar();
		}
	}
}