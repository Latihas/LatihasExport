﻿using System;
using Newtonsoft.Json.Linq;
using SaintCoinach.Ex.Relational.Definition;
using SaintCoinach.Imaging;

namespace SaintCoinach.Ex.Relational.ValueConverters {
    public class IconConverter : IValueConverter {
        #region IValueConverter Members

        public string TargetTypeName { get { return "Image"; } }
        public Type TargetType { get { return typeof(ImageFile); } }

        public object Convert(IDataRow row, object rawValue) {
            var nr = System.Convert.ToInt32(rawValue);
            if (nr <= 0 || nr > 999999)
                return null;

            var sheet = row.Sheet;
            return IconHelper.GetIcon(sheet.Collection.PackCollection, sheet.Language, nr);
        }

        #endregion

        #region Serialization

        public JObject ToJson() {
            return new JObject {
                ["type"] = "icon"
            };
        }

        public static IconConverter FromJson(JToken obj) {
            return new IconConverter();
        }

        public void ResolveReferences(SheetDefinition sheetDef) { }

        #endregion
    }
}
