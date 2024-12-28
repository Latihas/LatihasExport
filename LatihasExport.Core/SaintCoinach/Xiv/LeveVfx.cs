using SaintCoinach.Ex.Relational;
using SaintCoinach.Imaging;
using SaintCoinach.Text;

namespace SaintCoinach.Xiv {
    public class LeveVfx : XivRow {
        #region Properties

        public XivString Effect { get { return AsString("Effect"); } }

        public ImageFile Icon { get { return AsImage("Icon"); } }

        #endregion

        #region Constructors

        public LeveVfx(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion
    }
}
