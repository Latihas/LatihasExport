using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentType;
using static LatihasExport.Plugin;
using Achievement = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement;
using Action = System.Action;
using InstanceContent = Lumina.Excel.Sheets.InstanceContent;
using InstanceContentType = FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentType;

namespace LatihasExport;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "InvertIf")]
public class MainWindow() : Window("LatihasExport") {
    private const ImGuiTableFlags ImGuiTableFlag = ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg;
    private static BRecipe[] _lRecipe = null!;
    private static uint[] _lFishCaught = null!, _lsFishCaught = null!;
    private static BUncaughtFish[] _lFishUnCaught = null!, _lsFishUnCaught = null!;
    private static BAchievement[]? _lAchievement;
    private static BT2[] _lTripleTriadCard = null!, _lOrchestrion = null!;
    private static BLeve[] _lLeve = null!, _lLeveAccepted = null!;
    private static BQuest[] _lQuest = null!;
    private static BHowTo[] _lHowto = null!, lInstanceContent = null!;
    private static BT3[] _lMount = null!, _lOrnament = null!, _lGlasses = null!, _lEmote = null!, _lCompanion = null!;
    private static unsafe PlayerState* _playerStateInstance;
    private static unsafe Achievement* _achievementInstance;
    private static unsafe QuestManager* _questManagerInstance;
    private readonly Configuration _configuration = Plugin.Configuration;

    private void MkDir() {
        try {
            Directory.CreateDirectory(_configuration.SavePath);
        }
        catch (Exception e) {
            Log.Error($"创建目录失败: {e.Message}");
        }
    }

    private void WriteFile(string fn, string content) {
        MkDir();
        File.WriteAllText(Path.Combine(_configuration.SavePath, fn), content);
    }

    private static IEnumerable<T> Gl<T>(Func<T, bool> predicate) where T : struct, IExcelRow<T> =>
        DataManager.GameData.GetExcelSheet<T>()!.Where(predicate);


