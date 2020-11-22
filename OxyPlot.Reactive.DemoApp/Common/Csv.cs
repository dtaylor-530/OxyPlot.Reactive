using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;

namespace OxyPlot.Reactive.DemoApp.Common
{
    class Csv : IDisposable
    {
        CompositeDisposable compositeDisposable = new CompositeDisposable();
        public IEnumerable<CsvRow> Read()
        {

            var reader = new StreamReader("../../../Data/Temp2.csv");
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            compositeDisposable.Add(reader);
            compositeDisposable.Add(csv);

            var records = csv.GetRecords<CsvRow>();
            return records;
        }

        public void Dispose()
        {
            compositeDisposable.Dispose();
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

