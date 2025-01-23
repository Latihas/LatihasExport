namespace LatihasExport.Core;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SaintCoinach;
using SaintCoinach.Ex;
using SaintCoinach.Xiv;
using static Beans;
using Achievement = SaintCoinach.Xiv.Achievement;

public class DumpAssets {
	private readonly ARealmReversed realm;

	internal DumpAssets(string gd) {
		realm = new ARealmReversed(gd, Language.ChineseSimplified);
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
		return realm.GameData.GetSheet<Achievement>().Where(i => i.Points != 0).Select(i =>
			new BAchievement(i.Key, i.Points, i.Name, i.AchievementCategory.ToString(), i.Description)).ToList();
	}

	internal enum SpecialType {
		Mount,
		Ornament
	}

	// ReSharper disable once UnusedMember.Global
	internal List<BSpecial> GetValidSpecial() {
		var res = realm.GameData.GetSheet<Mount>().Where(i => i.ToString() != "").Select(i => new BSpecial(SpecialType.Mount, i.Key, i.ToString())).ToList();
		res.AddRange(realm.GameData.GetSheet<Ornament>().Where(i => i.ToString() != "").Select(i => new BSpecial(SpecialType.Ornament, i.Key, i.ToString())));
		return res;
	}

	internal List<Recipe> GetValidRecipe() {
		return realm.GameData.GetSheet<Recipe>().Where(i => i.Key < 30000 && i.ToString() != "").ToList();
	}
}