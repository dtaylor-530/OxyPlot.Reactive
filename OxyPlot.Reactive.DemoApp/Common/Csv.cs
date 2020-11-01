using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OxyPlot.Reactive.DemoApp.Common
{
    class Csv
    {

        public static CsvRow[] Read()
        
        {
            using (var reader = new StreamReader("../../../Data/Temp2.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CsvRow>().ToArray();
                return records;
            }
        }



    }

    public class CsvRow
    {
        public double Odd { get; set; }
        public double Wager { get; set; }
        public double Profit { get; set; }

        public long DateTime { get; set; }

        public DateTime DateTime_ => new DateTime(DateTime);

    }
}

