using System.Collections.Generic;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.Strategies.EnterPath;

public interface IEnterPathStrategy
{
    Task<string> Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive);
}