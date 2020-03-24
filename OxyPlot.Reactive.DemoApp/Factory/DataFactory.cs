using Endless;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OxyPlotEx.DemoApp
{
    public class DataFactory
    {
        static readonly Random random = new Random();
        static readonly string[] arr = new[] { "a", "b", "c" };

        Dictionary<string, IEnumerator<int>> xc = arr.ToDictionary(c => c, c => (IEnumerator<int>)new InfiniteIncrementSequence(random.Next(0, 3)));
        Dictionary<string, IEnumerator<int>> xc2 = arr.ToDictionary(c => c, c => (IEnumerator<int>)new InfiniteSequence(random.Next(0, 3)));

        private readonly Dictionary<string, int> dict;

        string NextCharacter() => arr[random.Next(0, 3)];


        public DataFactory()
        {
            dict = arr.ToDictionary(a => a, a => 0);
        }

        public IEnumerable<KeyValuePair<string, double>> GetSin()
        {
            var chr = NextCharacter();

            while (xc[chr].MoveNext())
            {
                yield return new KeyValuePair<string, double>(chr, Math.Sin(((1+xc[chr].Current) + (1 + random.NextDouble())) / 4));
                chr = NextCharacter();
            }
        }
        public IEnumerable<KeyValuePair<string, double>> GetLineAscending()
        {
            var chr = NextCharacter();

            while (xc[chr].MoveNext())
            {
                var xx = new KeyValuePair<string, double>(chr, (1+xc[chr].Current )/ (1+random.NextDouble()) );
                yield return xx;
                chr = NextCharacter();
            }
        }

        public IEnumerable<KeyValuePair<string, double>> GetLine()
        {
            var chr = NextCharacter();

            while (xc2[chr].MoveNext())
            {
                var xx = new KeyValuePair<string, double>(chr,(1+ xc2[chr].Current )/ (1 + random.NextDouble()));
                yield return xx;
                chr = NextCharacter();
            }
        }

        public IEnumerable<(string, string,double)> GetLine2()
        {
            var chr = NextCharacter();

            while (xc2[chr].MoveNext())
            {
                dict[chr] = ++dict[chr];

                yield return (chr, dict[chr].ToString() ,( xc2[chr].Current+1) / (1 + random.NextDouble())); ;
                chr = NextCharacter();
            }
        }
    }



    class InfiniteIncrementSequence : IEnumerator<int>
    {

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position;

        public InfiniteIncrementSequence(int seed = -1)
        {
            position = seed;
        }

        public bool MoveNext()
        {
            position++;
            return true;
        }

        public void Reset()
        {
            position = -1;
        }

        public void Dispose()
        {
        }

        public int Current
        {
            get
            {
                try
                {
                    return position;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;
    }

    class InfiniteSequence : IEnumerator<int>
    {

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position;

        public InfiniteSequence(int seed = -1)
        {
            position = seed;
        }

        public bool MoveNext()
        { 
            return true;
        }

        public void Reset()
        {
            position = -1;
        }

        public void Dispose()
        {
        }

        public int Current
        {
            get
            {
                try
                {
                    return position;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;
    }
}




