using System;
using System.IO;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Input;

namespace LatihasExport;

public sealed class Plugin : IDalamudPlugin {
	public readonly WindowSystem WindowSystem = new("LatihasExport");

	public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commandManager) {
		PluginInterface = pluginInterface;
		CommandManager = commandManager;
		Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
		Configuration.Initialize(PluginInterface);
		MainWindow = new MainWindow(this);
		WindowSystem.AddWindow(MainWindow);
		CommandManager.AddHandler("/le", new CommandInfo(OnCommand) {
			HelpMessage = "打开主界面"
		});
		CommandManager.AddHandler("/latihasexport", new CommandInfo(OnCommand) {
			HelpMessage = "打开主界面"
		});
		PluginInterface.UiBuilder.Draw += () => WindowSystem.Draw();
		PluginInterface.UiBuilder.OpenConfigUi += () => OnCommand(null, null);
		if (Configuration.SavePath == "") {
			Configuration.SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LatihasExport");
			Configuration.Save();
		}
	}

	[PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; }
	[PluginService] internal static IDataManager DataManager { get; private set; }
	[PluginService] internal static ITextureProvider TextureProvider { get; private set; }
	[PluginService] internal static IPluginLog Log { get; private set; }
	[PluginService] internal ICommandManager CommandManager { get; private set; }
	// [PluginService] internal TextService CommandManager { get; private set; }
	public static Configuration Configuration { get; private set; }


	private MainWindow MainWindow { get; init; }

	public void Dispose() {
		WindowSystem.RemoveAllWindows();
		MainWindow.Dispose();
		CommandManager.RemoveHandler("/le");
		CommandManager.RemoveHandler("/latihasexport");
	}

	private void OnCommand(string command, string args) {
		MainWindow.Toggle();
	}
}