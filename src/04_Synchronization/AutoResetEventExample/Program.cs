﻿
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, AutoResetEvent!");

var motionEvent = new AutoResetEvent(false);

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton(motionEvent);
                services.AddSingleton<MotionDetectionService>();
                services.AddSingleton<AlarmService>();
                services.AddSingleton<LightService>();
                services.AddHostedService<Worker>();
            })
            .Build();

await host.RunAsync();


public class MotionDetectionService
{
    private readonly AutoResetEvent _motionEvent;

    public MotionDetectionService(AutoResetEvent motionEvent)
    {
        _motionEvent = motionEvent;
    }

    public void DetectMotion()
    {
        Console.WriteLine($"🎥 Motion detected at {DateTime.Now:HH:mm:ss}");
        _motionEvent.Set(); // Wysyła sygnał
    }
}

public class AlarmService
{
    private readonly AutoResetEvent motionEvent;

    public AlarmService(AutoResetEvent motionEvent)
    {
        this.motionEvent = motionEvent;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.Factory.StartNew(() => Start(cancellationToken), TaskCreationOptions.LongRunning);

    private void Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            motionEvent.WaitOne(); // Czeka na sygnał (np. od detektora ruchu)

            Console.WriteLine($"🚨 Alarm activated at {DateTime.Now:HH:mm:ss}");
        }
    }
}


public class LightService
{
    private readonly AutoResetEvent _motionEvent;

    public LightService(AutoResetEvent motionEvent)
    {
        _motionEvent = motionEvent;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.Factory.StartNew(() => Start(cancellationToken), TaskCreationOptions.LongRunning);

    public void Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _motionEvent.WaitOne(); // Czeka na sygnał (np. od detektora ruchu)

            Console.WriteLine($"💡 Light turned on at {DateTime.Now:HH:mm:ss}");
        }
    }
}




public class Worker : BackgroundService
{
    private readonly MotionDetectionService _motionService;
    private readonly AlarmService _alarmService;
    private readonly LightService _lightService;

    public Worker(MotionDetectionService motionService, AlarmService alarmService, LightService lightService)
    {
        _motionService = motionService;
        _alarmService = alarmService;
        _lightService = lightService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ⏱️ Uruchamiamy nasłuchiwanie alarmu
        _ = _alarmService.StartAsync(stoppingToken);
        _ = _lightService.StartAsync(stoppingToken);

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _motionService.DetectMotion();
        }
    }
}



//❗ Problemy:
// 1. Polling – AlarmService co 100ms pyta, czy coś się wydarzyło → marnuje CPU, nieefektywne
// 2. Można zgubić ruch, jeśli Reset() zostanie wywołany zanim AlarmService zdąży odczytać flagę.
// 3. Kod jest nieskalowalny – każde nowe urządzenie dodaje kolejne wątki sprawdzające.

