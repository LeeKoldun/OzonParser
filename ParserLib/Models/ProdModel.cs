using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserLib.Models
{
    public class ProdModel
    {
        public ProdModel() { }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProdParams { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string RatingCount { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImgUrl { get; set; } = string.Empty;
        public SellerModel Seller { get; set; } = new();
    }
}
