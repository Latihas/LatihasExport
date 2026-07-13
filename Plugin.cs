using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace LatihasExport;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class Plugin : IDalamudPlugin {
	internal static AchievementService AchievementServiceInstance = null!;
	private readonly MainWindow _mainWindow;
	// ReSharper disable once MemberCanBePrivate.Global
	public readonly WindowSystem WindowSystem = new("LatihasExport");

	public Plugin() {
		Configuration = PluginInterface.GetPluginConfig() as MConfiguration ?? new MConfiguration();
		_mainWindow = new MainWindow();
		WindowSystem.AddWindow(_mainWindow);
		var p = new CommandInfo(OnCommand) {
			HelpMessage = "打开主界面"
		};
		CommandManager.AddHandler("/le", p);
		CommandManager.AddHandler("/latihasexport", p);
		CommandManager.AddHandler("/startagl", new CommandInfo((_, _) => {
			if (!MainWindow.IsAutoGathering) MainWindow.StartAutoGatherLeve();
		}) {
			HelpMessage = "开始半自动采集理符"
		});
		CommandManager.AddHandler("/stopagl", new CommandInfo((_, _) => {
			MainWindow.StopAutoGatherLeve();
		}) {
			HelpMessage = "停止半自动采集理符"
		});
		PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
		PluginInterface.UiBuilder.OpenMainUi += OnCommand;
		if (Configuration.SavePath == "") {
			Configuration.SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LatihasExport");
			Configuration.Save();
		}
		AchievementServiceInstance = new AchievementService();
		Framework.Update += _ => AchievementServiceInstance.ProcNext();
	}

	internal static MConfiguration Configuration { get; private set; } = null!;

	[PluginService] public static INotificationManager NotificationManager { get; private set; } = null!;
	[PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] internal static IDataManager DataManager { get; private set; } = null!;
	[PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
	[PluginService] internal static IPluginLog Log { get; private set; } = null!;
	[PluginService] private static ICommandManager CommandManager { get; set; } = null!;
	[PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
	[PluginService] private static IFramework Framework { get; set; } = null!;
	[PluginService] internal static IObjectTable ObjectTable { get; set; } = null!;
	[PluginService] internal static IGameGui GameGui { get; set; } = null!;
	[PluginService] internal static ICondition Condition { get; set; } = null!;

	public void Dispose() {
		MainWindow.IsAutoGathering = false;
		MainWindow.IsAutoFighting = false;
		WindowSystem.RemoveAllWindows();
		CommandManager.RemoveHandler("/le");
		CommandManager.RemoveHandler("/latihasexport");
		CommandManager.RemoveHandler("/startagl");
		CommandManager.RemoveHandler("/stopagl");
		AchievementServiceInstance.Dispose();
	}

	private void OnCommand(string command, string args) => OnCommand();

	private void OnCommand() {
		MainWindow.RefreshData();
		_mainWindow.Toggle();
	}
}