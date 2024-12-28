using SaintCoinach.Ex.Relational;
using SaintCoinach.Imaging;
using SaintCoinach.Text;

namespace SaintCoinach.Xiv {
    public class Weather : XivRow {
        #region Properties

        public XivString Name { get { return AsString("Name"); } }
        public XivString Description { get { return AsString("Description"); } }
        public ImageFile Icon { get { return AsImage("Icon"); } }

        #endregion

        #region Constructors

        #region Constructor

        public Weather(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        #region ToString

        public override string ToString() {
            return Name;
        }

        #endregion
    }
}
