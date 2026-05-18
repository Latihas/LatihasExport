using System;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Ipc;
using static LatihasExport.Plugin;

namespace LatihasExport;

internal static class Ipcs {
	private const string Name = "vnavmesh";
	private static bool _hasLoggedInitSuccess, _hasLoggedPluginNotFound;
	private static ICallGateSubscriber<bool>? _navIsReady;
	private static ICallGateSubscriber<object>? _pathStop;
	private static ICallGateSubscriber<Vector3, bool, bool>? _pathfindAndMoveTo;

	private static bool IsPluginLoaded() {
		try {
			return PluginInterface.InstalledPlugins.Any(p => p is { Name: Name, IsLoaded: true });
		} catch (Exception ex) {
			Log.Error($"检查插件加载状态时发生错误: {ex.Message}");
			return false;
		}
	}

	private static void Init() {
		if (IsPluginLoaded()) {
			try {
				var pi = PluginInterface;
				_navIsReady = pi.GetIpcSubscriber<bool>($"{Name}.Nav.IsReady");
				_pathStop = pi.GetIpcSubscriber<object>($"{Name}.Path.Stop");
				pi.GetIpcSubscriber<bool>($"{Name}.Path.IsRunning");
				_pathfindAndMoveTo = pi.GetIpcSubscriber<Vector3, bool, bool>($"{Name}.SimpleMove.PathfindAndMoveTo");
				pi.GetIpcSubscriber<Vector3>($"{Name}.Query.Mesh.FlagToPoint");
				if (_hasLoggedInitSuccess) return;
				Log.Information("NavmeshIPC初始化成功");
				_hasLoggedInitSuccess = true;
			} catch (Exception ex) {
				Log.Error($"NavmeshIPC初始化失败: {ex}");
			}
		} else {
			if (_hasLoggedPluginNotFound) return;
			Log.Warning($"未找到 {Name} 插件，导航功能不可用");
			_hasLoggedPluginNotFound = true;
		}
	}

	private static T? Execute<T>(Func<T> func) {
		if (!IsPluginLoaded()) return default;
		try {
			return func();
		} catch (Exception ex) {
			Log.Error($"IPC执行错误: {ex}");
		}
		return default;
	}

	private static void Execute(Action action) {
		if (!IsPluginLoaded()) return;
		try {
			action();
		} catch (Exception ex) {
			Log.Error($"IPC执行错误: {ex}");
		}
	}

	private static bool IsReady() {
		var result = Execute(() => _navIsReady?.InvokeFunc());
		return result.HasValue && result.Value;
	}

	internal static void Stop() {
		if (!IsReady()) Init();
		Execute(() => _pathStop?.InvokeAction());
	}

	internal static void PathfindAndMoveTo(Vector3 pos, bool fly) {
		if (!IsReady()) Init();
		Stop();
		if (HasCore()) {
			CoreDiveTp(pos);
			return;
		}
		Execute(() => _pathfindAndMoveTo?.InvokeFunc(pos, fly));
	}

	private static void CoreDiveTp(Vector3 pos) {
		if (!IsReady()) Init();
		else {
			_divetp ??= PluginInterface.GetIpcSubscriber<Vector3, bool>("LatihasDalamudCore.DiveTp");
			_divetp.InvokeAction(pos);
		}
	}

	private static ICallGateSubscriber<Vector3, bool>? _divetp;

	private static bool HasCore() {
		try {
			return PluginInterface.InstalledPlugins.Any(p => p is { Name: "LatihasDalamudCore", IsLoaded: true })
			       && PluginInterface.GetIpcSubscriber<bool>("LatihasDalamudCore.WAS_HERE").InvokeFunc();
		} catch (Exception) {
			return false;
		}
	}
}