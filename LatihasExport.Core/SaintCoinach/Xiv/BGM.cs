using SaintCoinach.Ex.Relational;
using SaintCoinach.Text;

namespace SaintCoinach.Xiv {
    // ReSharper disable once InconsistentNaming
    public class BGM : XivRow {
        #region Properties

        public XivString File { get { return AsString("File"); } }

        #endregion

        #region Constructors

        #region Constructor

        public BGM(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        public override string ToString() {
            return File;
        }
    }
}
