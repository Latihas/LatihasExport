using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System.Windows.Forms;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Achievement = Lumina.Excel.Sheets.Achievement;

namespace LatihasExport;

public class MainWindow(Plugin plugin) : Window("LatihasExport"), IDisposable {
	private readonly Configuration _configuration = Plugin.Configuration;

	public void Dispose() {
	}

	private void MkDir() {
		try {
			Directory.CreateDirectory(_configuration.SavePath);
		}
		catch (Exception e) {
			Plugin.Log.Error($"创建目录失败: {e.Message}");
		}
	}

	void WriteFile(string fn, string content) {
		MkDir();
	}

	public override unsafe void Draw() {
		if (ImGui.InputText("保存路径", ref _configuration.SavePath, 114514)) _configuration.Save();
		ImGui.SameLine();
		if (ImGui.Button("打开输出目录")) {
			MkDir();
			Process.Start(new ProcessStartInfo(_configuration.SavePath) {
				UseShellExecute = true
			});
		}

		if (ImGui.BeginTabBar("tab")) {
			if (ImGui.BeginTabItem("制作笔记")) {
				if (ImGui.BeginTable("Table1", 3, ImGuiTableFlags.Borders)) {
					ImGui.TableSetupColumn("Op", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 24);
					ImGui.TableSetupColumn("Name");
					foreach (var res in from x in Plugin.DataManager.GameData.GetExcelSheet<Recipe>()
					         let a = x.RowId
					         where a < 30000 && !QuestManager.IsRecipeComplete(a)
					         select x.ItemResult.Value
					         into res
					         where res.Name != ""
					         select res) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						var txt = res.Name.ToString();
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						if (Plugin.TextureProvider.TryGetFromGameIcon((uint)res.Icon, out var tex) && tex.TryGetWrap(out var icon, out _))
							ImGui.Image(icon.Handle, new Vector2(24, 24));
						ImGui.TableSetColumnIndex(2);
						ImGui.Text(txt);
					}
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}

			if (ImGui.BeginTabItem("钓鱼笔记")) {
				if (ImGui.BeginTable("Table2", 3, ImGuiTableFlags.Borders)) {
					ImGui.TableSetupColumn("Op", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("Name");
					ImGui.TableSetupColumn("Dst");

					foreach (var res in from i in Plugin.DataManager.GameData.GetExcelSheet<FishParameter>()
					         where i.IsInLog && i.Text != "" && !PlayerState.Instance()->IsFishCaught(i.RowId)
					         select i) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						var txt = Plugin.DataManager.GetExcelSheet<Item>(Plugin.DataManager.Language).GetRowOrDefault(res.Item.RowId)?.Name.ToString();
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(txt);
						ImGui.TableSetColumnIndex(2);
						ImGui.Text(res.FishingSpot.Value.PlaceName.Value.Name.ToString());
					}
					foreach (var res in from i in Plugin.DataManager.GameData.GetExcelSheet<SpearfishingItem>()
					         where i.IsVisible && !PlayerState.Instance()->IsSpearfishCaught(i.RowId)
					         select i) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						var txt = Plugin.DataManager.GetExcelSheet<Item>(Plugin.DataManager.Language).GetRowOrDefault(res.Item.RowId)?.Name.ToString();
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(txt);
					}
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("成就")) {
				if (FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Instance()->IsLoaded()) {
					if (ImGui.BeginTable("Table4", 5, ImGuiTableFlags.Borders)) {
						ImGui.TableSetupColumn("Op", ImGuiTableColumnFlags.WidthFixed, 64);
						ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 24);
						ImGui.TableSetupColumn("Name");
						foreach (var res in from i in Plugin.DataManager.GameData.GetExcelSheet<Achievement>()
						         where !FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement.Instance()->IsComplete((int)i.RowId) &&
						               i.Name != "" && !i.AchievementHideCondition.Value.HideAchievement && i.AchievementCategory.Value.AchievementKind.Value.RowId is not (13 or 8 or 0)
						         select i) {
							ImGui.TableNextRow();
							ImGui.TableSetColumnIndex(0);
							var txt = res.Name.ToString();
							ImGui.Text(res.RowId.ToString());
							ImGui.TableSetColumnIndex(1);
							if (Plugin.TextureProvider.TryGetFromGameIcon((uint)res.Icon, out var tex) && tex.TryGetWrap(out var icon, out _))
								ImGui.Image(icon.Handle, new Vector2(24, 24));
							ImGui.TableSetColumnIndex(2);
							ImGui.Text(txt);
							ImGui.TableSetColumnIndex(3);
							ImGui.Text(res.Points.ToString());
							ImGui.TableSetColumnIndex(4);
							ImGui.Text(res.AchievementCategory.Value.Name.ToString());
						}
						ImGui.EndTable();
					}
				}else ImGui.Text("打开一次成就界面以刷新");
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("幻卡")) {
				if (ImGui.BeginTable("Table4", 2, ImGuiTableFlags.Borders)) {
					ImGui.TableSetupColumn("Op", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("Name");
					foreach (var res in from i in Plugin.DataManager.GameData.GetExcelSheet<TripleTriadCard>()
					         where i.RowId > 0 && i.Name != "" && !UIState.Instance()->IsTripleTriadCardUnlocked((ushort)i.RowId)
					         select i) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						var txt = res.Name.ToString();
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(txt);
					}
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}

			if (ImGui.BeginTabItem("理符")) {
				if (ImGui.BeginTable("Table5", 5, ImGuiTableFlags.Borders)) {
					ImGui.TableSetupColumn("Op", ImGuiTableColumnFlags.WidthFixed, 64);
					ImGui.TableSetupColumn("Name");
					foreach (var res in from i in Plugin.DataManager.GameData.GetExcelSheet<Leve>()
					         where !QuestManager.Instance()->IsLevequestComplete((ushort)i.RowId)
					               && i.AllowanceCost != 0 && i is { RowId: > 0, GilReward: > 0 }
					         select i) {
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);
						var txt = res.Name.ToString();
						ImGui.Text(res.RowId.ToString());
						ImGui.TableSetColumnIndex(1);
						ImGui.Text(txt);
						ImGui.TableSetColumnIndex(2);
						ImGui.Text(res.ClassJobLevel.ToString());
						ImGui.TableSetColumnIndex(3);
						ImGui.Text(res.ClassJobCategory.Value.Name.ToString());
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