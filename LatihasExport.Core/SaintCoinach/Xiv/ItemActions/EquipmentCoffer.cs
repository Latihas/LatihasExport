using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv.ItemActions {
    public class EquipmentCoffer : ItemAction {
        // Used to add several pieces of equipment to inventory.
        // Only usuable within a major town.

        public EquipmentCoffer(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }
    }
}
