using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Data.Factory
{
    public class DataFactory
    {
        private static readonly Random random = new Random();
        private static readonly string[] arr = new[] { "a", "b", "c" };

        private Dictionary<string, IEnumerator<int>> xc = arr.ToDictionary(c => c, c => (IEnumerator<int>)new InfiniteIncrementSequence(random.Next(0, 3)));
        private Dictionary<string, IEnumerator<int>> xc2 = arr.ToDictionary(c => c, c => (IEnumerator<int>)new InfiniteSequence(random.Next(0, 3)));

        private readonly Dictionary<string, int> dict;

        private string NextCharacter() => arr[random.Next(0, 3)];

        public DataFactory()
        {
            dict = arr.ToDictionary(a => a, a => 0);
        }

        public IEnumerable<KeyValuePair<string, double>> GetSin()
        {
            var chr = NextCharacter();

            while (xc[chr].MoveNext())
            {
                yield return new KeyValuePair<string, double>(chr, Math.Sin((1 + xc[chr].Current + (1 + random.NextDouble())) / 4));
                chr = NextCharacter();
            }
        }

        public IEnumerable<KeyValuePair<string, double>> GetXCubed()
        {
            var chr = NextCharacter();

            while (xc[chr].MoveNext())
            {
                var next = (1 + xc[chr].Current + (1 + random.NextDouble())) / 4;
                var pow3= Math.Pow(next, 3);
                yield return new KeyValuePair<string, double>(chr, pow3);
                chr = NextCharacter();
            }
        }

        public IEnumerable<KeyValuePair<string, double>> GetLineAscending()
        {
            var chr = NextCharacter();

            while (xc[chr].MoveNext())
            {
                var xx = new KeyValuePair<string, double>(chr, (1 + xc[chr].Current) / (1 + random.NextDouble()));
                yield return xx;
                chr = NextCharacter();
            }
        }

        public IEnumerable<KeyValuePair<string, double>> GetLine()
        {
            var chr = NextCharacter();

            while (xc2[chr].MoveNext())
            {
                var xx = new KeyValuePair<string, double>(chr, (1 + xc2[chr].Current) / (1 + random.NextDouble()));
                yield return xx;
                chr = NextCharacter();
            }
        }

        public IEnumerable<(string, string, double)> GetLine2(int change = 1)
        {
            var chr = NextCharacter();
            int i = 0;
            while (xc2[chr].MoveNext())
            {
                dict[chr] = ++dict[chr];

                yield return (chr, dict[chr].ToString(), (xc2[chr].Current + 1) / (1 + random.NextDouble()));
                if (i++ % change == 0)
                    chr = NextCharacter();
            }
        }

        public IEnumerable<(string, string, double)> GetLine3(int change = 1)
        {
            var chr = NextCharacter();
            int i = 0;
            while (xc2[chr].MoveNext())
            {
                dict[chr] = ++dict[chr];

                yield return (chr, chr, (xc2[chr].Current + 1) / (1 + random.NextDouble()));
                if (i++ % change == 0)
                    chr = NextCharacter();
            }
        }
    }

    internal class InfiniteIncrementSequence : IEnumerator<int>
    {
        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        private int position;

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

    internal class InfiniteSequence : IEnumerator<int>
    {
        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        private int position;

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