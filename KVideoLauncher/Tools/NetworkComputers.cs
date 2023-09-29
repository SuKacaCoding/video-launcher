using GongSolutions.Shell;
using GongSolutions.Shell.Interop;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools;

public class NetworkComputers : IAsyncEnumerable<string>
{
    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        var folder = new ShellItem((Environment.SpecialFolder)CSIDL.NETWORK);
        using IEnumerator<ShellItem> e = folder.GetEnumerator(SHCONTF.FOLDERS);

        while (await Task.Run(e.MoveNext, cancellationToken))
            yield return e.Current.ParsingName;
    }
}