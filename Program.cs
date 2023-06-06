using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Simple-Downloader <url> [<directory>]");
            return;
        }

        string source = args[0];
        string destination = args.Length > 1 ? args[1] : "";

        try
        {
            if (IsUrl(source))
            {
                if (IsTorrent(source))
                {
                    await DownloadTorrentAsync(source, destination);
                }
                else
                {
                    await DownloadFileAsync(source, destination);
                }
            }
            else
            {
                if (!File.Exists(source))
                {
                    Console.WriteLine("The source file does not exist.");
                    return;
                }

                CopyFile(source, destination);
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

    static bool IsTorrent(string path)
    {
        string extension = Path.GetExtension(path);
        return extension.Equals(".torrent", StringComparison.OrdinalIgnoreCase);
    }

    static async Task DownloadTorrentAsync(string url, string destination)
    {
        string fileName = Path.GetFileName(url);
        string filePath = string.IsNullOrEmpty(destination)
            ? Path.Combine(Environment.CurrentDirectory, fileName)
            : Path.Combine(destination, fileName);

        using (HttpClient client = new HttpClient())
        {
            using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        Console.WriteLine($"Torrent file downloaded and saved to: {filePath}");
        await ProcessTorrentFile(filePath, destination);
    }

    static async Task ProcessTorrentFile(string filePath, string destination)
    {
        Torrent torrent = Torrent.Load(filePath);
        string torrentPath = Path.Combine(destination, torrent.Name);

        using (var engine = new ClientEngine())
        using (var torrentManager = new TorrentManager(torrent, torrentPath, new TorrentSettings()))
        {
            engine.Register(torrentManager);
            torrentManager.Start();

            while (torrentManager.State != TorrentState.Seeding)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Downloading: {CalculateProgressPercentage(torrentManager)}%");

                await Task.Delay(1000);
            }

            Console.WriteLine();
            Console.WriteLine($"Torrent download completed. Files saved to: {torrentPath}");
        }
    }

    static int CalculateProgressPercentage(TorrentManager torrentManager)
    {
        double progress = (double)torrentManager.Progress * 100;
        return (int)progress;
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
                        Console.Write($"Downloading: {CalculateProgressPercentage(downloadedBytes, totalBytes)}%");

                        DrawProgressBar(downloadedBytes, totalBytes);
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"File downloaded and saved to: {filePath}");
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

    static void DrawProgressBar(long completed, long total)
    {
        const int ProgressBarWidth = 10;
        const string ProgressBarChars = "░▒▓█";

        double progress = (double)completed * 2 / total;
        int completedWidth = (int)(progress * ProgressBarWidth);

        int numFullChars = completedWidth / (ProgressBarWidth / ProgressBarChars.Length);
        int numPartialChars = completedWidth % (ProgressBarWidth / ProgressBarChars.Length);

        string progressBar = new string(ProgressBarChars[ProgressBarChars.Length - 1], numFullChars);

        if (numPartialChars > 0)
            progressBar += ProgressBarChars[numPartialChars - 1];

        progressBar = progressBar.PadRight(ProgressBarWidth, ProgressBarChars[0]);

        Console.Write($" {progressBar}");
    }
}
