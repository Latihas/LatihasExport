using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintCoinach;
using SaintCoinach.Ex;
using SaintCoinach.Xiv;
using static LatihasExport.Beans;
using Achievement = SaintCoinach.Xiv.Achievement;

namespace LatihasExport;

public class DumpAssets {
	private readonly ARealmReversed realm;

	internal DumpAssets(Process p) {
		var fullp = p.MainModule?.FileName;
		var GameDirectory = fullp?.Substring(0, fullp.Length - @"game\ffxiv_dx11.exe".Length - 1);
		realm = new ARealmReversed(GameDirectory, Language.ChineseSimplified);
	}

	internal IEnumerable<BFish> GetValidFishParameter() {
		return from i in realm.GameData.GetSheet<FishParameter>()
			where i.IsInLog && i.Item.Name != ""
			let it = i.Item
			select new BFish(i.Key, it.Key, it.Name, it.Description);
	}

	internal IEnumerable<BFish> GetValidSpearfishingItem() {
		return from i in realm.GameData.GetSheet<SpearfishingItem>()
			where i.IsVisible
			let it = i.Item
			select new BFish(i.Key, it.Key, it.Name, it.Description);
	}

	internal List<BAchievement> GetValidAchievement() {
		return realm.GameData.GetSheet<Achievement>().Select(i =>
			new BAchievement(i.Key, i.Points, i.Name, i.AchievementCategory.ToString(), i.Description)).ToList();
	}

	internal List<Recipe> GetValidRecipe() {
		return realm.GameData.GetSheet<Recipe>().Where(i =>
			i.RecipeLevelTable.Stars == 0
			&& i.Key != 0
			&& i.Key < 30000
			&& i.ToString() != "").ToList();
	}
}