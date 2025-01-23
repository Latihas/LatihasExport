using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using Label = System.Windows.Forms.Label;

namespace LatihasExport;

public class Proxy : IActPluginV1 {
	private Assembly Core;
	private dynamic RealPlugin;

	public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
		var text = Path.Combine(ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.DirectoryName!, "libs");
		Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + text);
		Load(Path.Combine(text, "EntityFramework.dll"));
		var asm= Load(Path.Combine(text, "FFXIVClientStructs.dll"));
		Core = Load(Path.Combine(text, "LatihasExport.Core.dll"));
		RealPlugin = Core.CreateInstance("LatihasExport.Core.Main");
		RealPlugin!.InitPlugin(pluginScreenSpace, pluginStatusText);
		RealPlugin.InitROOTDIR(ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.DirectoryName);
		RealPlugin.InitFFXIVClientStructs(asm);
	}

	public void DeInitPlugin() {
		RealPlugin.DeInitPlugin();
		RealPlugin.Dispose();
		Core = null;
	}

	private static Assembly Load(string path) {
		return Assembly.Load(File.ReadAllBytes(path));
	}
}