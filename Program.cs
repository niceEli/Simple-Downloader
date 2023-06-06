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

        List<(string url, string location)> downloads = new List<(string url, string location)>();
        bool extractFiles = false;

        string currentLocation = null;

        foreach (var arg in args)
        {
            if (arg == "-E" || arg == "--Extract")
            {
                extractFiles = true;
                continue;
            }

            if (arg == "-L" || arg == "--Location")
            {
                currentLocation = null;
                continue;
            }

            if (currentLocation == null)
            {
                currentLocation = arg;
            }
            else
            {
                downloads.Add((arg, currentLocation));
                currentLocation = null;
            }
        }

        List<Task> tasks = new List<Task>();

        foreach (var (url, location) in downloads)
        {
            tasks.Add(DownloadFileAsync(url, location));

            if (extractFiles && IsZipFile(url))
            {
                string zipFilePath = Path.Combine(location, Path.GetFileName(url));
                tasks.Add(ExtractZipFileAsync(zipFilePath));
            }
        }

        await Task.WhenAll(tasks);
    }

    static bool IsUrl(string path)
    {
        return Uri.TryCreate(path, UriKind.Absolute, out _);
    }

    static bool IsZipFile(string url)
    {
        return Path.GetExtension(url).Equals(".zip", StringComparison.OrdinalIgnoreCase);
    }

    static async Task DownloadFileAsync(string url, string destination)
    {
        using (HttpClient client = new HttpClient())
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            string filePath = Path.Combine(destination, fileName);

            using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    const int bufferSize = 8192;
                    var buffer = new byte[bufferSize];
                    long totalBytes = response.Content.Headers.ContentLength ?? 0;
                    long downloadedBytes = 0;
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"Downloading: {CalculateProgressPercentage(downloadedBytes, totalBytes)}% ({downloadedBytes}/{totalBytes} bytes)");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"File downloaded and saved to: {filePath}");
        }
    }

    static async Task ExtractZipFileAsync(string filePath)
    {
        string extractPath = Path.GetDirectoryName(filePath);

        await Task.Run(() =>
        {
            ZipFile.ExtractToDirectory(filePath, extractPath);
        });

        File.Delete(filePath);
        Console.WriteLine($"Zip file extracted to: {extractPath}");
    }

    static int CalculateProgressPercentage(long receivedBytes, long totalBytes)
    {
        if (totalBytes > 0)
        {
            double progress = (double)receivedBytes / totalBytes;
            return (int)(progress * 100);
        }

        return 0;
    }
}
