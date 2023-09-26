using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserLib.Models {
    public class SellerModel {
        public SellerModel() { }

        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Ogrn { get; set; } = string.Empty;
        public string Nds { get; set; } = string.Empty;
    }
}
