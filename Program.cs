using System.Diagnostics;
using CsvHelper;
using CommandLine;

await CommandLine.Parser.Default.ParseArguments<ProgramOptions>(args)
    .WithParsedAsync( async (ProgramOptions opts) => {
        IEnumerable<VideoInfo> videoInfos = ReadCsv(opts.CsvPath);
        if (opts.Limit > 0) {
            System.Console.WriteLine($"limit option set, only downloading the first {opts.Limit} videos");
            videoInfos = videoInfos.Take(opts.Limit);
        }
        System.Console.WriteLine($"Downloading videos with concurrency {opts.Concurrency}");
        await YoutubeDownload(videoInfos, opts.Concurrency, opts.OutputDirectory, opts.CookieFilePath);
    });

List<VideoInfo> ReadCsv(string csvPath) {
    using var fileReader = new StreamReader(csvPath);
    using var csvReader = new CsvReader(fileReader, System.Globalization.CultureInfo.InvariantCulture);
    return csvReader.GetRecords<VideoInfo>().ToList();
}

async Task<VideoDownloadResult> DownloadVideoAsync(VideoInfo video, string outputDir, string cookePath) {
    const string CMD_YOUTUBE_DL = "youtube-dl";
    const string FORMAT = "best";

    var escapedTitle = Path.GetInvalidFileNameChars().Aggregate(
        video.Title,
        (accum, invalidChar) => {
            return accum.Replace(invalidChar, '#');
        }
    );

    var outputPathBase = $"{outputDir}/{escapedTitle}";

    var taskCompletionSource = new TaskCompletionSource<VideoDownloadResult>();

    var process = new Process {
        StartInfo = {
            FileName = CMD_YOUTUBE_DL,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        },
        EnableRaisingEvents = true,
    };
    process.StartInfo.ArgumentList.Add("--cookies");
    process.StartInfo.ArgumentList.Add(cookePath);
    process.StartInfo.ArgumentList.Add("-f");
    process.StartInfo.ArgumentList.Add(FORMAT);
    process.StartInfo.ArgumentList.Add("-o");
    process.StartInfo.ArgumentList.Add($"{outputPathBase}.mp4");
    process.StartInfo.ArgumentList.Add(video.Url);

    process.Exited += (sender, args) => {
        taskCompletionSource.SetResult(new VideoDownloadResult {
            VideoInfo = video,
            StatusCode = process.ExitCode,
        });
    };

    process.Start();

    process.BeginErrorReadLine();
    process.BeginOutputReadLine();
    process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
        if (!String.IsNullOrEmpty(e.Data)) File.AppendAllText($"{outputPathBase}.stdout.txt", e.Data + "\n");
    };
    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
        if (!String.IsNullOrEmpty(e.Data)) File.AppendAllText($"{outputPathBase}.stderr.txt", e.Data + "\n");
    };

    return await taskCompletionSource.Task;
}

Task YoutubeDownload(IEnumerable<VideoInfo> videos, int concurrency, string outputDir, string cookiePath) {
    var videoEnumerator = videos.GetEnumerator();
    var tasks = new List<Task>();

    while (true) {
        if (tasks.Count >= concurrency) continue;
        if (!videoEnumerator.MoveNext()) break;

        var v = videoEnumerator.Current;
        var t = DownloadVideoAsync(v, outputDir, cookiePath);
        tasks.Add(t);
        System.Console.WriteLine($"Starting download of {v.Title}");
        t.ContinueWith((result) => {
            System.Console.WriteLine($"Download of {v.Title} Complete");
            tasks.Remove(t);
        });
    }
    System.Console.WriteLine("No more downloads to start");
    return Task.WhenAll(tasks);
}

public class VideoInfo {
    public string Title {get; set;}
    public string Url {get; set;}
}

public class VideoDownloadResult {
    public VideoInfo VideoInfo;
    public int StatusCode;
}

public class ProgramOptions {
    [Option('c', "concurrency", Default = 5, Required = false, HelpText = "Max number of simultaneous downloads")]
    public int Concurrency {get; set;}
    [Option('l', "limit", Default = 0, Required = false, HelpText = "Only process the first N entries, 0 means no limit")]
    public int Limit {get; set;}
    [Value(0, MetaName = "csv path", HelpText = "The CSV file which specifies videos to download")]
    public string CsvPath {get; set;}
    [Value(1, MetaName = "cookie path", HelpText = "The path to the netscape format cookie file")]
    public string CookieFilePath {get; set;}
    [Value(2, MetaName = "output directory", HelpText = "The directory to store output files")]
    public string OutputDirectory {get; set;}
}