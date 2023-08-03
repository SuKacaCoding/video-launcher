namespace KVideoLauncher;

public static class Startup
{
    [STAThread]
    private static void Main(string[] args)
    {
        var wrapper = new SingleInstanceApplicationWrapper();
        wrapper.Run(args);
    }
}