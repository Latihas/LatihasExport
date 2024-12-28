namespace SaintCoinach.Xiv {
    /// <summary>
    /// Class containing data about a BNpc.
    /// </summary>
    public class BNpcData {
        #region Fields
        private BNpcBase _Base;
        private BNpcName _Name;
        #endregion

        #region Constructor
        public BNpcData(XivCollection collection, Libra.BNpcName libraRow) {
            _Base = collection.GetSheet<BNpcBase>()[(int)libraRow.BaseKey];
            _Name = collection.GetSheet<BNpcName>()[(int)libraRow.NameKey];
        }
        #endregion
    }
}
