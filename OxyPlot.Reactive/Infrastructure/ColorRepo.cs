using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxyPlot.Reactive.Infrastructure
{
    internal class ColorRepo
    {
        //private static Dictionary<string, string> ColorsMatch = new Dictionary<string, string>();
        //private static IEnumerator<KeyValuePair<string, string>> Colors2;
        private static Random random = new Random();

        public static OxyColor GetColor(string key) => GetColor(Math.Abs(key.Length == 1 ? key.First() - 'a' : key.GetHashCode()));

        //private static OxyColor NextColor()
        //{
        //    Colors2.MoveNext(); return OxyColor.Parse(Colors2.Current.Value);
        //}
        public static OxyColor GetColor(int? key)
        {
                lock (LazyColors)
                    return LazyColors[key ?? random.Next(0, 1000)];
        }

        /// <summary>
        /// A nicer color palette for the web.
        /// https://github.com/mrmrs/colors
        /// </summary>
        /// <returns></returns>
        //private static OxyColor GetColorRandom(string key)
        //{
        //    if (ColorsMatch.ContainsKey(key) == false)
        //    {
        //        Colors2.MoveNext();
        //        ColorsMatch[key] = Colors2.Current.Key;
        //    }
        //    return Colors.Value[ColorsMatch[key]];
        //}

        private static phirSOFT.LazyDictionary.LazyDictionary<int, OxyColor> LazyColors = new phirSOFT.LazyDictionary.LazyDictionary<int, OxyColor>(a =>
        {
            if (NiceColors.Value.ContainsKey(a))
                return NiceColors.Value[a];
            else
            {
                var get = ToDistinctNumberCombination(a, NiceColors.Value.Count);
                return Blend(NiceColors.Value[get.Item1], NiceColors.Value[get.Item2], random.NextDouble());
            }
        });

        /// <summary>
        /// A nicer color palette for the web.
        /// https://github.com/mrmrs/colors
        /// </summary>
        private static Lazy<Dictionary<int, OxyColor>> NiceColors = new Lazy<Dictionary<int, OxyColor>>(() =>
            new Dictionary<string, string>
            {
                { "navy", "#001F3F"} ,

                { "blue", "#0074D9"} ,
                { "aqua", "#7FDBFF"} ,
                { "teal", "#39CCCC"} ,
                { "olive", "#3D9970"} ,
                { "green", "#2ECC40"} ,
                { "lime", "#01FF70"} ,
                {  "yellow", "#FFDC00"} ,
                {  "orange", "#FF851B"} ,
                { "red", "#FF4136"} ,
                { "fuchsia", "#F012BE"} ,

                { "purple", "#B10DC9"} ,

                { "maroon", "#85144B"} ,
                //{ "white", "#FFFFFF"} ,
                //{ "gray", "#AAAAAA"} ,
                { "silver", "#DDDDDD"} ,
                { "black", "#111111"}
            }.Index().ToDictionary(a => a.Key, a => OxyColor.Parse(a.Value.Value)));

        /// <summary>Blends the specified colors together.</summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of <paramref name="color"/> to keep,
        /// “on top of” <paramref name="backColor"/>.</param>
        /// <returns>The blended colors.</returns>
        public static OxyColor Blend(OxyColor color, OxyColor backColor, double amount)
        {
            byte a = (byte)((color.A * amount) + backColor.A * (1 - amount));
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return OxyColor.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Returns two numbers unique to <see cref="num"/> between 0 and <see cref="mod"/>
        /// </summary>
        /// <param name="num"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static (int, int) ToDistinctNumberCombination(int num, int mod)
        {
            var first = num % mod;

            var second = ((int)((1d * num - first) / mod)) % mod;

            return (first, second);
        }
    }
}