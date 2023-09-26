using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserLib {
    public class ProdModel {
        public ProdModel() { }
        public ProdModel(
            string title,
            string desc,
            string prodParams
        ) {
            Title = title;
            Description = desc;
            ProdParams = prodParams;
        }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProdParams { get; set; } = string.Empty;
    }
}
