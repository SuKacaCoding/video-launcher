using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KVideoLauncher.ExtensionMethods;

namespace KVideoLauncher.Models;

public static class DataFileHelper
{
    public static IEnumerable<DriveInfo> DriveInfos { get; }

    private const string DriveDataFileRelativePath = "Drives.txt";

    private static readonly string DataFolderPath = Path.Combine
        (path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path2: "KVideoLauncher");

    static DataFileHelper() =>
        DriveInfos = from driveStr in ReadDataFile(DriveDataFileRelativePath).ToUpper().Split(Environment.NewLine)
            where driveStr.GetDriveInfo() is { }
            select driveStr.GetDriveInfo();

    private static string ReadDataFile(string fileRelativePath)
    {
        string filePath = Path.Combine(DataFolderPath, fileRelativePath);
        return Path.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
    }
}