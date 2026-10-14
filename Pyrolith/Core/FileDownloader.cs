namespace Pyrolith.Core;

public readonly record struct DownloadProgress(long BytesDownloaded, long? TotalBytes, double Percentage);

/// <summary>Streams a file to disk and reports progress (percentage is 0 when the size is unknown).</summary>
public static class FileDownloader
{
    private const int BufferSize = 81920;
    private const int ReportEveryNChunks = 10; // ~800 KB, keeps UI updates cheap

    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Pyrolith/1.0");
        return client;
    }

    public static async Task DownloadFileAsync(
        string url,
        string destinationFilePath,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(
            destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, useAsync: true);

        var buffer = new byte[BufferSize];
        long totalRead = 0;
        int read, chunk = 0;

        while ((read = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            if (progress != null && ++chunk % ReportEveryNChunks == 0)
                progress.Report(new DownloadProgress(totalRead, totalBytes, Percent(totalRead, totalBytes)));
        }

        if (totalBytes.HasValue && totalRead != totalBytes.Value)
            throw new IOException($"Download incomplete: received {totalRead} of {totalBytes} bytes.");

        progress?.Report(new DownloadProgress(totalRead, totalBytes, totalBytes.HasValue ? 100 : 0));
    }

    private static double Percent(long done, long? total) =>
        total is > 0 ? Math.Round((double)done / total.Value * 100, 2) : 0;
}
