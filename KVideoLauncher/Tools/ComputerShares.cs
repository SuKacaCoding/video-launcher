using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace KVideoLauncher.Tools;

public class ComputerShares : IAsyncEnumerable<string>
{
    private readonly string _computerName;

    public ComputerShares(string computerName) => _computerName = computerName;

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        var cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        cmd.Start();
        const string netCommand = "net view";
        await cmd.StandardInput.WriteLineAsync($"{netCommand} {_computerName}");
        await cmd.StandardInput.FlushAsync();
        cmd.StandardInput.Close();

        string output = await cmd.StandardOutput.ReadToEndAsync(cancellationToken);
        await cmd.WaitForExitAsync(cancellationToken);
        cmd.Close();

        const string textToContain = " Disk";
        if (!output.Contains(textToContain))
            yield break;

        List<string> lines = Regex.Split(output, pattern: @"\r\n|\r|\n").ToList();
        lines.RemoveAll(line => !line.Contains(textToContain));

        const string textToEndBefore = "Disk";
        foreach (string shareName in
                 from line in lines
                 let match = Regex.Match(line, pattern: $@"\s+{textToEndBefore}")
                 select line[..match.Index])
            yield return @$"{_computerName}\{shareName}";
    }
}