    internal static unsafe void RefreshData() {
        try {
            _playerStateInstance = PlayerState.Instance();
            _achievementInstance = Achievement.Instance();
            _questManagerInstance = QuestManager.Instance();
            var UIStateInstance = UIState.Instance();
            _lRecipe = BRecipe.GetData();
            var tmp = Gl<FishParameter>(i => i.IsInLog && i.Text != "").ToArray();
            _lFishCaught = tmp.Where(i => _playerStateInstance->IsFishCaught(i.RowId)).Select(i => i.Item.RowId).ToArray();
            _lFishUnCaught = tmp.Where(i => !_playerStateInstance->IsFishCaught(i.RowId)).Select(res => new BUncaughtFish(
                res.RowId.ToString(),
                res.Item.RowId.ToString(),
                DataManager.GetExcelSheet<Item>(DataManager.Language).GetRowOrDefault(res.Item.RowId)!.Value.Name.ToString(),
                res.FishingSpot.Value.PlaceName.Value.Name.ToString()
            )).ToArray();
            var tmp2 = Gl<SpearfishingItem>(i => i.IsVisible).ToArray();
            _lsFishCaught = tmp2.Where(i => _playerStateInstance->IsSpearfishCaught(i.RowId)).Select(i => i.Item.RowId).ToArray();
            _lsFishUnCaught = tmp2.Where(i => !_playerStateInstance->IsSpearfishCaught(i.RowId)).Select(res => new BUncaughtFish(
                res.RowId.ToString(),
                res.Item.RowId.ToString(),
                DataManager.GetExcelSheet<Item>(DataManager.Language).GetRowOrDefault(res.Item.RowId)!.Value.Name.ToString(),
                ""
            )).ToArray();
            if (_achievementInstance->IsLoaded()) _lAchievement = BAchievement.GetData();
            _lTripleTriadCard = Gl<TripleTriadCard>(i => i.RowId > 0 && i.Name != "" && !UIStateInstance->IsTripleTriadCardUnlocked((ushort)i.RowId))
                .Select(res => new BT2(
                    res.RowId.ToString(),
                    res.Name.ToString()
                )).ToArray();
            _lLeve = BLeve.GetData();
            _lLeveAccepted = _questManagerInstance->LeveQuests.ToArray().Where(i => i.LeveId != 0).Select(i => _lLeve.First(x => x._rowId == i.LeveId)).ToArray();
            _lQuest = BQuest.GetData();
            _lOrchestrion = Gl<Orchestrion>(i => !_playerStateInstance->IsOrchestrionRollUnlocked(i.RowId) && i.Name != "").Select(res => new BT2(
                res.RowId.ToString(),
                res.Name.ToString()
            )).ToArray();
            _lMount = Gl<Mount>(i => !_playerStateInstance->IsMountUnlocked(i.RowId) && i.Icon != 0 && !i.Singular.ToString().IsNullOrEmpty()).Select(res => new BT3(
                res.RowId.ToString(),
                res.Icon,
                res.Singular.ToString())).ToArray();
            _lOrnament = Gl<Ornament>(i => !_playerStateInstance->IsOrnamentUnlocked(i.RowId) && i.RowId > 0).Select(res => new BT3(
                res.RowId.ToString(),
                res.Icon,
                res.Singular.ToString())).ToArray();
            _lGlasses = Gl<Glasses>(i => !_playerStateInstance->IsGlassesUnlocked((ushort)i.RowId) && i.RowId > 0).Select(res => new BT3(
                res.RowId.ToString(),
                res.Icon,
                res.Singular.ToString())).ToArray();
            _lEmote = Gl<Emote>(i => !UIStateInstance->IsEmoteUnlocked((ushort)i.RowId) && i.Icon != 0 && !i.Name.ToString().IsNullOrEmpty()).Select(res => new BT3(
                res.RowId.ToString(),
                res.Icon,
                res.Name.ToString())).ToArray();
            _lHowto = Gl<HowTo>(i => !UIStateInstance->IsHowToUnlocked(i.RowId) && i.Name != "").Select(res => new BHowTo(
                res.RowId.ToString(),
                res.Category.Value.Category.ToString(),
                res.Name.ToString()
            )).ToArray();
            lInstanceContent = Gl<InstanceContent>(i =>
                !UIState.IsInstanceContentCompleted(i.RowId) &&
                !i.ContentFinderCondition.Value.Name.IsEmpty &&
                i.InstanceContentType.RowId is not ((int)InstanceContentType.QuestBattle
                    or (int)TreasureHuntDungeon or (int)Mahjong or (int)GoldSaucer or (int)OceanFishing or (int)UnrealTrial
                    or (int)InstanceContentType.DeepDungeon or (int)RivalWing or (int)CrystallineConflict or (int)SeasonalDungeon
                    or (int)InstanceContentType.TripleTriad)
            ).Select(res => new BHowTo(
                res.RowId.ToString(), ((InstanceContentType)res.InstanceContentType.RowId).ToString(), res.ContentFinderCondition.Value.Name.ToString()
            )).ToArray();
            _lCompanion = Gl<Companion>(i => !UIStateInstance->IsCompanionUnlocked(i.RowId) && i.Icon != 0).Select(res => new BT3(
                res.RowId.ToString(),
                res.Icon,
                res.Singular.ToString())).ToArray();
        }
        catch (Exception e) {
            Log.Error(e.ToString());
        }
    }

    private static void Start(string cmd) => Process.Start(new ProcessStartInfo(cmd) {
        UseShellExecute = true
    });

    private static void ToCsv<T>(string[] header, T[] data, string csvName) {
        csvName += ".csv";
        if (ImGui.Button($"导出到 {csvName}")) {
            var sb = new StringBuilder(string.Join(",", header)).Append('\n');
            foreach (var p in data) sb.Append(p).Append('\n');
            File.WriteAllText(Path.Combine(Plugin.Configuration.SavePath, csvName), sb.ToString(), Encoding.UTF8);
            NotificationManager.AddNotification(new Notification {
                Title = csvName,
                Content = $"已导出到 {csvName}"
            });
        }
    }

