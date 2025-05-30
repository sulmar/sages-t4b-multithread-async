﻿
Console.WriteLine("Hello, Monitor!");

var gate = new ParkingGate(maxVehicles: 3);
var vehicleCount = 10;

var tasks = Enumerable.Range(1, vehicleCount)
    .Select(id => Task.Run(async () =>
    {
        await Task.Delay(Random.Shared.Next(100, 500)); // losowe opóźnienie przyjazdu - 👉 sprawdż co się stanie przy mniejszych czasach!
        await gate.TryEnterAsync(id);
    }))
    .ToArray();


await Task.WhenAll(tasks);

public class ParkingGate
{
    private int _remainingSpots;

    private readonly object _lock = new object();

    public ParkingGate(int maxVehicles)
    {
        _remainingSpots = maxVehicles;
    }


    public async Task TryEnterAsync(int vehicleId)
    {
        await Task.Yield();


        bool lockTaken = false;

        try
        {

            Monitor.TryEnter(_lock, ref lockTaken);

            Thread.Sleep(500);

            // await Task.Delay(500); // Problem!

            Console.WriteLine(lockTaken);

            if (lockTaken && _remainingSpots > 0)
            {
                _remainingSpots--;
                Console.WriteLine($"🚗 Pojazd #{vehicleId} wpuszczony.");

                //if (DateTime.Now.Second % 2 == 0)
                //    throw new ApplicationException();

            }
            else
            {
                Console.WriteLine($"❌ Pojazd #{vehicleId} odrzucony — brak miejsc.");
            }
        }
        finally
        {
            if (lockTaken)
            {
              //  Console.WriteLine(lockTaken);
                Monitor.Exit(_lock);
            }
            
        }


    }
}