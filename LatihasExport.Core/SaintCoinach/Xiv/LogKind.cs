using SaintCoinach.Ex.Relational;
using SaintCoinach.Text;

namespace SaintCoinach.Xiv {
    public class LogKind : XivRow {
        #region Properties

        public XivString Format { get { return AsString("Format"); } }
        public XivString Name { get { return AsString("Name"); } }
        public XivString Example { get { return AsString("Example"); } }

        #endregion

        #region Constructors

        #region Constructor

        public LogKind(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        public override string ToString() {
            return Name;
        }
    }
}
