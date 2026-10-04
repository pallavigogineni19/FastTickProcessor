using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastTickProcessor
{


    public static class Symbols
    {
        public static readonly string[] Names =
            { "AAPL", "MSFT", "GOOG", "AMZN", "TSLA", "NVDA", "META", "NFLX" };

        // Starting prices in cents, same order as Names.
        public static readonly int[] BasePricesCents =
            { 18700, 41500, 17200, 18500, 24800, 12100, 50200, 62000 };

        // FrozenDictionary: built once, optimized for fast read-only lookups.
        // Used at report time to turn a symbol id into a display name.
        public static readonly FrozenDictionary<int, string> CompanyNames =
            new Dictionary<int, string>
            {
                [0] = "Apple",
                [1] = "Microsoft",
                [2] = "Alphabet",
                [3] = "Amazon",
                [4] = "Tesla",
                [5] = "Nvidia",
                [6] = "Meta",
                [7] = "Netflix"
            }.ToFrozenDictionary();

        // Looks up a symbol id straight from a span: no string is created for the lookup.
        public static bool TryGetId(ReadOnlySpan<char> symbol, out int id)
        {
            for (int i = 0; i < Names.Length; i++)
            {
                if (symbol.SequenceEqual(Names[i].AsSpan()))
                {
                    id = i;
                    return true;
                }
            }
            id = -1;
            return false;
        }
    }
}
