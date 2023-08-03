using Microsoft.VisualBasic.ApplicationServices;

namespace KVideoLauncher;

public class SingleInstanceApplicationWrapper : WindowsFormsApplicationBase
{
    public SingleInstanceApplicationWrapper() => IsSingleInstance = true;

    protected override bool OnStartup(StartupEventArgs e)
    {
        _app = new App();
        _app.Run();

        return false;
    }

    protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
    {
        base.OnStartupNextInstance(e);
        _app?.Activate();
    }

    private App? _app;
}