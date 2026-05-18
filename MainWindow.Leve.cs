using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.ImGuiNotification;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using static LatihasExport.Plugin;

namespace LatihasExport;

public partial class MainWindow {
	private static void DrawLeve() {
		if (ImGui.Button("导出已接受的可制作物品到Artisan")) {
			var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
			foreach (var p in _lLeveAccepted.Select(acc => BRecipe.GetMaterial(acc.ItemName)))
				if (p != 0)
					sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
			sb.Append("]}");
			var s = sb.ToString();
			ImGui.SetClipboardText(s);
			NotificationManager.AddNotification(new Notification {
				Title = "已复制",
				Content = s
			});
		}
		ImGui.SameLine();
		if (ImGui.Button("导出所有的可制作物品到Artisan")) {
			var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
			foreach (var acc in _lLeve) {
				var p = BRecipe.GetMaterial(acc.ItemName);
				if (p != 0) sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
			}
			sb.Append("]}");
			var s = sb.ToString();
			ImGui.SetClipboardText(s);
			NotificationManager.AddNotification(new Notification {
				Title = "已复制",
				Content = s
			});
		}
		ImGui.SameLine();
		if (ImGui.Button("导出所有的可制作物品前50到Artisan")) {
			var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
			var iter = 0;
			foreach (var acc in _lLeve) {
				var p = BRecipe.GetMaterial(acc.ItemName);
				if (p != 0) {
					if (iter++ == 49) break;
					sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
				}
			}
			sb.Append("]}");
			var s = sb.ToString();
			ImGui.SetClipboardText(s);
			NotificationManager.AddNotification(new Notification {
				Title = "已复制前50",
				Content = s
			});
		}
		ImGui.SameLine();
		if (ImGui.Button("导出所有的不可制作物品")) {
			var sb = new StringBuilder();
			foreach (var p in _lLeve.Select(acc => BRecipe.GetNonMaterial(acc.ItemName)))
				if (p is not null)
					sb.Append($"{p},");
			var s = sb.ToString();
			ImGui.SetClipboardText(s);
			NotificationManager.AddNotification(new Notification {
				Title = "已复制",
				Content = s
			});
		}
		ImGui.SameLine();
		ToCsv(BLeve.Header, _lLeve, "理符");
		ImGui.Separator();
		ImGui.Text("先接取并到达目的地附近开启理符任务，然后点击开始即可。");
		AutoGetherLeve();
		AutoFightLeve();
		ImGui.Separator();
		ImGui.Text("已接受理符");
		NewTable(BLeve.Header, _lLeveAccepted, BLeve.Acts, filter: BLeve.Filters, filterTag: "_lLeveAccepted");
		ImGui.Text("所有理符");
		NewTable(BLeve.Header, _lLeve, BLeve.Acts, filter: BLeve.Filters, filterTag: "_lLeve");
	}

	internal static bool IsAutoGathering;
	internal static bool IsAutoFighting;

	private static void AutoGetherLeve() {
		if (!IsAutoGathering && ImGui.Button("开始半自动采集理符")) {
			IsAutoGathering = true;
			Task.Run(async () => {
				while (IsAutoGathering) {
					var ptr = GameGui.GetAddonByName("Gathering");
					if (ptr == null || ptr == IntPtr.Zero || !ptr.IsVisible) {
						var pp = ObjectTable.LocalPlayer!.Position;
						Pointer<GameObject> gp;
						unsafe {
							var gl = GameObjectManager.Instance()->Objects.IndexSorted.ToArray().Where(obj => {
								try {
									var o = obj.Value;
									return o->ObjectKind == ObjectKind.GatheringPoint
									       && o->NamePlateIconId == 71244
									       && o->TargetableStatus.HasFlag(ObjectTargetableFlags.IsTargetable);
								} catch (Exception) {
									return false;
								}
							}).OrderBy(obj => Vector3.DistanceSquared(obj.Value->Position, pp)).FirstOrDefault();
							if (gl == null) break;
							gp = gl.Value;
							Ipcs.PathfindAndMoveTo(gl.Value->Position, Condition[ConditionFlag.Mounted]);
						}
						await Task.Delay(3000);
						Ipcs.Stop();
						await Task.Delay(200);
						unsafe {
							TargetSystem.Instance()->SetHardTarget(gp);
						}
						await Task.Delay(200);
						unsafe {
							TargetSystem.Instance()->InteractWithObject(gp);
						}
						await Task.Delay(200);
					} else {
						var clicked = false;
						unsafe {
							var atk = (AtkUnitBase*)ptr.Address;
							var AtkUldManager = atk->UldManager;
							for (var i = 0; i < AtkUldManager.NodeListCount; i++) {
								var gri = AtkUldManager.NodeList[i];
								if ((ushort)gri->Type == 1010) {
									var cb = gri->GetAsAtkComponentCheckBox();
									var GriUldManager = cb->UldManager;
									for (var j = 0; j < GriUldManager.NodeListCount; j++) {
										var grj = GriUldManager.NodeList[j];
										if (grj->Type == NodeType.Res) {
											for (var k = 0; k < grj->ChildCount; k++) {
												var grk = grj->ChildNode[k];
												if ((ushort)grk.Type == 1005 && grk.IsVisible()) {
													Click(cb, atk);
													clicked = true;
													break;
												}
											}
										}
									}
								}
							}
						}
						if (clicked) await Task.Delay(400);
					}
				}
				IsAutoGathering = false;
			});
		}
		if (IsAutoGathering && ImGui.Button("停止半自动采集理符")) {
			IsAutoGathering = false;
			Ipcs.Stop();
		}
	}

	private static void AutoFightLeve() {
		if (!IsAutoFighting && ImGui.Button("开始半自动战斗理符")) {
			IsAutoFighting = true;
			Task.Run(async () => {
				while (IsAutoFighting) {
					if (Condition[ConditionFlag.InCombat]) {
						await Task.Delay(3000);
						continue;
					}
					unsafe {
						var pp = ObjectTable.LocalPlayer!.Position;
						var gl = GameObjectManager.Instance()->Objects.IndexSorted.ToArray().Where(obj => {
							try {
								var o = obj.Value;
								return o->ObjectKind == ObjectKind.BattleNpc
								       && o->NamePlateIconId == 71244
								       && !o->IsDead();
							} catch (Exception) {
								return false;
							}
						}).OrderBy(obj => Vector3.DistanceSquared(obj.Value->Position, pp)).FirstOrDefault();
						if (gl == null) break;
						Ipcs.PathfindAndMoveTo(gl.Value->Position, Condition[ConditionFlag.Mounted]);
					}
					await Task.Delay(3000);
				}
				IsAutoFighting = false;
			});
		}
		if (IsAutoFighting && ImGui.Button("停止半自动战斗理符")) {
			IsAutoFighting = false;
			Ipcs.Stop();
		}
	}
}