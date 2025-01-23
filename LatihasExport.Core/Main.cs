using System.Reflection;
using GreyMagic;
using Newtonsoft.Json.Linq;
using PostNamazu.Common;
using static LatihasExport.Core.Utils;

namespace LatihasExport.Core;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using Newtonsoft.Json;
using static System.Reflection.BindingFlags;
using static Beans;
using static Beans.BFishType;

public class Main : IActPluginV1 {
	private static TextBox lab;
	private TabPage _page;
	private int buttonBount;
	private string OUTDIR;
	internal static string GENDIR;
	internal static string ROOTDIR;
	private Address.PlayerStateAddress psa;
	private Address.AchievementAddress aa;
	// private Address.RecipeNoteAddress rna;
	private Address.QuestManagerAddress qma;
	private DumpAssets da;
	private Assembly FFXIVClientStructs;

	// ReSharper disable once UnusedMember.Global
	public void InitROOTDIR(string s) {
		ROOTDIR = s;
		OUTDIR = ROOTDIR + "/out/";
		GENDIR = ROOTDIR + "/Generated/";
	}

	public void InitFFXIVClientStructs(Assembly asm) {
		FFXIVClientStructs = asm;
	}

	private int xivid = -1;
	private PostNamazu.PostNamazu postnamazu;

	private bool GetNewXIV() {
		if (postnamazu is null) {
			foreach (var item in ActGlobals.oFormActMain.ActPlugins.Where(
						item => item.pluginFile.Name.ToUpper().Contains("POSTNAMAZU"))) {
				postnamazu = item.pluginObj as PostNamazu.PostNamazu;
				if (postnamazu!.State != PostNamazu.PostNamazu.StateEnum.NotReady) continue;
				Log("鲇鱼精初始化错误，请检查游戏状态");
				return false;
			}
		}
		var proc = (Process)typeof(PostNamazu.PostNamazu).GetField("FFXIV", NonPublic | Instance)!.GetValue(postnamazu);
		if (xivid == proc.Id) return false;
		xivid = proc.Id;
		var fullp = proc.MainModule!.FileName;
		XivGameDirectory = fullp.Substring(0, fullp.Length - "game/ffxiv_dx11.exe".Length - 1);
		return true;
	}

	private string XivGameDirectory;

	private void Init() {
		if (!GetNewXIV()) return;
		Log($"ROOTDIR: {ROOTDIR}");
		da = new DumpAssets(XivGameDirectory);
		Log("Init Address");
		memory = postnamazu!.Memory;
		scanner = postnamazu.SigScanner;
		psa = new Address.PlayerStateAddress("PlayerState");
		aa = new Address.AchievementAddress("Achievement");
		// rna = new Address.RecipeNoteAddress("RecipeNote");
		qma = new Address.QuestManagerAddress("QuestManager");
		ima = new Address.InventoryManagerAddress("InventoryManager");
	}

	internal static SigScanner scanner;
	internal static ExternalProcessMemory memory;
	private Address.InventoryManagerAddress ima;

