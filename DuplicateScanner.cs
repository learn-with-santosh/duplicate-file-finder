using System.Security.Cryptography;

namespace DuplicateFileFinder;

public class DuplicateScanner
{
    public event Action<string>? StatusUpdate;
    public event Action<int>? ProgressUpdate;
    public event Action<int>? TotalFilesFound;

    private readonly CancellationTokenSource _cts = new();

    public void Cancel() => _cts.Cancel();

    public async Task<Dictionary<string, List<FileItem>>> ScanForDuplicatesAsync(
        string scanPath, HashSet<string> extensions)
    {
        var result = new Dictionary<string, List<FileItem>>();

        StatusUpdate?.Invoke("Collecting files...");

        // Gather all files
        var allFiles = new List<string>();
        CollectFiles(scanPath, extensions, allFiles);

        TotalFilesFound?.Invoke(allFiles.Count);
        StatusUpdate?.Invoke($"Found {allFiles.Count} files. Grouping by size...");

        if (_cts.Token.IsCancellationRequested) return result;

        // Group by file size first (quick filter)
        var sizeGroups = new Dictionary<long, List<string>>();
        foreach (var file in allFiles)
        {
            try
            {
                var info = new FileInfo(file);
                if (!info.Exists || info.Length == 0) continue;

                if (!sizeGroups.ContainsKey(info.Length))
                    sizeGroups[info.Length] = new List<string>();
                sizeGroups[info.Length].Add(file);
            }
            catch { /* skip inaccessible files */ }
        }

        // Only keep groups with more than one file (potential duplicates)
        var potentialDuplicates = sizeGroups.Where(g => g.Value.Count > 1).ToList();

        StatusUpdate?.Invoke($"Hashing {potentialDuplicates.Sum(g => g.Value.Count)} potential duplicates...");

        int processed = 0;
        int totalToHash = potentialDuplicates.Sum(g => g.Value.Count);

        // Hash files that share the same size
        foreach (var group in potentialDuplicates)
        {
            if (_cts.Token.IsCancellationRequested) break;

            foreach (var filePath in group.Value)
            {
                if (_cts.Token.IsCancellationRequested) break;

                try
                {
                    var hash = await ComputeHashAsync(filePath, _cts.Token);
                    var fileItem = new FileItem
                    {
                        FilePath = filePath,
                        FileSize = group.Key,
                        Hash = hash
                    };

                    if (!result.ContainsKey(hash))
                        result[hash] = new List<FileItem>();
                    result[hash].Add(fileItem);

                    processed++;
                    int pct = totalToHash > 0 ? (processed * 100 / totalToHash) : 0;
                    ProgressUpdate?.Invoke(pct);
                    StatusUpdate?.Invoke($"Hashing file {processed}/{totalToHash}: {Path.GetFileName(filePath)}");
                }
                catch { processed++; }
            }
        }

        // Remove non-duplicates (groups with only 1 file)
        var keys = result.Keys.Where(k => result[k].Count < 2).ToList();
        foreach (var key in keys)
            result.Remove(key);

        return result;
    }

    private void CollectFiles(string path, HashSet<string> extensions, List<string> files)
    {
        try
        {
            foreach (var file in Directory.GetFiles(path))
            {
                if (_cts.Token.IsCancellationRequested) return;

                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (extensions.Count == 0 || extensions.Contains(ext))
                    files.Add(file);
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                if (_cts.Token.IsCancellationRequested) return;
                CollectFiles(dir, extensions, files);
            }
        }
        catch { /* skip inaccessible directories */ }
    }

    private static async Task<string> ComputeHashAsync(string filePath, CancellationToken ct)
    {
        using var md5 = MD5.Create();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan);

        // For large files, only hash first 10MB + last 10MB for speed
        const long threshold = 50 * 1024 * 1024; // 50MB
        const int chunkSize = 10 * 1024 * 1024;  // 10MB

        if (stream.Length > threshold)
        {
            using var ms = new MemoryStream();
            var buffer = new byte[chunkSize];

            // Read first chunk
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, chunkSize), ct);
            ms.Write(buffer, 0, bytesRead);

            // Read last chunk
            stream.Seek(-chunkSize, SeekOrigin.End);
            bytesRead = await stream.ReadAsync(buffer.AsMemory(0, chunkSize), ct);
            ms.Write(buffer, 0, bytesRead);

            // Include file size in hash to reduce collisions
            var sizeBytes = BitConverter.GetBytes(stream.Length);
            ms.Write(sizeBytes, 0, sizeBytes.Length);

            ms.Position = 0;
            var hash = await md5.ComputeHashAsync(ms, ct);
            return Convert.ToHexString(hash);
        }
        else
        {
            var hash = await md5.ComputeHashAsync(stream, ct);
            return Convert.ToHexString(hash);
        }
    }
}
