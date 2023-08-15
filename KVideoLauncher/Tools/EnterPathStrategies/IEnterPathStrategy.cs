using System.Collections.Generic;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public interface IEnterPathStrategy
{
    string Enter(EnterPath enterPath, Dictionary<string, string> lastEnteredPathByDrive);
}