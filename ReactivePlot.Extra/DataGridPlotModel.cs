using Kaos.Collections;
using ReactivePlot.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace ReactivePlot.Ex
{

    class Constants
    {
        public const string X = "x";
    }

    public class DataGridPlotModel<TType> : DataGridPlotModel<double, TType>
        where TType : IKeyPoint<string, double, double>
    {
        public DataGridPlotModel(DataGrid plotModel) : base(plotModel)
        {
        }
    }

    public class DataGridDateTimePlotModel<TType> : DataGridPlotModel<DateTime, TType>
       where TType : IKeyPoint<string, DateTime, double>
    {
        public DataGridDateTimePlotModel(DataGrid plotModel) : base(plotModel)
        {
        }
    }

    public class DataGridPlotModel<TVar, TType> :
        IDataGridPlotModel<TType>,
        IAddSeries<TType>
        where TVar : IEquatable<TVar>
        where TType : IPoint<TVar, double>
    {
        int i = 0;
        private readonly int limit;
        private readonly DataTable<TVar, TType> dataTable = new DataTable<TVar, TType>();

        public DataGridPlotModel(DataGrid plotModel, int limit =100)
        {
            this.PlotModel = plotModel;
            this.limit = limit;
        }

        public DataGrid PlotModel { get; }


        public virtual void AddSeries(IReadOnlyCollection<TType> items, string title, int? index = null)
        {
            try
            {
                dataTable.Add(items, title);
            }
            catch (Exception ex)
            {

            }
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

        public virtual void Clear()
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
                while (dataTable.Titlesqueue.TryDequeue(out var title))
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
                // Only add 100 items;
                while (dataTable.ItemsQueue.TryDequeue(out var dsf) && (i++ <= limit))
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

    class DataTable<TVar, TType>
        where TVar : IEquatable<TVar>
        where TType : IPoint<TVar, double>
    {
        readonly RankedDictionary<TVar, Dictionary<string, double>> valuesDictionary = new RankedDictionary<TVar, Dictionary<string, double>>();
        readonly HashSet<string> titles = new HashSet<string>();


        public ConcurrentQueue<string> Titlesqueue { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<(int?, ExpandoObject)> ItemsQueue { get; } = new ConcurrentQueue<(int?, ExpandoObject)>();

        public void Add(IReadOnlyCollection<TType> items, string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new Exception("Title cannot be empty");

            ((TVar x, double y) xy, bool)[] arr = null;
            lock (valuesDictionary)
            {
                var keys = from xy in items.Select(a => { var (x, y) = a; return (x, y); })
                           join key in valuesDictionary.Keys on true equals true
                           into temp
                           let s = temp.SingleOrDefault(a => a.Equals(xy.x))
                           select ((xy.x, xy.y), !s.Equals(default));

                arr = keys.ToArray();
            }

            titles.Add(title);
            Titlesqueue.Enqueue(title);
            List<(int?, ExpandoObject)> listtemp = new List<(int?, ExpandoObject)>();

            foreach (var ((x, y), match) in arr)
            {

                var values = valuesDictionary[x] = match ? valuesDictionary[x] : new Dictionary<string, double>();
                values[title] = y;

                ExpandoObject row = new ExpandoObject();
                row.TryAdd(Constants.X, x);
                foreach (var kvpKey in titles)
                {
                    if (values.TryGetValue(kvpKey, out double value))
                        row.TryAdd(kvpKey.ToString()!, value);
                    else
                        row.TryAdd(kvpKey.ToString()!, null);
                }

                if (match)
                {
                    var indexInValues = valuesDictionary.IndexOfKey(x);
                    //PlotModel.Items.RemoveAt(indexInValues);
                    //PlotModel.Items.Insert(indexInValues, row);
                    if (indexInValues < 0)
                    {

                    }
                    listtemp.Add((indexInValues, row));
                }
                else
                {
                    if (valuesDictionary.TryGetLessThan(x, out var ssdf))
                    {
                        var indexInValues = valuesDictionary.IndexOfKey(ssdf.Key);
                        if (indexInValues < 0)
                        {

                        }
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
