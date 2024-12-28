using System;
using Newtonsoft.Json.Linq;
using SaintCoinach.Ex.Relational.Definition;

namespace SaintCoinach.Ex.Relational.ValueConverters {
    public class GenericReferenceConverter : IValueConverter {

        #region IValueConverter Members

        public string TargetTypeName { get { return "Row"; } }
        public Type TargetType { get { return typeof(IRelationalRow); } }

        public object Convert(IDataRow row, object rawValue) {
            var coll = (RelationalExCollection)row.Sheet.Collection;
            var key = System.Convert.ToInt32(rawValue);
            return coll.FindReference(key);
        }

        #endregion

        #region Serialization

        public JObject ToJson() {
            return new JObject {
                ["type"] = "generic"
            };
        }

        public static GenericReferenceConverter FromJson(JToken obj) {
            return new GenericReferenceConverter();
        }

        public void ResolveReferences(SheetDefinition sheetDef) { }

        #endregion
    }
}
