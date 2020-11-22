using Kaos.Collections;
using ReactivePlot.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;

namespace ReactivePlot.Ex
{

    class Constants
    {
        public const string X = "x";
    }

    public class DataGridPlotModel : IDataGridPlotModel<IDoublePoint<string>>, IAddData<IDoublePoint<string>>
    {

        DataTable dsfs = new DataTable();

        public DataGridPlotModel(DataGrid plotModel)
        {
            this.PlotModel = plotModel;

        }

        public DataGrid PlotModel { get; }


        public virtual void AddData(IDoublePoint<string>[] items, string title, int? index = null)
        {
            dsfs.Add(items, title);
        }


        public virtual bool RemoveSeries(string title)
        {
            lock (PlotModel)
            {
                if ((PlotModel.Columns.SingleOrDefault(a => a.Header.ToString() == title) is { } column))
                {
                    PlotModel.Columns.Remove(column);

                }

                return false;
            }
        }

        public virtual void ClearSeries()
        {
            lock (PlotModel)
            {
                PlotModel.Columns.Clear();
            }
        }



        public virtual void Invalidate(bool v)
        {
            AddXColumn();
            AddColumns();
            AddItems();

            void AddXColumn()
            {
                if (PlotModel.Columns.Count == 0)
                {
                    var columnx = new DataGridTextColumn
                    {
                        Header = Constants.X,
                        Binding = new Binding(Constants.X)
                    };
                    PlotModel.Columns.Add(columnx);
                }
            }


            void AddColumns()
            {
                while (dsfs.Titlesqueue.TryDequeue(out var title))
                    if (!(PlotModel.Columns.SingleOrDefault(a => a.Header.ToString() == title) is { } column))
                    {
                        column = new DataGridTextColumn
                        {
                            Header = title,
                            Binding = new Binding(title),
                            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                        };

                        //if (index.HasValue)
                        //    PlotModel.Columns.Insert(index.Value, column);
                        //else
                        PlotModel.Columns.Add(column);
                        //titles.Remove(title);
                    }
            }

            void AddItems()
            {
                while (dsfs.ItemsQueue.TryDequeue(out var dsf))
                {
                    var (rowIndex, row) = dsf;
                    if (rowIndex.HasValue && rowIndex < PlotModel.Items.Count)
                    {
                        PlotModel.Items.RemoveAt(rowIndex.Value);
                        PlotModel.Items.Insert(rowIndex.Value, row);
                    }
                    else
                    {
                        PlotModel.Items.Add(row);
                    }

                }
            }
        }
    }

    class DataTable
    {
        RankedDictionary<double, Dictionary<string, double>> valuesDictionary = new RankedDictionary<double, Dictionary<string, double>>();
        HashSet<string> titles = new HashSet<string>();

        public ConcurrentQueue<string> Titlesqueue { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<(int?, ExpandoObject)> ItemsQueue { get; } = new ConcurrentQueue<(int?, ExpandoObject)>();

        public void Add(IDoublePoint<string>[] items, string title)
        {

            var keys = from xy in items.Select(a => { var (x, y) = a; return (x, y); })
                       join key in valuesDictionary.Keys on true equals true
                       into temp
                       let s = temp.SingleOrDefault(a => a == xy.x)
                       select (xy.x, xy, !s.Equals(default));

            var arr = keys.ToArray();

            //if (index.HasValue)
            //{
            //    //throw new NotImplementedException();
            //    titles.Add(title);
            //    //foreach (var kvp in titles)
            //    //{
            //    //    var idx = titles[kvp.Key];
            //    //    if (idx > index.Value)
            //    //        titles[kvp.Key] = idx + 1;
            //    //}
            //}
            //else
            titles.Add(title);
            Titlesqueue.Enqueue(title);
            List<(int?, ExpandoObject)> listtemp = new List<(int?, ExpandoObject)>();

            foreach (var (key, (x, y), match) in arr)
            {

                var values = valuesDictionary[key] = match ? valuesDictionary[key] : new Dictionary<string, double>();
                values[title] = y;

                ExpandoObject row = new ExpandoObject();
                row.TryAdd(Constants.X, key);
                foreach (var kvp in titles)
                {
                    if (values.TryGetValue(kvp, out double value))
                        row.TryAdd(kvp, value);
                    else
                        row.TryAdd(kvp, null);
                }

                if (match)
                {
                    var indexInValues = valuesDictionary.IndexOfKey(key);
                    //PlotModel.Items.RemoveAt(indexInValues);
                    //PlotModel.Items.Insert(indexInValues, row);
                    listtemp.Add((indexInValues, row));


                }
                else
                {
                    if (valuesDictionary.TryGetLessThan(x, out var ssdf))
                    {
                        var indexInValues = valuesDictionary.IndexOfKey(ssdf.Key);
                        //PlotModel.Items.Insert(indexInValues, row);
                        listtemp.Add((indexInValues, row));
                    }
                    else
                        //PlotModel.Items.Add(row);
                        listtemp.Add((default, row));
                }
            }

            foreach (var item in listtemp)
                ItemsQueue.Enqueue(item);
        }
    }
}
