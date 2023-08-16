using System.Collections.Generic;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public interface IEnterPathStrategy
{
    string Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive);
}