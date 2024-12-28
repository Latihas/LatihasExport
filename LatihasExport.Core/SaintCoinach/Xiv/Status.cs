using SaintCoinach.Ex.Relational;
using SaintCoinach.Imaging;
using SaintCoinach.Text;

namespace SaintCoinach.Xiv {
    public class Status : XivRow {
        #region Properties

        public XivString Name { get { return AsString("Name"); } }
        public XivString Description { get { return AsString("Description"); } }
        public ImageFile Icon { get { return AsImage("Icon"); } }
        public bool CanDispel { get { return AsBoolean("CanDispel"); } }
        public byte Category { get { return As<byte>("StatusCategory"); } }

        #endregion

        #region Constructors

        #region Constructor

        public Status(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        public override string ToString() {
            return Name;
        }
    }
}
