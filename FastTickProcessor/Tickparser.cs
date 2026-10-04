using FastTickProcessor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastTickProcessor
{

    public static class TickParser
    {
        // Parses: 1790861400123|AAPL|18742|100|B
        // Takes a span, returns a struct through `out`: zero heap allocations per line.
        public static bool TryParse(ReadOnlySpan<char> line, out Tick tick)
        {
            tick = default;
            var reader = new FieldReader(line);

            if (!reader.TryNext(out var tsText) ||
                !long.TryParse(tsText, NumberStyles.None, CultureInfo.InvariantCulture, out long ts))
                return false;

            if (!reader.TryNext(out var symText) || !Symbols.TryGetId(symText, out int symbolId))
                return false;

            if (!reader.TryNext(out var priceText) ||
                !int.TryParse(priceText, NumberStyles.None, CultureInfo.InvariantCulture, out int price))
                return false;

            if (!reader.TryNext(out var qtyText) ||
                !int.TryParse(qtyText, NumberStyles.None, CultureInfo.InvariantCulture, out int qty))
                return false;

            if (!reader.TryNext(out var sideText) || sideText.Length != 1)
                return false;

            Side side;
            switch (sideText[0])
            {
                case 'B': side = Side.Buy; break;
                case 'S': side = Side.Sell; break;
                default: return false;
            }

            tick = new Tick(ts, symbolId, price, qty, side);
            return true;
        }
    }
}
