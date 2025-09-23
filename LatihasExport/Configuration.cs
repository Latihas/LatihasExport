using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace LatihasExport;

[Serializable]
public class Configuration : IPluginConfiguration {
	public string SavePath="";

	[NonSerialized] private IDalamudPluginInterface _pluginInterface;

	public int Version { get; set; }

	public void Initialize(IDalamudPluginInterface pluginInterface) {
		_pluginInterface = pluginInterface;
	}

	public void Save() {
		_pluginInterface.SavePluginConfig(this);
	}
}