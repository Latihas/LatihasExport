using System;
using Dalamud.Configuration;

namespace LatihasExport;

[Serializable]
public class MConfiguration : IPluginConfiguration {
	public string SavePath = "";

	public int Version { get; set; }

	public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}