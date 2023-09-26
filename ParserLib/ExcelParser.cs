using ParserLib.Models;
using SpreadsheetLight;

namespace ParserLib
{
    public static class ExcelParser {
        public static SLDocument Sl { get; set; }
        public static int RowIndex { get; set; } = 2;

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

        public static void InitSheet() {
            Sl = new SLDocument();
            for (int i = 0; i < _template.Length; i++) {
                Sl.SetCellValue(1, i + 1, _template[i]);
            }
        }

        public static void ConvertProdToExcel(ProdModel prod) {
            SellerModel seller = prod.Seller;

            Sl.SetCellValue(RowIndex, 1, RowIndex - 1); // ID
            Sl.SetCellValue(RowIndex, 2, prod.Title); // Name
            Sl.SetCellValue(RowIndex, 3, prod.Description); // Description
            Sl.SetCellValue(RowIndex, 4, prod.ProdParams); // Params
            Sl.SetCellValue(RowIndex, 5, prod.Price); // Price
            Sl.SetCellValue(RowIndex, 6, prod.Url); // Url

            Sl.SetCellValue(RowIndex, 7, seller.Name); // Shop
            Sl.SetCellValue(RowIndex, 8, seller.Url); // ShopUrl
            Sl.SetCellValue(RowIndex, 9, seller.Ogrn); // Ogrn
            Sl.SetCellValue(RowIndex, 10, seller.Nds); // Nds

            Sl.SetCellValue(RowIndex, 11, prod.Rating); // Rating
            Sl.SetCellValue(RowIndex, 12, prod.RatingCount); // RatingCount
            Sl.SetCellValue(RowIndex, 13, prod.ImgUrl); // ImgUrl

            RowIndex++;
        }

        public static void SaveSheet() => Sl.SaveAs("ResultTable.xls");
    }
}
