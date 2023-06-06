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
                await DownloadFileAsync(source, destination, shouldExtract);
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
                    CopyFile(source, destination, shouldExtract);
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

    static async Task DownloadFileAsync(string url, string destination, bool shouldExtract)
    {
        // Rest of the code

        Console.WriteLine($"File downloaded and saved to: {filePath}");

        if (shouldExtract && IsZipFile(filePath))
        {
            ExtractZipFile(filePath, destination);
            File.Delete(filePath);
            Console.WriteLine($"Zip file extracted and deleted: {filePath}");
        }
    }

    static void CopyFile(string sourcePath, string destinationDir, bool shouldExtract)
    {
        // Rest of the code

        if (shouldExtract && IsZipFile(destinationPath))
        {
            ExtractZipFile(destinationPath, destinationDir);
            File.Delete(destinationPath);
            Console.WriteLine($"Zip file extracted and deleted: {destinationPath}");
        }
    }

    // Rest of the code
}
