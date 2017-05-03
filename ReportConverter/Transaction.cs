using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportConverter
{
    public class Transaction
    {
        public string Date { get; set; }
        public string Account { get; set; }
        public string CounterAccount { get; set; }
        public string Type { get; set; }
        public string Amount { get; set; }
        public List<string> StatementLines { get; set; }
    }
}
