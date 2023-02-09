using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ordering
{
    public class OrderModel
    {
        public string sku { get; set; }
        public int Id { get; set; }

        public Regex regex { get; set; }

        public double authorityValue { get; set; }
    }

    public class CSVModel
    {
        public string SKU { get; set; }
        public int ID { get; set; }
    }

}
