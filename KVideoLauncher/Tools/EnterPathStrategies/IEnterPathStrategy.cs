﻿using System.Collections.Generic;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public interface IEnterPathStrategy
{
    string EnterAsync(EnterPath enterPath, Dictionary<string, string> lastEnteredPathByDrive);
}