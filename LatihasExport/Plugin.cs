using System;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace LatihasExport;

public sealed class Plugin : IDalamudPlugin {
    internal static AchievementService AchievementServiceInstance = null!;
    private readonly MainWindow _mainWindow;
    public readonly WindowSystem WindowSystem = new("LatihasExport");

    public Plugin() {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        _mainWindow = new MainWindow();
        WindowSystem.AddWindow(_mainWindow);
        var p = new CommandInfo(OnCommand) {
            HelpMessage = "打开主界面"
        };
        CommandManager.AddHandler("/le", p);
        CommandManager.AddHandler("/latihasexport", p);
        PluginInterface.UiBuilder.Draw += () => WindowSystem.Draw();
        PluginInterface.UiBuilder.OpenMainUi += () => OnCommand(null, null);
        if (Configuration.SavePath == "") {
            Configuration.SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LatihasExport");
            Configuration.Save();
        }
        AchievementServiceInstance = new AchievementService();
        Framework.Update += _ => AchievementServiceInstance.ProcNext();
    }

    public static Configuration Configuration { get; private set; }
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] internal static IDataManager DataManager { get; private set; }
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; }
    [PluginService] internal static IPluginLog Log { get; private set; }
    [PluginService] internal static ICommandManager CommandManager { get; private set; }
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; } = null!;

    public void Dispose() {
        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler("/le");
        CommandManager.RemoveHandler("/latihasexport");
    }

    private void OnCommand(string command, string args) {
        MainWindow.RefreshData();
        _mainWindow.Toggle();
    }
}