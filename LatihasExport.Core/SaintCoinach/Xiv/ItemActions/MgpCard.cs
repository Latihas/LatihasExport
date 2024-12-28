using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv.ItemActions
{
    public class MgpCard : ItemAction {
        #region Static

        private const int AmountKey = 0;

        #endregion

        #region Properties

        public int Amount {
            get { return GetData(AmountKey); }
        }

        #endregion

        #region Constructors

        #region Constructor

        public MgpCard(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion
    }
}
