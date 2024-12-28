using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv.ItemActions {
    public class AdventureBook : ItemAction {
        // Used to skip parts of the story or level a class.

        public AdventureBook(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }
    }
}
