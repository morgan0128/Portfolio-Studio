using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

[assembly: AssemblyFixture(typeof(PlaywrightTests.ApplicationFixture))]

namespace PlaywrightTests;

public sealed class ApplicationFixture : IAsyncLifetime
{
    public const string FrontendUrl = "http://localhost:4200";
    private const string BackendUrl = "http://localhost:5225";
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(2);
    private readonly List<Process> _startedProcesses = [];
    private readonly ConcurrentQueue<string> _recentOutput = new();
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(2) };

    public async ValueTask InitializeAsync()
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        try
        {
            var root = FindRepositoryRoot();
            if (!await IsListeningAsync(5225))
            {
                StartServer("Backend", root, "dotnet", "run", "--project",
                    Path.Combine(root, "WebApplication6", "WebApplication6.csproj"),
                    "--launch-profile", "http");
            }

            if (!await IsListeningAsync(4200))
            {
                var frontend = Path.Combine(root, "WebApplication6", "Frontend");
                var angularCli = Path.Combine(frontend, "node_modules", "@angular", "cli", "bin", "ng.js");
                if (!File.Exists(angularCli))
                {
                    throw new InvalidOperationException(
                        "Angular dependencies are missing. Run 'pnpm install' in WebApplication6/Frontend first.");
                }
                
                StartServer("Angular", frontend, "node", angularCli, "serve",
                    "--proxy-config", "proxy.conf.json", "--host", "localhost", "--port", "4200");
            }
            
            await WaitForReadyAsync($"{BackendUrl}/api/albums", "application/json");
            await WaitForReadyAsync(FrontendUrl, "text/html", "<app-root");
            await WaitForReadyAsync($"{FrontendUrl}/api/albums", "application/json");
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    private void StartServer(string name, string workingDirectory, string executable, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        
        startInfo.Environment["CI"] = "true";
        var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (_, args) => RecordOutput(name, args.Data);
        process.ErrorDataReceived += (_, args) => RecordOutput(name, args.Data);
        try
        {
            process.Start();
        }
        catch
        {
            process.Dispose();
            throw;
        }

        _startedProcesses.Add(process);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    private void RecordOutput(string name, string? line)
    {
        if (line is null) return;
        _recentOutput.Enqueue($"[{name}] {line}");
        while (_recentOutput.Count > 60) _recentOutput.TryDequeue(out _);
    }

    private async Task WaitForReadyAsync(string url, string mediaType, string? expectedText = null)
    {
        var timer = Stopwatch.StartNew();
        var cancellation = TestContext.Current.CancellationToken;
        var lastResponse = "No response yet.";
        while (timer.Elapsed < StartupTimeout)
        {
            cancellation.ThrowIfCancellationRequested();
            var exited = _startedProcesses.FirstOrDefault(process => process.HasExited);
            if (exited is not null)
            {
                exited.WaitForExit();
                throw new InvalidOperationException(
                    $"{exited.StartInfo.FileName} exited with code {exited.ExitCode} while starting the app.\n{ServerOutput()}");
            }

            try
            {
                using var response = await _http.GetAsync(url, cancellation);
                if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == mediaType)
                {
                    if (expectedText is null || (await response.Content.ReadAsStringAsync(cancellation)).Contains(expectedText))
                    {
                        return;
                    }
                }

                lastResponse = $"HTTP {(int)response.StatusCode}; content type {response.Content.Headers.ContentType}.";
            }
            catch (HttpRequestException exception)
            {
                lastResponse = exception.Message;
            }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
            {
                lastResponse = "HTTP request timed out.";
            }

            await Task.Delay(250, cancellation);
        }

        throw new TimeoutException(
            $"The app was not ready at {url} after {StartupTimeout.TotalSeconds} seconds. {lastResponse}\n" +
            $"Check the Development app settings, PostgreSQL, and ports 4200/5225.\n{ServerOutput()}");
    }

    private string ServerOutput() => string.Join(Environment.NewLine, _recentOutput);

    private static async Task<bool> IsListeningAsync(int port)
    {
        using var client = new TcpClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        try
        {
            await client.ConnectAsync("localhost", port, timeout.Token);
            return true;
        }
        catch (SocketException) { return false; }
        catch (OperationCanceledException) { return false; }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WebApplication6.sln"))) return directory.FullName;
        }

        throw new DirectoryNotFoundException("Run these local E2E tests from a checkout of WebApplication6.");
    }

    private void OnProcessExit(object? sender, EventArgs args) => StopServers();

    private void StopServers()
    {
        foreach (var process in _startedProcesses.AsEnumerable().Reverse())
        {
            try
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
            catch (InvalidOperationException) { /* The process has already exited. */ }
        }
    }

    public ValueTask DisposeAsync()
    {
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        StopServers();
        foreach (var process in _startedProcesses) process.Dispose();
        _startedProcesses.Clear();
        _http.Dispose();
        return ValueTask.CompletedTask;
    }
}