    private static void NewTable<T>(string[] header, T[] data, Action<T>[] acts, string? csvName = null, Func<T, string>[]? filter = null, string? filterTag = null) {
        if (csvName != null) ToCsv(header, data, csvName);
        var datax = (data.Clone() as T[])!;
        if (ImGui.BeginTable("Table", acts.Length, ImGuiTableFlag)) {
            foreach (var item in header) {
                if (item == "") ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 24);
                else if (item.Contains("序号")) ImGui.TableSetupColumn(item, ImGuiTableColumnFlags.WidthFixed, 96);
                else ImGui.TableSetupColumn(item, ImGuiTableColumnFlags.WidthStretch);
            }
            ImGui.TableHeadersRow();
            if (filter != null && filterTag != null) {
                var filterdata = new string[acts.Length];
                for (var i = 0; i < filterdata.Length; i++) filterdata[i] = "";
                ImGui.TableNextRow();
                for (var i = 0; i < acts.Length; i++) {
                    if (header[i].IsNullOrEmpty()) continue;
                    ImGui.TableSetColumnIndex(i);
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.InputText($"##Filter{i}", ref filterdata[i])) {
                        for (var j = 0; j < acts.Length; j++) {
                            if (header[j].IsNullOrEmpty()) continue;
                            datax = datax.Where(x => filter[j](x).Contains(filterdata[j])).ToArray();
                        }
                    }
                }
            }
            foreach (var res in datax) {
                ImGui.TableNextRow();
                for (var i = 0; i < acts.Length; i++) {
                    ImGui.TableSetColumnIndex(i);
                    acts[i](res);
                }
            }
            ImGui.EndTable();
        }
    }

    private static void NewTab(string tabname, Action act) {
        if (ImGui.BeginTabItem(tabname)) {
            act();
            ImGui.EndTabItem();
        }
    }

    private static void RenderIcon(int id) {
        if (TextureProvider.TryGetFromGameIcon((uint)id, out var tex) && tex.TryGetWrap(out var icon, out _))
            ImGui.Image(icon.Handle, new Vector2(24, 24));
    }

    public override void Draw() {
        if (ImGui.InputText("保存路径", ref _configuration.SavePath, 114514)) _configuration.Save();
        ImGui.SameLine();
        if (ImGui.Button("打开输出目录")) {
            MkDir();
            Start(_configuration.SavePath);
        }
        if (ImGui.Button("刷新")) RefreshData();
        if (ImGui.BeginTabBar("tab")) {
            NewTab("制作笔记", () => {
                if (ImGui.Button("导出到Artisan")) {
                    var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
                    foreach (var p in _lRecipe.Select(acc => BRecipe.GetMaterial(acc.Name)))
                        if (p != 0)
                            sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
                    sb.Append("]}");
                    var s = sb.ToString();
                    ImGui.SetClipboardText(s);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = s
                    });
                }
                ImGui.SameLine();
                if (ImGui.Button("导出前50到Artisan")) {
                    var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
                    var iter = 0;
                    foreach (var p in _lRecipe.Select(acc => BRecipe.GetMaterial(acc.Name)))
                        if (p != 0) {
                            if (iter++ == 49) break;
                            sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
                        }
                    sb.Append("]}");
                    var s = sb.ToString();
                    ImGui.SetClipboardText(s);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制前50",
                        Content = s
                    });
                }
                NewTable(BRecipe.Header, _lRecipe, BRecipe.Acts, "制作笔记", BRecipe.Filters, "_lRecipe");
            });
            NewTab("钓鱼", () => {
                if (ImGui.Button("导出到鱼糕")) {
                    var sb = new StringBuilder("{\"completed\":[");
                    foreach (var res in _lFishCaught) sb.Append(res).Append(',');
                    foreach (var res in _lsFishCaught) sb.Append(res).Append(',');
                    sb.Remove(sb.Length - 1, 1).Append("]}");
                    WriteFile("fish.json", sb.ToString());
                    Log.Info("导完了");
                }
                ImGui.SameLine();
                if (ImGui.Button("打开鱼糕")) Start("https://fish.ffmomola.com/ng/#/wiki/fishing");
                ImGui.SameLine();
                ToCsv(BUncaughtFish.Header, _lFishUnCaught.Concat(_lsFishUnCaught).ToArray(), "钓鱼");
                NewTable(BUncaughtFish.Header, _lFishUnCaught, BUncaughtFish.Acts, filter: BUncaughtFish.Filters, filterTag: "_lFishUnCaught");
                NewTable(BUncaughtFish.Header, _lsFishUnCaught, BUncaughtFish.Acts, filter: BUncaughtFish.Filters, filterTag: "_lsFishUnCaught");
            });
            NewTab("成就", () => {
                if (_lAchievement != null) {
                    if (ImGui.Button("一键获取空数据(可能会卡死)"))
                        foreach (var res in _lAchievement) {
                            if (AchievementServiceInstance.Current.ContainsKey(res._rowId)) continue;
                            AchievementServiceInstance.UpdateProgress(res._rowId);
                        }
                    ImGui.SameLine();
                    if (ImGui.Button("重置队列(可清除卡死)")) AchievementServiceInstance.Reset();
                    ImGui.SameLine();
                    if (ImGui.Button("重置获取到的数据")) {
                        AchievementServiceInstance.Reset();
                        AchievementServiceInstance.Current.Clear();
                        AchievementServiceInstance.Max.Clear();
                    }
                    ImGui.SameLine();
                    NewTable(BAchievement.Header, _lAchievement, BAchievement.Acts, "成就", BAchievement.Filters, "_lAchievement");
                }
                else ImGui.Text("打开一次成就界面以刷新");
            });
            NewTab("幻卡", () => {
                if (ImGui.Button("导出到arrtripletriad.com")) {
                    var sb = new StringBuilder("[false,");
                    unsafe {
                        var bitArray = new BitArray(UIState.Instance()->UnlockedTripleTriadCardsBitmask.ToArray());
                        for (var l = 1; l < bitArray.Count; l++) sb.Append(bitArray[l - 1].ToString().ToLower()).Append(',');
                    }
                    sb.Remove(sb.Length - 1, 1).Append(']');
                    WriteFile("ttc.json", sb.ToString());
                    Log.Info("导完了");
                }
                ImGui.SameLine();
                if (ImGui.Button("打开arrtripletriad.com")) Start("https://arrtripletriad.com/cn/huan-ka-yi-lan");
                ImGui.SameLine();
                NewTable(BT2.Header, _lTripleTriadCard, BT2.Acts, "幻卡", BT2.Filters, "_lTripleTriadCard");
            });
            NewTab("理符", () => {
                if (ImGui.Button("导出已接受的可制作物品到Artisan")) {
                    var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
                    foreach (var p in _lLeveAccepted.Select(acc => BRecipe.GetMaterial(acc.ItemName)))
                        if (p != 0)
                            sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
                    sb.Append("]}");
                    var s = sb.ToString();
                    ImGui.SetClipboardText(s);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = s
                    });
                }
                ImGui.SameLine();
                if (ImGui.Button("导出所有的可制作物品到Artisan")) {
                    var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
                    foreach (var p in _lLeve.Select(acc => BRecipe.GetMaterial(acc.ItemName)))
                        if (p != 0)
                            sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
                    sb.Append("]}");
                    var s = sb.ToString();
                    ImGui.SetClipboardText(s);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = s
                    });
                }
                ImGui.SameLine();
                if (ImGui.Button("导出所有的可制作物品前50到Artisan")) {
                    var sb = new StringBuilder("{\"Name\":\"New\",\"Recipes\":[");
                    var iter = 0;
                    foreach (var p in _lLeve.Select(acc => BRecipe.GetMaterial(acc.ItemName)))
                        if (p != 0) {
                            if (iter++ == 49) break;
                            sb.Append($"{{\"ID\":{p},\"Quantity\":1,\"ListItemOptions\":{{\"NQOnly\":false,\"Skipping\":false}}}},");
                        }
                    sb.Append("]}");
                    var s = sb.ToString();
                    ImGui.SetClipboardText(s);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制前50",
                        Content = s
                    });
                }
                ImGui.SameLine();
                if (ImGui.Button("导出所有的不可制作物品")) {
                    var sb = new StringBuilder();
                    foreach (var p in _lLeve.Select(acc => BRecipe.GetNonMaterial(acc.ItemName)))
                        if (p is not null)
                            sb.Append($"{p},");
                    var s = sb.ToString();
                    ImGui.SetClipboardText(s);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = s
                    });
                }
                ImGui.SameLine();
                ToCsv(BLeve.Header, _lLeve, "理符");
                ImGui.Text("已接受理符");
                NewTable(BLeve.Header, _lLeveAccepted, BLeve.Acts, filter: BLeve.Filters, filterTag: "_lLeveAccepted");
                ImGui.Text("所有理符");
                NewTable(BLeve.Header, _lLeve, BLeve.Acts, filter: BLeve.Filters, filterTag: "_lLeve");
            });
            NewTab("任务", () => NewTable(BQuest.Header, _lQuest, BQuest.Acts, "任务", BQuest.Filters, "_lQuest"));
            NewTab("乐谱", () => NewTable(BT2.Header, _lOrchestrion, BT2.Acts, "乐谱", BT2.Filters, "_lOrchestrion"));
            NewTab("坐骑", () => NewTable(BT3.Header, _lMount, BT3.Acts, "坐骑", BT3.Filters, "_lMount"));
            NewTab("装饰", () => NewTable(BT3.Header, _lOrnament, BT3.Acts, "装饰", BT3.Filters, "_lOrnament"));
            NewTab("眼镜", () => NewTable(BT3.Header, _lGlasses, BT3.Acts, "眼镜", BT3.Filters, "_lGlasses"));
            NewTab("表情", () => NewTable(BT3.Header, _lEmote, BT3.Acts, "表情", BT3.Filters, "_lEmote"));
            NewTab("教程", () => NewTable(BHowTo.Header, _lHowto, BHowTo.Acts, "教程", BHowTo.Filters, "_lHowto"));
            NewTab("宠物", () => NewTable(BT3.Header, _lCompanion, BT3.Acts, "宠物", BT3.Filters, "_lCompanion"));
            NewTab("副本", () => NewTable(BHowTo.Header, lInstanceContent, BHowTo.Acts, "副本", BHowTo.Filters, "_lInstanceContent"));
            // NewTab("陆行鸟车", () => {
            // 	unsafe {
            // 		NewTable(["序号", "名称"], Gl<ChocoboTaxiStand>(i =>
            // 			!UIState.Instance()->IsChocoboTaxiStandUnlocked(i.RowId)), [
            // 			res => ImGui.Text(res.RowId.ToString()),
            // 			res => ImGui.Text(res.PlaceName.ToString())
            // 		]);
            // 	}
            // });
            // NewTab("过场动画", () => {
            //     unsafe {
            //         NewTable([], Gl<Cutscene>(i =>
            //             !UIState.Instance()->IsCutsceneSeen(i.RowId)), [
            //             res => ImGui.Text(res.RowId.ToString()),
            //             res => ImGui.Text(res.Path.ToString()),
            //         ]);
            //     }
            // });
            ImGui.EndTabBar();
        }
    }

    #region Beans

    private class BRecipe(string rowId, int icon, string name) {
        internal static readonly string[] Header = ["序号", "", "名称"];
        internal static readonly Action<BRecipe>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => RenderIcon(res.Icon),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            }
        ];
        internal static readonly Func<BRecipe, string>[] Filters = [
            res => res.RowId,
            _ => "",
            res => res.Name
        ];
        private readonly int Icon = icon;
        internal readonly string Name = name;
        private readonly string RowId = rowId;

        public override string ToString() => $"{RowId},,{Name}";

        internal static BRecipe[] GetData() => DataManager.GameData.GetExcelSheet<Recipe>()!
            .Select(x => new {
                x,
                a = x.RowId
            })
            .Where(t => t.a < 30000 && !QuestManager.IsRecipeComplete(t.a))
            .Select(t => t.x.ItemResult.Value)
            .Where(t => t.Name != "").Select(res => new BRecipe(
                res.RowId.ToString(),
                res.Icon,
                res.Name.ToString()
            )).ToArray();

        internal static uint GetMaterial(string name) {
            try {
                return DataManager.GameData.GetExcelSheet<Recipe>()!.First(i => i.ItemResult.Value.Name == name).RowId;
            }
            catch (Exception) {
                return 0;
            }
        }

        internal static string? GetNonMaterial(string name) {
            try {
                _ = DataManager.GameData.GetExcelSheet<Recipe>()!.First(i => i.ItemResult.Value.Name == name);
                return null;
            }
            catch (Exception) {
                return name;
            }
        }
    }

    private class BUncaughtFish(string rowId, string itemId, string name, string place) {
        internal static readonly string[] Header = ["序号", "鱼糕序号", "名称", "地点"];
        internal static readonly Action<BUncaughtFish>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => ImGui.Text(res.ItemId),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            },
            res => ImGui.Text(res.Place)
        ];
        internal static readonly Func<BUncaughtFish, string>[] Filters = [
            res => res.RowId,
            res => res.ItemId,
            res => res.Name,
            res => res.Place
        ];
        private readonly string ItemId = itemId;
        private readonly string Name = name;
        private readonly string Place = place;
        private readonly string RowId = rowId;
        public override string ToString() => $"{RowId},{ItemId},{Name},{Place}";
    }

    private class BAchievement(uint rowId, int icon, string name, string points, string category) {
        internal static readonly string[] Header = ["序号", "", "名称", "成就点", "分类", "进度", "查询"];
        internal static readonly Action<BAchievement>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => RenderIcon(res.Icon),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            },
            res => ImGui.Text(res.Points),
            res => ImGui.Text(res.Category),
            res => ImGui.Text(GetProcess(res)),
            res => {
                if (ImGui.Button($"查询:{res.Name}")) AchievementServiceInstance.UpdateProgress(res._rowId);
            }
        ];
        internal static readonly Func<BAchievement, string>[] Filters = [
            res => res.RowId,
            _ => "",
            res => res.Name,
            res => res.Points,
            res => res.Category,
            _ => "",
            _ => ""
        ];
        internal readonly uint _rowId = rowId;
        private readonly string Category = category;
        private readonly int Icon = icon;
        private readonly string Name = name;
        private readonly string Points = points;
        private readonly string RowId = rowId.ToString();
        private static string GetProcess(BAchievement res) => !AchievementServiceInstance.Current.TryGetValue(res._rowId, out var x) ? "" : $"{x}/{AchievementServiceInstance.Max[res._rowId]}";
        public override string ToString() => $"{RowId},,{Name},{Points},{Category},{GetProcess(this)},";

        internal static unsafe BAchievement[] GetData() => Gl<Lumina.Excel.Sheets.Achievement>(i =>
            !_achievementInstance->IsComplete((int)i.RowId) && i.Name != "" && !i.AchievementHideCondition.Value.HideAchievement && i.AchievementCategory.Value.AchievementKind.Value.RowId is not (13 or 8 or 0)).Select(res => new BAchievement(
            res.RowId,
            res.Icon,
            res.Name.ToString(),
            res.Points.ToString(),
            res.AchievementCategory.Value.Name.ToString()
        )).ToArray();
    }

    private class BLeve(uint rowId, string job, string lv, string name, string startzone, string startplace, string npc, string itemName, string itemCount) {
        internal static readonly string[] Header = ["序号", "职业", "等级", "名称", "开始区域", "开始地点", "NPC", "提交道具", "道具数量"];
        internal static readonly Action<BLeve>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => ImGui.Text(res.Job),
            res => ImGui.Text(res.Lv),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            },
            res => ImGui.Text(res.StartZone),
            res => ImGui.Text(res.StartPlace),
            res => ImGui.Text(res.Npc),
            res => {
                if (res.ItemName == "") {
                    ImGui.Text("");
                    return;
                }
                if (ImGui.Button(res.ItemName)) {
                    ImGui.SetClipboardText(res.ItemName);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.ItemName
                    });
                }
            },
            res => ImGui.Text(res.ItemCount)
        ];
        internal static readonly Func<BLeve, string>[] Filters = [
            res => res.RowId,
            res => res.Job,
            res => res.Lv,
            res => res.Name,
            res => res.StartZone,
            res => res.StartPlace,
            res => res.Npc,
            res => res.ItemName,
            res => res.ItemCount
        ];
        internal readonly uint _rowId = rowId;
        private readonly string ItemCount = itemCount;
        internal readonly string ItemName = itemName;
        private readonly string Job = job;
        private readonly string Lv = lv;
        private readonly string Name = name;
        private readonly string Npc = npc;
        private readonly string RowId = rowId.ToString();
        private readonly string StartPlace = startplace;
        private readonly string StartZone = startzone;
        public override string ToString() => $"{RowId},{Job},{Lv},{Name},{StartZone},{StartPlace},{Npc},{ItemName},{ItemCount}";

        internal static unsafe BLeve[] GetData() {
            var lLevex = Gl<Leve>(i => !_questManagerInstance->IsLevequestComplete((ushort)i.RowId) && i.AllowanceCost != 0 && i is { RowId: > 0, GilReward: > 0 }).ToArray();
            var lLeveName = lLevex.Select(i => i.Name);
            var lDcLeves = new Dictionary<ReadOnlySeString, CraftLeve>();
            foreach (var x in Gl<CraftLeve>(i => lLeveName.Contains(i.Leve.Value.Name)))
                lDcLeves[x.Leve.Value.Name] = x;
            return lLevex.Select(res => {
                var itemN = "";
                var itemC = "";
                if (lDcLeves.TryGetValue(res.Name, out var value1)) {
                    itemN = value1.Item[0].Value.Name.ToString();
                    itemC = value1.ItemCount[0].ToString();
                }
                return new BLeve(
                    res.RowId,
                    res.ClassJobCategory.Value.Name.ToString(),
                    res.ClassJobLevel.ToString(),
                    res.Name.ToString(),
                    res.PlaceNameStartZone.Value.Name.ToString(),
                    res.PlaceNameStart.Value.Name.ToString(),
                    res.LeveClient.Value.Name.ToString(),
                    itemN,
                    itemC
                );
            }).ToArray();
        }
    }

    private class BHowTo(string rowId, string category, string name) {
        internal static readonly string[] Header = ["序号", "分类", "名称"];
        internal static readonly Action<BHowTo>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => ImGui.Text(res.Category),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            }
        ];
        internal static readonly Func<BHowTo, string>[] Filters = [
            res => res.RowId,
            res => res.Category,
            res => res.Name
        ];
        private readonly string Category = category;
        private readonly string Name = name;
        private readonly string RowId = rowId;
        public override string ToString() => $"{RowId},{Category},{Name}";
    }

    private class BT2(string rowId, string name) {
        internal static readonly string[] Header = ["序号", "名称"];
        internal static readonly Action<BT2>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            }
        ];
        internal static readonly Func<BT2, string>[] Filters = [
            res => res.RowId,
            res => res.Name
        ];
        private readonly string Name = name;
        private readonly string RowId = rowId;
        public override string ToString() => $"{RowId},{Name}";
    }

    private class BT3(string rowId, int icon, string name) {
        internal static readonly string[] Header = ["序号", "", "名称"];
        internal static readonly Action<BT3>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => RenderIcon(res.Icon),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            }
        ];
        internal static readonly Func<BT3, string>[] Filters = [
            res => res.RowId,
            _ => "",
            res => res.Name
        ];
        private readonly int Icon = icon;
        private readonly string Name = name;
        private readonly string RowId = rowId;
        public override string ToString() => $"{RowId},,{Name}";
    }

    private class BQuest(string rowId, string lv, int icon, string name, string place, string category1, string category2, string category3) {
        internal static readonly string[] Header = ["序号", "等级", "", "名称", "地点", "分类1", "分类2", "分类3"];
        internal static readonly Action<BQuest>[] Acts = [
            res => ImGui.Text(res.RowId),
            res => ImGui.Text(res.Lv),
            res => RenderIcon(res.Icon),
            res => {
                if (ImGui.Button(res.Name)) {
                    ImGui.SetClipboardText(res.Name);
                    NotificationManager.AddNotification(new Notification {
                        Title = "已复制",
                        Content = res.Name
                    });
                }
            },
            res => ImGui.Text(res.Place),
            res => ImGui.Text(res.Category1),
            res => ImGui.Text(res.Category2),
            res => ImGui.Text(res.Category3)
        ];
        internal static readonly Func<BQuest, string>[] Filters = [
            res => res.RowId,
            res => res.Lv,
            _ => "",
            res => res.Name,
            res => res.Place,
            res => res.Category1,
            res => res.Category2,
            res => res.Category3
        ];
        private readonly string Category1 = category1;
        private readonly string Category2 = category2;
        private readonly string Category3 = category3;
        private readonly int Icon = icon;
        private readonly string Lv = lv;
        private readonly string Name = name;
        private readonly string Place = place;
        private readonly string RowId = rowId;
        public override string ToString() => $"{RowId},{Lv},,{Name},{Place},{Category1},{Category2},{Category3}";

        internal static unsafe BQuest[] GetData() =>
            Gl<Quest>(i => !UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted((ushort)i.RowId + 0x10000u) && i.PlaceName.Value.Name != "" && i.JournalGenre.RowId > 0 && i.JournalGenre.Value.JournalCategory.Value.RowId != 96).Select(res => new BQuest(
                res.RowId.ToString(),
                res.ClassJobLevel[0].ToString(),
                res.JournalGenre.Value.Icon,
                res.Name.ToString(),
                res.PlaceName.Value.Name.ToString(),
                res.JournalGenre.Value.Name.ToString(),
                res.JournalGenre.Value.JournalCategory.Value.Name.ToString(),
                res.JournalGenre.Value.JournalCategory.Value.JournalSection.Value.Name.ToString()
            )).ToArray();
    }

    #endregion
}