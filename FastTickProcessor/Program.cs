using System.Diagnostics;
using FastTickProcessor;

string path = Path.Combine(Path.GetTempPath(), "ticks.txt");
//const int Count = 5_000_000;
const int Count = 20;

// ---------- Stage 1: generate data ----------
var genWatch = Stopwatch.StartNew();
TickGenerator.Generate(path, Count);
genWatch.Stop();

Console.WriteLine($"[Stage 1] Generated {Count:N0} ticks in {genWatch.ElapsedMilliseconds} ms " +
                  $"({new FileInfo(path).Length / (1024 * 1024)} MB)");

// ---------- Stage 2: parse + process ----------
var processor = new TickProcessor();

// Warm-up on a throwaway processor so JIT compilation doesn't skew the timing.
await TickFileReader.ProcessFileAsync(path, new TickProcessor());

GC.Collect();
GC.WaitForPendingFinalizers();

// GetTotalAllocatedBytes counts allocations across ALL threads, so it stays correct
// even if the code after an await resumes on a different thread.
long allocBefore = GC.GetTotalAllocatedBytes(precise: true);
int gen0Before = GC.CollectionCount(0);
var sw = Stopwatch.StartNew();

var result = await TickFileReader.ProcessFileAsync(path, processor);

sw.Stop();
long allocated = GC.GetTotalAllocatedBytes(precise: true) - allocBefore;
int gen0 = GC.CollectionCount(0) - gen0Before;

processor.PrintReport(Console.Out);

double seconds = sw.Elapsed.TotalSeconds;
Console.WriteLine();
Console.WriteLine($"[Stage 2] Parsed {result.Parsed:N0} ticks ({result.Bad} bad) in {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"          Throughput:      {result.Parsed / seconds / 1_000_000:F2} million ticks/sec");
Console.WriteLine($"          Heap allocated:  {allocated / 1024.0:F1} KB");
Console.WriteLine($"          Gen0 GCs:        {gen0}");

//File.Delete(path);