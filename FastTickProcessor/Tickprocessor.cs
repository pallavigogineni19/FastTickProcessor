using FastTickProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastTickProcessor
{


    // Mutable struct holding running stats for one symbol. Stored in an array, updated in place.
    public struct SymbolStats
    {
        public long Count;
        public long Volume;
        public long NotionalCents;   // sum of price * quantity, used to compute VWAP
        public int Last;
        public int High;
        public int Low;
    }

    // sealed: nothing can inherit from it, so the JIT can call its methods directly and inline them.
    public sealed class TickProcessor
    {
        private readonly SymbolStats[] _stats = new SymbolStats[Symbols.Names.Length];

        public TickProcessor()
        {
            for (int i = 0; i < _stats.Length; i++)
                _stats[i].Low = int.MaxValue;
        }

        // `in` passes the 24-byte Tick by reference instead of copying it.
        public void Process(in Tick tick)
        {
            // ref local: modify the struct inside the array directly (no copy-modify-store).
            ref SymbolStats s = ref _stats[tick.SymbolId];

            s.Count++;
            s.Volume += tick.Quantity;
            s.NotionalCents += (long)tick.PriceCents * tick.Quantity;
            s.Last = tick.PriceCents;
            if (tick.PriceCents > s.High) s.High = tick.PriceCents;
            if (tick.PriceCents < s.Low) s.Low = tick.PriceCents;
        }

        public void PrintReport(TextWriter output)
        {
            output.WriteLine();
            output.WriteLine($"{"Symbol",-7}{"Company",-11}{"Ticks",10}{"Volume",14}{"Last",10}{"High",10}{"Low",10}{"VWAP",10}");

            // Plain for loop: no LINQ in the hot/report path.
            for (int i = 0; i < _stats.Length; i++)
            {
                ref readonly SymbolStats s = ref _stats[i];
                if (s.Count == 0) continue;

                long vwapCents = s.Volume == 0 ? 0 : s.NotionalCents / s.Volume;

                output.WriteLine(
                    $"{Symbols.Names[i],-7}{Symbols.CompanyNames[i],-11}{s.Count,10:N0}{s.Volume,14:N0}" +
                    $"{Dollars(s.Last),10}{Dollars(s.High),10}{Dollars(s.Low),10}{Dollars(vwapCents),10}");
            }
        }

        private static string Dollars(long cents) => $"{cents / 100}.{cents % 100:00}";
    }
}
