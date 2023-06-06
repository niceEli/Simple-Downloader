using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Simple-Downloader <url> [<directory>] [-E or --Extract]");
            return;
        }

        string source = args[0];
        string destination = args.Length > 1 ? args[1] : "";
        bool shouldExtract = args.Length > 2 && (args[2] == "-E" || args[2] == "--Extract");

        try
        {
            if (IsUrl(source))
            {
                await DownloadFileAsync(source, destination);
            }
            else
            {
                if (!File.Exists(source))
                {
                    Console.WriteLine("The source file does not exist.");
                    return;
                }

                if (shouldExtract && IsZipFile(source))
                {
                    ExtractZipFile(source, destination);
                }
                else
                {
                    CopyFile(source, destination);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static bool IsUrl(string path)
    {
        return Uri.TryCreate(path, UriKind.Absolute, out _);
    }

    static bool IsZipFile(string path)
    {
        string extension = Path.GetExtension(path);
        return string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase);
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
                        Console.Write($"Downloading: {CalculateProgressPercentage(downloadedBytes, totalBytes)}%" + " (" + downloadedBytes + " / " + totalBytes + "Bytes");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"File downloaded and saved to: {filePath}");

            if (shouldExtract && IsZipFile(filePath))
            {
                ExtractZipFile(filePath, destination);
                File.Delete(filePath);
                Console.WriteLine($"Zip file extracted and deleted: {filePath}");
            }
        }
    }

    static void CopyFile(string sourcePath, string destinationDir)
    {
        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = string.IsNullOrEmpty(destinationDir)
            ? Path.Combine(Environment.CurrentDirectory, fileName)
            : Path.Combine(destinationDir, fileName);

        File.Copy(sourcePath, destinationPath, true);

        Console.WriteLine($"File copied to: {destinationPath}");

        if (shouldExtract && IsZipFile(destinationPath))
        {
            ExtractZipFile(destinationPath, destinationDir);
            File.Delete(destinationPath);
            Console.WriteLine($"Zip file extracted and deleted: {destinationPath}");
        }
    }

    static void ExtractZipFile(string zipFilePath, string destinationDir)
    {
        ZipFile.ExtractToDirectory(zipFilePath, destinationDir);
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
