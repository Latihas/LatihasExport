using SaintCoinach.Ex.Relational;
using SaintCoinach.Imaging;
using SaintCoinach.Text;

namespace SaintCoinach.Xiv {
    public class Trait : XivRow {
        #region Properties

        public XivString Name { get { return AsString("Name"); } }
        public XivString Description { get { return AsString("Description"); } }
        public ImageFile Icon { get { return AsImage("Icon"); } }
        public ClassJob ClassJob { get { return As<ClassJob>(); } }
        public int Level { get { return AsInt32("Level"); } }
        public ClassJobCategory ClassJobCategory { get { return As<ClassJobCategory>(); } }
        public int Value { get { return AsInt32("Value"); } }

        #endregion

        #region Constructors

        #region Constructor

        public Trait(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        public override string ToString() {
            return Name;
        }
    }
}
