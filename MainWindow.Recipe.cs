using System.Linq;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using static LatihasExport.Plugin;

namespace LatihasExport;

public partial class MainWindow {
	private static void DrawRecipe() {
		if (ImGui.Button("导出到Artisan")) {
			var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
			foreach (var p in _lRecipe.Select(acc => BRecipe.GetMaterial(acc.Name)))
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
		if (ImGui.Button("导出前50到Artisan")) {
			var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
			var iter = 0;
			foreach (var p in _lRecipe.Select(acc => BRecipe.GetMaterial(acc.Name)))
				if (p != 0) {
					if (iter++ == 49) break;
					sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
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
		NewTable(BRecipe.Header, _lRecipe, BRecipe.Acts, "制作笔记", BRecipe.Filters, "_lRecipe");
	}
}