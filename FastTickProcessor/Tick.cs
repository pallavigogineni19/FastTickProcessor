using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastTickProcessor
{
    public enum Side : byte { Buy, Sell }

    // One price update. A readonly struct: no heap object per tick, safe to copy, immutable.
    // Price is stored as integer cents (18742 = $187.42) to avoid floating-point rounding issues
    // and keep the struct small. Size is about 24 bytes.
    public readonly record struct Tick(
        long TimestampMs,   // Unix time in milliseconds
        int SymbolId,       // index into Symbols.Names (an int is cheaper than a string)
        int PriceCents,
        int Quantity,
        Side Side);
}




