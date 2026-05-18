using Dalamud.Bindings.ImGui;
using static LatihasExport.Plugin;

namespace LatihasExport;

public partial class MainWindow {
	private static void DrawAchievement() {
		if (_lAchievement != null) {
			if (ImGui.Checkbox("按照进度排序(勾选刷新，非实时更新)", ref AchievementOrderByProgress)) RefreshData();
			if (ImGui.Button("一键获取空数据(可能会卡死)"))
				foreach (var res in _lAchievement) {
					if (AchievementServiceInstance.Current.ContainsKey(res._rowId)) continue;
					AchievementServiceInstance.UpdateProgress(res._rowId);
				}
			ImGui.SameLine();
			if (ImGui.Button("重置队列(可清除卡死)")) AchievementServiceInstance.Reset();
			ImGui.SameLine();
			if (ImGui.Button("重置获取到的数据")) {
				AchievementServiceInstance.Reset();
				AchievementServiceInstance.Current.Clear();
				AchievementServiceInstance.Max.Clear();
			}
			ImGui.SameLine();
			NewTable(BAchievement.Header, _lAchievement, BAchievement.Acts, "成就", BAchievement.Filters, "_lAchievement");
		} else ImGui.Text("打开一次成就界面以刷新");
	}
}