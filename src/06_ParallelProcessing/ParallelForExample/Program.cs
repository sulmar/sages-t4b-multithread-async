﻿
using System.Diagnostics;
using System.Threading.Tasks;

Console.WriteLine("Hello, Parallel.For!");

Console.WriteLine($"CPU: {Environment.ProcessorCount} rdzeni");

const int count = 20;

//MeasureForExecutionTimeSync(count);
//MeasureForExecutionTimeTask(count);
MeasureForExecutionTimeParallel(count);

void MeasureForExecutionTimeParallel(int count)
{
    CancellationTokenSource cts = new CancellationTokenSource(1000);

    var items = Enumerable.Range(0, count);

    var options = new ParallelOptions
    { 
        MaxDegreeOfParallelism = 4, // Przepustnica
        CancellationToken = cts.Token,        
    };

    IProgress<string> progress = new Progress<string>(message => Console.WriteLine(message));

    var stopwatch = Stopwatch.StartNew();
    
    // równoległa pętla for(int i = 0; i < count) { DoWork(i); } 
    Parallel.For(0, count, options, i => DoWork(i, progress));

    stopwatch.Stop();


    Console.WriteLine($"Czas wykonania (Parallel): {stopwatch.ElapsedMilliseconds} ms");

}

Console.ReadKey();

async Task MeasureForExecutionTimeTask(int count)
{
    Console.WriteLine("Wykonanie asynchroniczne...");

    // dławik / przepustnica
    SemaphoreSlim? throtler = new SemaphoreSlim(4);

    var stopwatch = Stopwatch.StartNew();

    var items = Enumerable.Range(0, count);

    var tasks = items.Select(async i =>
    {
        await throtler.WaitAsync();

        await DoWorkAsync(i);

        throtler.Release();

    });
    await Task.WhenAll(tasks);

    stopwatch.Stop();


    Console.WriteLine($"Czas wykonania (taski): {stopwatch.ElapsedMilliseconds} ms");
}



void MeasureForExecutionTimeSync(int count)
{
    Console.WriteLine("Wykonanie sekwencyjne...");

    var stopwatch = Stopwatch.StartNew();

    for (int i = 0; i < count; i++)
    {
        DoWork(i);
    }

    stopwatch.Stop();


    Console.WriteLine($"Czas wykonania (sekwencyjnie): {stopwatch.ElapsedMilliseconds} ms");
}

static async Task DoWorkAsync(int item)
{
    Console.WriteLine($"Przetwarzanie {item} na wątku {Thread.CurrentThread.ManagedThreadId}");
    Thread.SpinWait(1_000_000); // Obciążenie CPU

    await Task.Delay(100); // Symulacja opoźnienia
    Console.WriteLine($"Zakonczono {item} na wątku {Thread.CurrentThread.ManagedThreadId}");
}
// Symulacja operacji CPU-bound czasochłonnej
static void DoWork(int item, IProgress<string> progress = null)
{
    progress?.Report($"Przetwarzanie {item} na wątku {Thread.CurrentThread.ManagedThreadId}");
    Thread.SpinWait(1_000_000); // Obciążenie CPU
    Thread.Sleep(100); // Symulacja opoźnienia
    progress?.Report($"Zakonczono {item} na wątku {Thread.CurrentThread.ManagedThreadId}");
}
