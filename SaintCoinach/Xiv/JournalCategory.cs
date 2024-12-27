using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    public class JournalCategory : XivRow {
        #region Properties

        public Text.XivString Name => AsString("Name");
        public JournalSection JournalSection => As<JournalSection>();

        #endregion

        #region Constructors

        public JournalCategory(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion
    }
}
