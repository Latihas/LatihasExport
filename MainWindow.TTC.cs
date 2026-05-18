using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using static LatihasExport.Plugin;

namespace LatihasExport;

public partial class MainWindow {
	private static void DrawTTC() {
		if (ImGui.Button("导出到arrtripletriad.com")) {
			var sb = new StringBuilder("[false,");
			unsafe {
				var bitArray = UIState.Instance()->UnlockedTripleTriadCardsBitArray;
				for (var l = 1; l < bitArray.ByteLength; l++) sb.Append(bitArray[l - 1].ToString().ToLower()).Append(',');
			}
			sb.Remove(sb.Length - 1, 1).Append(']');
			WriteFile("ttc.json", sb.ToString());
			NotificationManager.AddNotification(new Notification {
				Title = "导完了",
				Content = "已导出到 ttc.json"
			});
		}
		ImGui.SameLine();
		if (ImGui.Button("打开arrtripletriad.com")) Start("https://arrtripletriad.com/cn/huan-ka-yi-lan");
		ImGui.SameLine();
		NewTable(BT2.Header, _lTripleTriadCard, BT2.Acts, "幻卡", BT2.Filters, "_lTripleTriadCard");
	}
}