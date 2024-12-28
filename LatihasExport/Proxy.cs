using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using Label = System.Windows.Forms.Label;

namespace LatihasExport;

public class Proxy : IActPluginV1 {
	private Assembly LE;
	private dynamic RealPlugin;

	public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
		var text = Path.Combine(ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.DirectoryName!, "libs");
		Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + text);
		Load(Path.Combine(text, "EntityFramework.dll"));
		LE = Load(Path.Combine(text, "LatihasExport.Core.dll"));
		RealPlugin = LE.CreateInstance("LatihasExport.Core.Main");
		RealPlugin!.InitPlugin(pluginScreenSpace, pluginStatusText);
		RealPlugin.InitROOTDIR(ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.DirectoryName);
	}

	public void DeInitPlugin() {
		RealPlugin.DeInitPlugin();
		RealPlugin.Dispose();
		LE = null;
	}

	private static Assembly Load(string path) {
		return Assembly.Load(File.ReadAllBytes(path));
	}
}