using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace LatihasExport;

[Serializable]
public class Configuration : IPluginConfiguration {
	public string SavePath = "";

	public int Version { get; set; }

	public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}