	public void InitPlugin(TabPage page, Label pluginStatusText) {
		try {
			_page = page;
			page.Text = "Latihas Export";
			lab = new TextBox {
				Text = "",
				Width = 1920,
				Height = 800,
				Location = new Point(0, 64),
				Multiline = true,
				Font = new Font("宋体", 12, FontStyle.Bold),
				ScrollBars = ScrollBars.Vertical
			};
			lab.TextChanged += (_, _) => {
				lab.SelectionStart = lab.Text.Length;
				lab.ScrollToCaret();
			};
			AddButton("打开输出目录", () => {
				if (!Directory.Exists(OUTDIR)) Directory.CreateDirectory(OUTDIR);
				Process.Start(OUTDIR);
			});
			AddButton("角色状态", () => {
				Log(psa);
			});
			// AddButton("导出小玩意(坐骑、时尚配饰等)", () => {
			// 	var mountarr = new BitArray(psa._unlockedMountsBitmask.ToArray());
			// 	var ornamentarr = new BitArray(psa._unlockedOrnamentsBitmask.ToArray());
			// 	Log(mountarr.Length);
			// 	Log(ornamentarr.Length);
			// 	var sb_all = new StringBuilder();
			// 	var sb_rest = new StringBuilder();
			// 	foreach (var s in da!.GetValidSpecial()) {
			// 		try {
			// 			if (s.Category == Mount) {
			// 				s.Completed = mountarr[s.Id];
			// 				if (!s.Completed) sb_rest.Append(s);
			// 			}
			// 			else if (s.Category == DumpAssets.SpecialType.Ornament) {
			// 				s.Completed = ornamentarr[s.Id];
			// 				if (!s.Completed) sb_rest.Append(s);
			// 			}
			// 			sb_all.Append(s);
			// 		}
			// 		catch (Exception e) {
			// 			Log(e.ToString());
			// 			Log(s);
			// 		}
			// 	}
			// 	WriteFile("special_all.csv", sb_all);
			// 	WriteFile("special_rest.csv", sb_rest);
			// 	Log($"导出成功。");
			// });
			AddButton("导出制作笔记", () => {
				try {
					// Log(rna);
					Log(qma);
					var dic = new Dictionary<string, int>();
					var lis = new List<BRecipe>();
					var vrs = da!.GetValidRecipe();
					var comparr = new BitArray(qma._completedRecipesBitmask.ToArray());
					foreach (var i in vrs) {
						var sbj = new StringBuilder("[");
						var comp = comparr[i.Key];
						foreach (var j in i.Ingredients) {
							var n = j.Item.ToString();
							sbj.Append(n).Append('+');
							if (comp) continue;
							if (!dic.ContainsKey(n)) dic[n] = 0;
							dic[n] += j.Count;
						}
						if (sbj.Length > 0) sbj.Remove(sbj.Length - 1, 1).Append(']');
						lis.Add(new BRecipe(i.Key, i.RecipeLevelTable.ClassJobLevel, i.ToString(), i.ClassJob.ToString(), i.ResultItem.ItemSearchCategory.ToString(), sbj.ToString(), i.ResultItem.Description, comp));
					}
					StringBuilder sb_all = new(BRecipe.Header),
						sb_rest = new(BRecipe.Header),
						sb2 = new("材料,数量\n");
					foreach (var i in lis) {
						sb_all.Append(i);
						if (!i.Completed) sb_rest.Append(i);
					}
					foreach (var x in dic) sb2.Append($"{x.Key},{x.Value}\n");
					WriteFile("recipe_all.csv", sb_all);
					WriteFile("recipe_rest.csv", sb_rest);
					WriteFile("recipe_rest_material.csv", sb2);
					Log("导出完成。");
				}
				catch (Exception e) { Log(e); }
			});
			AddButton("导出钓鱼笔记", () => {
				var completed = new List<int>();
				var sb_rest = new StringBuilder(BFish.Header);
				var sb_all = new StringBuilder(BFish.Header);
				var fisharr = new BitArray(psa._caughtFishBitmask.ToArray());
				var speararr = new BitArray(psa._caughtSpearfishBitmask.ToArray());
				var fishes = new List<BFish>();
				foreach (var fish in da!.GetValidFishParameter()) {
					try {
						fish.Type = Fishing;
						fish.Completed = fisharr[fish.InlineId];
						fishes.Add(fish);
					}
					catch (Exception) {
						Log(fish.InlineId);
						Log("F");
					}
				}
				foreach (var fish in da.GetValidSpearfishingItem()) {
					fish.Type = Spear;
					fish.Completed = speararr[fish.InlineId - 20000];
					fishes.Add(fish);
				}
				foreach (var fish in fishes) {
					if (fish.Completed) completed.Add(fish.YgId);
					else sb_rest.Append(fish);
					sb_all.Append(fish);
				}
				WriteFile("fish_done.json", "{\"completed\":" + JsonConvert.SerializeObject(completed) + "}");
				WriteFile("fish_rest.csv", sb_rest);
				WriteFile("fish_all.csv", sb_all);
				Log($"导出成功。完成{completed.Count},剩余{fishes.Count - completed.Count}。");
			});
			AddButton("导出成就", () => {
				Log("打开一次成就页面使StateParsed变为Loaded即可");
				Log(aa);
				var sb_rest = new StringBuilder(BAchievement.Header);
				var sb_all = new StringBuilder(BAchievement.Header);
				var bitarr = new BitArray(aa._completedAchievements.ToArray());
				var achievements = da!.GetValidAchievement();
				var completed = 0;
				foreach (var achievement in achievements) {
					achievement.Completed = bitarr[achievement.Id];
					if (!achievement.Completed) {
						sb_rest.Append(achievement);
						completed++;
					}
					sb_all.Append(achievement);
				}
				WriteFile("achievement_all.csv", sb_all);
				WriteFile("achievement_rest.csv", sb_rest);
				Log($"导出成功。完成{completed},剩余{achievements.Count - completed}。");
			});
			AddButton("Test", () => {
				try {
				}
				catch (Exception e) {
					Log(e);
				}
				// InteropGenerator.Runtime.Resolver.GetInstance.Setup();
				// FFXIVClientStructs.Interop.Generated.Addresses.Register();
				// InteropGenerator.Runtime.Resolver.GetInstance.Resolve();

				// var p = ima.scanner.ScanText("E9 ?? ?? ?? ?? 8B CB E8 ?? ?? ?? ?? 84 C0 74 16");
				// Log(ima.ToMemory(p));
				// Log(ima.ToMemory(ima.base_addr));
				// Log(ima.toMemoryArrayString(0, 5000));
				// Log(ima);
				// Log(ima.EasyExecFunc<uint>(p, ima.base_addr));
			});
			page.Controls.Add(lab);
			pluginStatusText.Text = "Plugin Inited.";
			Log("Inited");
		}
		catch (Exception e) {
			pluginStatusText.Text = e.ToString();
		}
	}

	public void DeInitPlugin() {
	}

	private void AddButton(string title, Action action) {
		var b = new Button {
			Text = title,
			Width = 128,
			Height = 64,
			Location = new Point(128 * buttonBount++, 0)
		};
		b.Click += (_, _) => {
			ClearLog();
			Init();
			action();
		};
		_page.Controls.Add(b);
	}

	private void WriteFile(string name, object content) {
		if (!Directory.Exists(OUTDIR)) Directory.CreateDirectory(OUTDIR);
		File.WriteAllText(OUTDIR + name, content.ToString(), Encoding.UTF8);
	}

	public static void Log(object text) {
		lab.Text += text + "\r\n";
	}

	private static void ClearLog() {
		lab.Text = "";
	}
}