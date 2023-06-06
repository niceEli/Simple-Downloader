using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Simple-Downloader [<url1> -L <directory1> [<url2> -L <directory2>] ...] [-E|--Extract]");
            return;
        }

        List<Task> downloadTasks = new List<Task>();
        bool extractFiles = false;

        string currentUrl = "";
        string currentLocation = "";

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg == "-E" || arg == "--Extract")
            {
                extractFiles = true;
                continue;
            }

            if (arg == "-L" && i + 1 < args.Length)
            {
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    downloadTasks.Add(DownloadFileAsync(currentUrl, currentLocation));

                    if (extractFiles && IsZipFile(currentUrl))
                    {
                        string zipFilePath = GetZipFilePath(currentUrl, currentLocation);
                        downloadTasks.Add(ExtractZipFileAsync(zipFilePath));
                    }
                }

                currentLocation = args[i + 1];
                i++;
                continue;
            }

            if (IsUrl(arg))
            {
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    downloadTasks.Add(DownloadFileAsync(currentUrl, currentLocation));

                    if (extractFiles && IsZipFile(currentUrl))
                    {
                        string zipFilePath = GetZipFilePath(currentUrl, currentLocation);
                        downloadTasks.Add(ExtractZipFileAsync(zipFilePath));
                    }
                }

                currentUrl = arg;
                currentLocation = "";
            }
            else
            {
                Console.WriteLine("Invalid command line arguments. URLs and locations must be specified alternately.");
                return;
            }
        }

        if (!string.IsNullOrEmpty(currentUrl))
        {
            downloadTasks.Add(DownloadFileAsync(currentUrl, currentLocation));

            if (extractFiles && IsZipFile(currentUrl))
            {
                string zipFilePath = GetZipFilePath(currentUrl, currentLocation);
                downloadTasks.Add(ExtractZipFileAsync(zipFilePath));
            }
        }

        await Task.WhenAll(downloadTasks);
    }

    static bool IsUrl(string path)
    {
        return Uri.TryCreate(path, UriKind.Absolute, out _);
    }

    static bool IsZipFile(string url)
    {
        return Path.GetExtension(url).Equals(".zip", StringComparison.OrdinalIgnoreCase);
    }

    static string GetZipFilePath(string url, string destination)
    {
        Uri uri = new Uri(url);
        string fileName = Path.GetFileName(uri.LocalPath);
        string filePath = string.IsNullOrEmpty(destination)
            ? Path.Combine(Environment.CurrentDirectory, fileName)
            : Path.Combine(destination, fileName);

        return filePath;
    }

    static async Task DownloadFileAsync(string url, string destination)
    {
        // Same as before...
    }

    static async Task ExtractZipFileAsync(string filePath)
    {
        // Same as before...
    }

    static void CopyFile(string sourcePath, string destinationDir)
    {
        // Same as before...
    }

    static int CalculateProgressPercentage(long receivedBytes, long totalBytes)
    {
        // Same as before...
    }
}
