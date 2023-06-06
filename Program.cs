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
        if (args.Length < 2 || args.Length % 2 != 0)
        {
            Console.WriteLine("Usage: Simple-Downloader <url1> -L <directory1> [<url2> -L <directory2>] [-E|--Extract]");
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

            if (arg == "-L")
            {
                if (i + 1 < args.Length)
                {
                    currentLocation = args[i + 1];
                    i++;
                }
                else
                {
                    Console.WriteLine("Invalid command line arguments. The location is missing.");
                    return;
                }
            }
            else
            {
                currentUrl = arg;

                try
                {
                    if (IsUrl(currentUrl))
                    {
                        downloadTasks.Add(DownloadFileAsync(currentUrl, currentLocation));

                        if (extractFiles && IsZipFile(currentUrl))
                        {
                            string zipFilePath = GetZipFilePath(currentUrl, currentLocation);
                            downloadTasks.Add(ExtractZipFileAsync(zipFilePath));
                        }
                    }
                    else
                    {
                        if (!File.Exists(currentUrl))
                        {
                            Console.WriteLine("The source file does not exist.");
                            return;
                        }

                        CopyFile(currentUrl, currentLocation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
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
        using (HttpClient client = new HttpClient())
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            string filePath = string.IsNullOrEmpty(destination)
                ? Path.Combine(Environment.CurrentDirectory, fileName)
                : Path.Combine(destination, fileName);

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
        string zipFileName = Path.GetFileName(filePath);

        Console.WriteLine($"Extracting: {zipFileName}");

        await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(filePath, extractPath));

        File.Delete(filePath);

        Console.WriteLine($"Zip file extracted and deleted: {filePath}");
    }

    static void CopyFile(string sourcePath, string destinationDir)
    {
        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = string.IsNullOrEmpty(destinationDir)
            ? Path.Combine(Environment.CurrentDirectory, fileName)
            : Path.Combine(destinationDir, fileName);

        File.Copy(sourcePath, destinationPath, true);

        Console.WriteLine($"File copied to: {destinationPath}");
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
