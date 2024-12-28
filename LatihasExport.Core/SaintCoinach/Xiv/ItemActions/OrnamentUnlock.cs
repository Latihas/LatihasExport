using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv.ItemActions {
    public class OrnamentUnlock : ItemAction {
        // Used to unlock fashion accessory, such as parasol.

        private const int OrnamentKey = 0;
        
        public OrnamentUnlock(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        public Ornament Ornament { 
            get {
                var key = GetData(OrnamentKey);
                return Sheet.Collection.GetSheet<Ornament>()[key];
            }
        }
    }
}
