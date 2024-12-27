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
using static LatihasExport.Beans;
using static LatihasExport.Beans.BFishType;
using static LatihasExport.DumpAssets.SpecialType;
using Ornament = SaintCoinach.Xiv.Ornament;

namespace LatihasExport;

public class Main : IActPluginV1 {
	private static TextBox lab;
	private TabPage _page;
	private int buttonBount;
	private string OUTDIR;
	internal static string ROOTDIR;
	public static readonly bool[] Alive = { true };
	private Address.PlayerStateAddress psa;
	private Address.AchievementAddress aa;
	private Address.RecipeNoteAddress rna;
	private DumpAssets da;
	private bool inited;

	private void Init() {
		foreach (var item in ActGlobals.oFormActMain.ActPlugins.Where(
					item => item.pluginFile.Name.ToUpper().Contains("POSTNAMAZU"))) {
			var postnamazu = item.pluginObj as PostNamazu.PostNamazu;
			if (postnamazu!.State == PostNamazu.PostNamazu.StateEnum.NotReady) {
				Log("鲇鱼精初始化错误，请检查游戏状态");
				return;
			}
			da = new DumpAssets((Process)typeof(PostNamazu.PostNamazu).GetField("FFXIV", NonPublic | Instance)!.GetValue(postnamazu));
			var memory = postnamazu!.Memory;
			Log("Init Address");
			psa = new Address.PlayerStateAddress(postnamazu.SigScanner, memory);
			aa = new Address.AchievementAddress(postnamazu.SigScanner, memory);
			rna = new Address.RecipeNoteAddress(postnamazu.SigScanner, memory);
			ROOTDIR = ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.DirectoryName;
			OUTDIR =  ROOTDIR + "/out/";
			inited = true;
		}
	}

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
			AddButton("打开输出目录", (_, _) => {
				if (!inited) Init();
				if (!Directory.Exists(OUTDIR)) Directory.CreateDirectory(OUTDIR);
				Process.Start(OUTDIR);
			});
			AddButton("角色状态", (_, _) => {
				if (!inited) Init();
				ClearLog();
				Log(psa);
			});
			AddButton("导出小玩意(坐骑、时尚配饰等)", (_, _) => {
				if (!inited) Init();
				ClearLog();
				var mountarr = new BitArray(psa._unlockedMountsBitmask.ToArray());
				var ornamentarr = new BitArray(psa._unlockedOrnamentsBitmask.ToArray());
				Log(mountarr.Length);
				Log(ornamentarr.Length);
				var sb_all = new StringBuilder();
				var sb_rest = new StringBuilder();
				foreach (var s in da!.GetValidSpecial()) {
					try {
						if (s.Category == Mount) {
							s.Completed = mountarr[s.Id];
							if (!s.Completed) sb_rest.Append(s);
						}
						else if (s.Category == DumpAssets.SpecialType.Ornament) {
							s.Completed = ornamentarr[s.Id];
							if (!s.Completed) sb_rest.Append(s);
						}
						sb_all.Append(s);
					}
					catch (Exception e) {
						Log(e.ToString());
						Log(s);
					}
				}
				WriteFile("special_all.csv", sb_all);
				WriteFile("special_rest.csv", sb_rest);
				Log($"导出成功。");
			});
			AddButton("导出制作笔记", (b, _) => {
				if (!inited) Init();
				if (MessageBox.Show("这(或许)将耗费巨量时间", "开始确认", MessageBoxButtons.OKCancel) != DialogResult.OK) return;
				((Button)b).Enabled = false;
				new Task(() => {
					try {
						ClearLog();
						Log("7.1更新会改变文件结构，届时记得更新Definitions/Recipe.json等");
						Log(rna);
						var dic = new Dictionary<string, int>();
						var lis = new List<BRecipe>();
						var vrs = da!.GetValidRecipe();
						Log("获取游戏内完成情况");
						var cc = rna.IsRecipeComplete(vrs.Select(i => (uint)i.Key).ToList());
						Log("映射资源");
						foreach (var i in vrs) {
							if (cc[(uint)i.Key]) continue;
							var sbj = new StringBuilder("[");
							foreach (var j in i.Ingredients) {
								var n = j.Item.ToString();
								sbj.Append(n).Append('+');
								if (!dic.ContainsKey(n)) dic[n] = 0;
								dic[n] += j.Count;
							}
							sbj.Remove(sbj.Length - 1, 1).Append(']');
							lis.Add(new BRecipe(i.Key, i.RecipeLevelTable.ClassJobLevel, i.ToString(), i.ClassJob.ToString(), i.ResultItem.ItemSearchCategory.ToString(), sbj.ToString(), i.ResultItem.Description));
						}
						var sb = new StringBuilder(BRecipe.Header);
						foreach (var i in lis) sb.Append(i);
						WriteFile("recipe_rest.csv", sb);
						var sb2 = new StringBuilder("材料,数量\n");
						foreach (var x in dic) sb2.Append($"{x.Key},{x.Value}\n");
						WriteFile("recipe_rest_material.csv", sb2);
						Log("导出完成。");
						((Button)b).Enabled = true;
					}
					catch (Exception e) {
						Log(e);
					}
				}).Start();
			});
			AddButton("导出钓鱼笔记", (_, _) => {
				if (!inited) Init();
				ClearLog();
				var completed = new List<int>();
				var sb_rest = new StringBuilder(BFish.Header);
				var sb_all = new StringBuilder(BFish.Header);
				var fisharr = new BitArray(psa._caughtFishBitmask.ToArray());
				var speararr = new BitArray(psa._caughtSpearfishBitmask.ToArray());
				var fishes = new List<BFish>();
				foreach (var fish in da!.GetValidFishParameter()) {
					fish.Type = Fishing;
					fish.Completed = fisharr[fish.InlineId];
					fishes.Add(fish);
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
			AddButton("导出成就", (_, _) => {
				if (!inited) Init();
				ClearLog();
				Log(aa);
				if (aa.State != Address.AchievementAddress.AchievementState.Loaded) {
					Log("导出失败，请打开一次成就页面使State变为Loaded");
					return;
				}
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

			page.Controls.Add(lab);
			pluginStatusText.Text = "Plugin Inited.";
			Log("Inited");
		}
		catch (Exception e) {
			pluginStatusText.Text = e.ToString();
		}
	}

	public void DeInitPlugin() {
		Alive[0] = false;
	}

	private void AddButton(string title, EventHandler onClick) {
		var b = new Button {
			Text = title,
			Width = 128,
			Height = 64,
			Location = new Point(128 * buttonBount++, 0)
		};
		b.Click += onClick;
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