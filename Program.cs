using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
	static async Task Main (string[] args)
	{
		Console.ForegroundColor = ConsoleColor.DarkCyan;

		Console.WriteLine("█████████████████████");
		Console.WriteLine("█ Simple Downloader █");
		Console.WriteLine("█    By niceEli     █");
		Console.WriteLine("█                   █");
		Console.WriteLine("█      V1.0.X       █");
		Console.WriteLine("█████████████████████");
		Console.WriteLine();

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
				await DownloadFileAsync(source, destination);
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

	static bool IsUrl (string path)
	{
		return Uri.TryCreate(path, UriKind.Absolute, out _);
	}

	static async Task DownloadFileAsync (string url, string destination)
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
					const int bufferSize = 819200;
					var buffer = new byte[bufferSize];
					long totalBytes = response.Content.Headers.ContentLength ?? 0;
					long downloadedBytes = 0;
					int bytesRead;

					while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
					{
						await fileStream.WriteAsync(buffer, 0, bytesRead);
						downloadedBytes += bytesRead;

						Console.SetCursorPosition(0, Console.CursorTop);
						Console.Write($"Downloading: {CalculateProgressPercentage(downloadedBytes, totalBytes)}%" + " " + downloadedBytes / 128000 + "/" + totalBytes / 128000 + " Blocks");
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write(" " + GenerateProgressBar(downloadedBytes, totalBytes));
					}
				}
			}

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write("File downloaded and saved to: ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(filePath);
		}
	}

	static void CopyFile (string sourcePath, string destinationDir)
	{
		string fileName = Path.GetFileName(sourcePath);
		string destinationPath = string.IsNullOrEmpty(destinationDir)
			? Path.Combine(Environment.CurrentDirectory, fileName)
			: Path.Combine(destinationDir, fileName);

		File.Copy(sourcePath, destinationPath, true);

		Console.WriteLine($"File copied to: {destinationPath}");
	}

	static int CalculateProgressPercentage (long receivedBytes, long totalBytes)
	{
		if (totalBytes > 0)
		{
			double progress = (double)receivedBytes / totalBytes;
			return (int)(progress * 100);
		}

		return 0;
	}

	static string GenerateProgressBar (float current, float max)
	{
		const int ProgressBarWidth = 10; // Adjust this value to change the width of the progress bar
		float progress = current / max;
		if (progress < 0)
			progress = 0;
		else if (progress > 1)
			progress = 1;

		int filledWidth = (int)(progress * ProgressBarWidth);
		int emptyWidth = ProgressBarWidth - filledWidth;

		string progressBar = new string('█', filledWidth) + new string('░', emptyWidth);
		return progressBar;
	}
}
