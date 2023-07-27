using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public interface IEnterPathStrategy
{
    Task<string> Enter(EnterPath enterPath);
}