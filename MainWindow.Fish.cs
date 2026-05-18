using System.Linq;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using static LatihasExport.Plugin;

namespace LatihasExport;

public partial class MainWindow {
	private static void DrawFish() {
		if (ImGui.Button("导出到鱼糕")) {
			var sb = new StringBuilder("{\"completed\":[");
			foreach (var res in _lFishCaught) sb.Append(res).Append(',');
			foreach (var res in _lsFishCaught) sb.Append(res).Append(',');
			sb.Remove(sb.Length - 1, 1).Append("]}");
			WriteFile("fish.json", sb.ToString());
			NotificationManager.AddNotification(new Notification {
				Title = "导完了",
				Content = "已导出到 fish.json"
			});
		}
		ImGui.SameLine();
		if (ImGui.Button("打开鱼糕")) Start("https://fish.ffmomola.com/ng/#/wiki/fishing");
		ImGui.SameLine();
		ToCsv(BUncaughtFish.Header, _lFishUnCaught.Concat(_lsFishUnCaught).ToArray(), "钓鱼");
		NewTable(BUncaughtFish.Header, _lFishUnCaught, BUncaughtFish.Acts, filter: BUncaughtFish.Filters, filterTag: "_lFishUnCaught");
		NewTable(BUncaughtFish.Header, _lsFishUnCaught, BUncaughtFish.Acts, filter: BUncaughtFish.Filters, filterTag: "_lsFishUnCaught");
	}
}