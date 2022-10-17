using System.Text.Json;

namespace AppWatcher;




public interface IConfig
{
    string ExecutablePath();
    string CheckFilesPath();
    int KillAfter();
    int CheckEvery();
}

public sealed class Config : IConfig
{

    private InternalConfig internalConfig;
    private class InternalConfig
    {
        public  string executablePath { get; set; }
        public  string checkFilesPath { get; set; }
        public  int killAfter { get; set; }
        public  int checkEvery { get; set; }
    }

    public Config()
    {

        internalConfig =  JsonSerializer.Deserialize<InternalConfig>(File.ReadAllText("config.json"));

     
    }

    public string ExecutablePath() => internalConfig.executablePath;

    public string CheckFilesPath() => internalConfig.checkFilesPath;

    public int KillAfter() => internalConfig.killAfter;

    public int CheckEvery() => internalConfig.checkEvery;
}