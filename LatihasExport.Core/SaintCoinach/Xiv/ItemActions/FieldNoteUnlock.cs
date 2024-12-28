using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv.ItemActions {
    public class FieldNoteUnlock : ItemAction {
        // Used to unlock a certain field note in bozja.

        private const int FieldNoteKey = 0;

        public FieldNoteUnlock(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        public MYCWarResultNotebook FieldNote {
            get {
                return Sheet.Collection.GetSheet<MYCWarResultNotebook>()[GetData(FieldNoteKey)];
            }
        }
    }
}
