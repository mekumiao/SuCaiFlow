using System.Collections.Concurrent;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowEngineConcurrencyExecutor : IDisposable {
    private readonly Channel<QueuedWork> _channel;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _pending = new();
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _workers = new();
    private readonly SuCaiFlowEngineOptions _options;
    private readonly ILogger<SuCaiFlowEngineConcurrencyExecutor> _logger;
    private int _workerIdSeed;
    private int _targetConcurrency;
    private CancellationToken _hostToken = CancellationToken.None;
    private volatile bool _started;

    public SuCaiFlowEngineConcurrencyExecutor(
        IOptions<SuCaiFlowEngineOptions> options,
        ILogger<SuCaiFlowEngineConcurrencyExecutor> logger) {
        _options = options.Value;
        _logger = logger;
        _targetConcurrency = _options.MaxConcurrentTasks;

        ArgumentNullException.ThrowIfNull(_options);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_options.MaxConcurrentTasks);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_options.ChannelBufferFactor, 500);

        _channel = Channel.CreateBounded<QueuedWork>(
            new BoundedChannelOptions(_options.MaxConcurrentTasks * _options.ChannelBufferFactor) {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
    }

    public void Start(CancellationToken hostToken) {
        if (_started)
            throw new InvalidOperationException("Executor already started.");

        _started = true;
        _hostToken = hostToken;

        AdjustWorkers();
    }

    public async ValueTask<Guid> EnqueueAsync(
        Func<CancellationToken, Task> work,
        CancellationToken ct = default) {
        if (!_started)
            throw new InvalidOperationException("Executor not started.");

        var id = Guid.NewGuid();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _hostToken);

        var item = new QueuedWork(id, work, cts);

        _pending[id] = cts;
        await _channel.Writer.WriteAsync(item, ct);

        return id;
    }

    public bool TryDequeue(Guid id) {
        if (_pending.TryRemove(id, out var cts)) {
            cts.Cancel();
            cts.Dispose();
            return true;
        }
        return false;
    }

    public void SetConcurrency(int newConcurrency) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newConcurrency);

        _targetConcurrency = newConcurrency;

        if (_started)
            AdjustWorkers();
    }

    private void AdjustWorkers() {
        int delta = _targetConcurrency - _workers.Count;

        if (delta > 0) {
            for (int i = 0; i < delta; i++)
                StartWorker();
        }
        else if (delta < 0) {
            delta = -delta;
            foreach (var worker in _workers.Take(delta)) {
                worker.Value.Cancel();
                _workers.TryRemove(worker.Key, out _);
            }
        }
    }

    private void StartWorker() {
        int id = Interlocked.Increment(ref _workerIdSeed);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_hostToken);

        if (!_workers.TryAdd(id, cts))
            return;

        _ = Task.Run(() => WorkerLoopAsync(cts), CancellationToken.None);
    }

    private async Task WorkerLoopAsync(CancellationTokenSource workerCts) {
        try {
            while (await _channel.Reader.WaitToReadAsync(workerCts.Token)) {
                while (_channel.Reader.TryRead(out var item)) {

                    if (item.Cts.IsCancellationRequested) {
                        item.Cts.Dispose();
                        continue;
                    }

                    _pending.TryRemove(item.Id, out _);

                    try {
                        await item.Work(item.Cts.Token);
                    }
                    catch (OperationCanceledException) {
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "执行 Work 时出错");
                    }
                    finally {
                        item.Cts.Dispose();
                    }
                }
            }
        }
        catch (OperationCanceledException) {
        }
    }

    public void Dispose() {
        _channel.Writer.TryComplete();

        foreach (var worker in _workers.Values) {
            worker.Cancel();
            worker.Dispose();
        }

        foreach (var cts in _pending.Values) {
            cts.Cancel();
            cts.Dispose();
        }

        _pending.Clear();
        _workers.Clear();
    }

    private sealed record QueuedWork(
        Guid Id,
        Func<CancellationToken, Task> Work,
        CancellationTokenSource Cts
    );
}
