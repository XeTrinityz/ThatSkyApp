using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ThatSkyAppV2.Services;

public class DownloadService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, DownloadProgress> _activeDownloads;
    private const int BufferSize = 81920; // 80KB buffer
    private const int MaxRetries = 3;
    private const int MaxParallelDownloads = 2;

    public record DownloadProgress(
        long TotalBytes,
        long DownloadedBytes,
        double ProgressPercentage,
        double SpeedBytesPerSecond,
        TimeSpan EstimatedTimeRemaining
    );

    public DownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _activeDownloads = new ConcurrentDictionary<string, DownloadProgress>();
    }

    public async Task<bool> DownloadFileAsync(
        string url,
        string destination,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        int retryCount = 0;
        while (retryCount < MaxRetries)
        {
            try
            {
                return await DownloadFileInternalAsync(url, destination, progress, cancellationToken);
            }
            catch (HttpRequestException) when (retryCount < MaxRetries - 1)
            {
                retryCount++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);
            }
        }

        return false;
    }

    private async Task<bool> DownloadFileInternalAsync(
        string url,
        string destination,
        IProgress<DownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        // Check status code instead of throwing
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        using var fileStream = new FileStream(
            destination,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            BufferSize,
            useAsync: true);

        using var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[BufferSize];
        var downloadedBytes = 0L;
        var startTime = DateTime.UtcNow;
        var lastUpdate = startTime;
        int bytesRead;

        while ((bytesRead = await downloadStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            downloadedBytes += bytesRead;

            var now = DateTime.UtcNow;
            if ((now - lastUpdate).TotalMilliseconds >= 100) // Update progress max 10 times per second
            {
                var elapsedSeconds = (now - startTime).TotalSeconds;
                var speedBytesPerSecond = downloadedBytes / elapsedSeconds;

                var remainingBytes = totalBytes - downloadedBytes;
                var estimatedTimeRemaining = speedBytesPerSecond > 0
                    ? TimeSpan.FromSeconds(remainingBytes / speedBytesPerSecond)
                    : TimeSpan.MaxValue;

                var currentProgress = new DownloadProgress(
                    totalBytes,
                    downloadedBytes,
                    totalBytes > 0 ? (double)downloadedBytes / totalBytes * 100 : 0,
                    speedBytesPerSecond,
                    estimatedTimeRemaining);

                progress?.Report(currentProgress);
                _activeDownloads[url] = currentProgress;
                lastUpdate = now;
            }
        }

        // Ensure the file is properly written
        await fileStream.FlushAsync(cancellationToken);
        return true;
    }

    public void Dispose()
    {
        _activeDownloads.Clear();
    }
}