using System.Diagnostics;
using static System.IO.Directory;

namespace AppWatcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private Process? _currentProcess;
    private readonly IConfig _config;
    public Worker(ILogger<Worker> logger, IConfig config)
    {
        _logger = logger;
        _config = config;
        
        Console.WriteLine(_config.CheckEvery());
        Console.WriteLine(_config.KillAfter());
        Console.WriteLine(_config.ExecutablePath());
        Console.WriteLine(_config.CheckFilesPath());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        StartProcess(_config.ExecutablePath());
        var currentFiles = GetFiles(_config.CheckFilesPath());
        foreach (var file in currentFiles)
        {
            File.Delete(file);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            
            var mainProcessResult = CheckProcess(_currentProcess);
            if (!mainProcessResult)
            {
                RestartProcess();
            }

            var childProcessResults = CheckChildProcesses();
            if (!childProcessResults)
            {
                RestartProcess();

            }

            if (!CheckResponseFiles())
            {
                RestartProcess();
            }
            await Task.Delay(TimeSpan.FromSeconds(_config.CheckEvery()), stoppingToken);
        }
    }

    private void RestartProcess()
    {
        _currentProcess?.Kill();
        StartProcess(_config.ExecutablePath());
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
        
        return !p.HasExited ;
    }


    private bool CheckChildProcesses()
    {
        var childProcesses = _currentProcess!.GetChildProcesses();
        var hasFoundError = childProcesses.Any(childProcess => !CheckProcess(childProcess));

        return !hasFoundError;
    }

    private bool CheckResponseFiles()
    {
        var cDate = DateTime.Now;
        var lastCheck = GetFiles(_config.CheckFilesPath()).Select(Path.GetFileNameWithoutExtension).Select(
            (s) =>
            {
                var splitName = s.Split('-');
                var date = splitName[0].Split('.');
                var time = splitName[1].Split('.');

                return new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]), int.Parse(time[0]),
                    int.Parse(time[1]), int.Parse(time[2]));
            }).OrderBy(t => t.TimeOfDay).ToList().LastOrDefault();
    
        
        var diffInSeconds = (cDate - lastCheck).TotalSeconds;
        Console.WriteLine(diffInSeconds);
        if (diffInSeconds >= 63801282420)
        {
            return true;
        }
        return diffInSeconds <= _config.KillAfter();
    }

}