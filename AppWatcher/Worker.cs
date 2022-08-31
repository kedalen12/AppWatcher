using System.Diagnostics;

namespace AppWatcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private Process? _currentProcess;
    private const string Path = @"C:\Users\kedalen\Desktop\BadApp.exe";
    private TimeSpan _timeSpanAfterNotRespondingForKill = TimeSpan.FromSeconds(2);
    public Worker(ILogger<Worker> logger)
    
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StartProcess(Path);
        while (!stoppingToken.IsCancellationRequested)
        {
            
            var mainProcessResult = CheckProcess(_currentProcess);
            if (!mainProcessResult)
            {
                _currentProcess?.Kill();
                StartProcess(Path);
            }

            var childProcessResults = CheckChildProcesses();
            if (!childProcessResults)
            {
                _currentProcess?.Kill();
                StartProcess(Path);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private void StartProcess(string processPath)
    {
        _currentProcess = Process.Start(processPath);
    }


    private bool CheckProcess(Process? p)
    {
        if (p is null)
        {
           
            return false;
        }

        if (!p.Responding)
        {
            _logger.LogInformation("Process is not responding");
        }
        return !p.HasExited ;
    }


    private bool CheckChildProcesses()
    {
        var childProcesses = _currentProcess!.GetChildProcesses();
        var hasFoundError = childProcesses.Any(childProcess => !CheckProcess(childProcess));

        return !hasFoundError;
    }

}