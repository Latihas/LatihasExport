using SaintCoinach.Ex;
using SaintCoinach.Ex.Relational;
using SaintCoinach.Imaging;
using SaintCoinach.Text;

namespace SaintCoinach.Xiv {
    public class Mount : XivRow, IQuantifiableXivString {
        #region Properties

        public XivString Singular { get { return AsString("Singular"); } }
        public XivString Plural { get { return Sheet.Collection.ActiveLanguage == Language.Japanese ? Singular : AsString("Plural"); } }
        public XivString Description { get { return AsString("Description"); } }
        public XivString GuideDescription { get { return AsString("Description{Enhanced}"); } }
        public XivString Tooltip { get { return AsString("Tooltip"); } }
        public ImageFile Icon { get { return AsImage("Icon"); } }
        public ModelChara ModelChara => As<ModelChara>();

        #endregion

        #region Constructors

        #region Constructor

        public Mount(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        public override string ToString() {
            return Singular;
        }

        #region IQuantifiableName Members
        string IQuantifiable.Singular {
            get { return Singular; }
        }

        string IQuantifiable.Plural {
            get { return Plural; }
        }
        #endregion
    }
}
