using SpreadsheetLight;

namespace ParserLib {
    public static class ExcelParser {
        private const string _contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private static string[] _template = {
            "ID",
            "Name",
            "Description",
            "Params",
            "Price",
            "Url",
            "Shop",
            "ShopUrl",
            "Ogrn",
            "NDS",
            "Rating",
            "RatingCount",
            "ImgUrl"
        };

        public static void ConvertProdToExcel(ProdModel[] prods) {
            using(SLDocument sl = new SLDocument()) {
                for(int i = 0; i < _template.Length; i++) {
                    sl.SetCellValue(1, i, _template[i]);
                }

                for(int i = 0; i < prods.Length; i++) {
                    var prod = prods[i];
                    sl.SetCellValue(i + 2, 0, i + 1); // ID
                    sl.SetCellValue(i + 2, 1, prod.Title); // Name
                    sl.SetCellValue(i + 2, 2, prod.Description); // Description
                }

                sl.SaveAs("Report.xls");
            }
        }
    }
}
