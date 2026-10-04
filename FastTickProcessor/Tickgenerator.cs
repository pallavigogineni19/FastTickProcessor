using FastTickProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastTickProcessor
{

    public static class TickGenerator
    {
        // Writes lines like:  1790861400123|AAPL|18742|100|B
        //                     timestamp   |sym |price |qty|side
        public static void Generate(string path, int count, int seed = 42)
        {
            var rng = new Random(seed);                       // fixed seed = same data every run
            int[] prices = (int[])Symbols.BasePricesCents.Clone();
            long ts = new DateTimeOffset(2026, 10, 1, 13, 30, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

            using var writer = new StreamWriter(path, append: false, Encoding.ASCII, bufferSize: 1 << 16)
            {
                NewLine = "\n"
            };

            // Format each line into a stack buffer: no string allocated per tick.
            Span<char> buf = stackalloc char[64];

            for (int i = 0; i < count; i++)
            {
                ts += rng.Next(0, 5);                         // 0-4 ms between ticks
                int id = rng.Next(Symbols.Names.Length);

                // Random walk: price moves up to +/- 3 cents, never below $1.00
                prices[id] = Math.Max(100, prices[id] + rng.Next(-3, 4));

                int qty = rng.Next(1, 11) * 10;               // 10..100 shares
                char side = rng.Next(2) == 0 ? 'B' : 'S';

                int pos = 0, written;

                ts.TryFormat(buf, out written); pos += written;
                buf[pos++] = '|';

                string name = Symbols.Names[id];
                name.AsSpan().CopyTo(buf[pos..]); pos += name.Length;
                buf[pos++] = '|';

                prices[id].TryFormat(buf[pos..], out written); pos += written;
                buf[pos++] = '|';

                qty.TryFormat(buf[pos..], out written); pos += written;
                buf[pos++] = '|';
                buf[pos++] = side;

                writer.WriteLine(buf[..pos]);
            }
        }
    }
}
