using FastTickProcessor;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastTickProcessor
{


    public sealed class ReadCounters
    {
        public long Parsed;
        public long Bad;
    }

    public static class TickFileReader
    {
        // ValueTask: no Task allocation when the result is ready synchronously.
        public static async ValueTask<ReadCounters> ProcessFileAsync(string path, TickProcessor processor)
        {
            var counters = new ReadCounters();

            // ArrayPool: rent one 64K-char buffer and reuse it for the whole file,
            // instead of allocating a string for every line.
            char[] buffer = ArrayPool<char>.Shared.Rent(64 * 1024);
            try
            {
                 using var reader = new StreamReader(path); // testing
               /*  using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                     FileShare.Read, bufferSize: 1, useAsync: true);
                using var reader = new StreamReader(stream, Encoding.ASCII,
                    detectEncodingFromByteOrderMarks: false, bufferSize: 64 * 1024); */
                int carry = 0;   // chars left over from a partial line at the end of the last read

                while (true)
                {
                    int read = await reader.ReadAsync(buffer.AsMemory(carry));
                    if (read == 0) break;

                    carry = ProcessLines(buffer, carry + read, processor, counters);
                }

                // last line without a trailing newline
                if (carry > 0)
                    HandleLine(buffer.AsSpan(0, carry), processor, counters);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            return counters;
        }

        // Kept as a normal (non-async) method on purpose: spans can't live across an await.
        // Returns how many chars were left over (a partial line), moved to the start of the buffer.
        private static int ProcessLines(char[] buffer, int end, TickProcessor processor, ReadCounters counters)
        {
            int start = 0;

            while (true)
            {
                ReadOnlySpan<char> window = buffer.AsSpan(start, end - start);
                int newline = window.IndexOf('\n');
                if (newline < 0) break;

                HandleLine(window[..newline].TrimEnd('\r'), processor, counters);   // slice, not Substring
                start += newline + 1;
            }

            int leftover = end - start;
            buffer.AsSpan(start, leftover).CopyTo(buffer);
            return leftover;
        }

        private static void HandleLine(ReadOnlySpan<char> line, TickProcessor processor, ReadCounters counters)
        {
            if (TickParser.TryParse(line, out Tick tick))
            {
                counters.Parsed++;
                processor.Process(in tick);
            }
            else
            {
                counters.Bad++;
            }
        }
    }
}