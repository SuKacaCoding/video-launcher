using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterPath
{
    public IEnterPathStrategy? Strategy { set; private get; }
    public string? Path { get; set; }
    public static EnterPath Instance { get; } = new();

    public Task<string> Enter()
    {
        Debug.Assert(condition: Path is { }, message: "Path is { }");
        Debug.Assert(condition: Strategy is { }, message: "Strategy is { }");
        return Strategy.Enter(this);
    }
}