using ParserLib.Models;
using SpreadsheetLight;

namespace ParserLib
{
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

        public static void ConvertProdToExcel(List<ProdModel> prods) {
            using(SLDocument sl = new SLDocument()) {
                for(int i = 0; i < _template.Length; i++) {
                    sl.SetCellValue(1, i + 1, _template[i]);
                }

                for(int i = 0; i < prods.Count; i++) {
                    ProdModel prod = prods[i];
                    SellerModel seller = prod.Seller;

                    sl.SetCellValue(i + 2, 1, i + 1); // ID
                    sl.SetCellValue(i + 2, 2, prod.Title); // Name
                    sl.SetCellValue(i + 2, 3, prod.Description); // Description
                    sl.SetCellValue(i + 2, 4, prod.ProdParams); // Params
                    sl.SetCellValue(i + 2, 5, prod.Price); // Price
                    sl.SetCellValue(i + 2, 6, prod.Url); // Url

                    sl.SetCellValue(i + 2, 7, seller.Name); // Shop
                    sl.SetCellValue(i + 2, 8, seller.Url); // ShopUrl
                    sl.SetCellValue(i + 2, 9, seller.Ogrn); // Ogrn
                    sl.SetCellValue(i + 2, 10, seller.Nds); // Nds

                    sl.SetCellValue(i + 2, 11, prod.Rating); // Rating
                    sl.SetCellValue(i + 2, 12, prod.RatingCount); // RatingCount
                    sl.SetCellValue(i + 2, 13, prod.ImgUrl); // ImgUrl
                }

                sl.SaveAs("TestTable.xls");
            }
        }
    }
}
