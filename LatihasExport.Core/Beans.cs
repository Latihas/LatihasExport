namespace LatihasExport.Core;

using static Beans.BFishType;
using static DumpAssets;

public static class Beans {
	internal enum BFishType {
		Unknown,
		Spear,
		Fishing
	}

	internal record BFish(int InlineId, int YgId, string Name, string Description) {
		public const string Header = "完成情况,类型,内部序号,鱼糕序号,名字,描述\n";
		public readonly int InlineId = InlineId, YgId = YgId;
		public readonly string Name = Name, Description = Description.Replace("\r\n", "");
		public bool Completed;
		public BFishType Type = Unknown;

		public override string ToString() {
			return $"{Completed},{Type},{InlineId},{YgId},{Name},{Description}\n";
		}
	}

	internal record BRecipe(int Id, int Level, string Name, string Job, string Category, string Ing, string Description) {
		public const string Header = "序号,等级,名字,职业,分类,材料,描述\n";
		public readonly int Id = Id, Level = Level;
		public readonly string Name = Name, Job = Job, Category = Category, Ing = Ing, Description = Description.Replace("\r\n", "");

		public override string ToString() {
			return $"{Id},{Level},{Name},{Job},{Category},{Ing},{Description}\n";
		}
	}

	internal record BSpecial(SpecialType Category, int Id, string Name) {
		public const string Header = "完成情况,分类,序号,描述\n";
		public readonly int Id = Id;
		public readonly string Name = Name;
		public readonly SpecialType Category = Category;
		public bool Completed;

		public override string ToString() {
			return $"{Completed},{Category},{Id},{Name}\n";
		}
	}

	internal record BAchievement(int Id, int Points, string Name, string AchievementCategory, string Description) {
		public const string Header = "完成情况,序号,点数,名字,分类,描述\n";
		public readonly int Id = Id, Points = Points;
		public readonly string Name = Name, AchievementCategory = AchievementCategory, Description = Description.Replace("\r\n", "");
		public bool Completed;

		public override string ToString() {
			return $"{Completed},{Id},{Points},{Name},{AchievementCategory},{Description}\n";
		}
	}
}