﻿// See https://aka.ms/new-console-template for more information
using AsyncAwaitExample;

Console.WriteLine("Hello, Tasks!");

Printer printer = new Printer();
CostCalculator calculator = new CostCalculator();

// Synchroniczny
printer.Print("Document #1");
var cost = calculator.Calculate("Document #1");
Console.WriteLine($"Cost: {cost:C2}");

// Asynchroniczny
printer.PrintAsync("Document #1")
    .ContinueWith(taskPrint => calculator.CalculateAsync("Document #1")
        .ContinueWith(calculateTask => Console.WriteLine($"Cost: {calculateTask.Result:C2}")));

PrinterService printerService = new PrinterService();

Task print1Task = printerService.PrintAndCalculateAsync();

Task print2Task = printerService.PrintAndCalculateAsync();

await Task.WhenAll(print1Task, print2Task);

// await Task.WhenAny(print1Task, print2Task);


Console.WriteLine("Finished.");
Console.ReadLine();




public class Printer
{
    public Task InitAsync()
    {
        return Task.CompletedTask;
    }

    public Printer()
    {
        Console.WriteLine("Printer initalized.");
    }

    public void Print(string content)
    {
        Console.WriteLine($"Printing '{content}' on thread {Thread.CurrentThread.ManagedThreadId}...");
        Thread.Sleep(TimeSpan.FromSeconds(1));
        Console.WriteLine($"Print completed.");
    }

    public Task PrintAsync(string content)
    {
        return Task.Run(() => Print(content));
    }
}

public class CostCalculator
{
    public decimal Calculate(string content)
    {
        Console.WriteLine($"Calculating on thread {Thread.CurrentThread.ManagedThreadId}...");
        Thread.Sleep(TimeSpan.FromSeconds(1));
        Console.WriteLine("Calculated.");

        return content.Length * 0.05m;

    }

    public Task<decimal> CalculateAsync(string content)
    {
        return Task.Run(() => Calculate(content));
    }
}