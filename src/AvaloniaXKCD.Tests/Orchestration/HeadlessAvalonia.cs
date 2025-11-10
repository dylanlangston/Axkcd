namespace AvaloniaXKCD.Tests.Orchestration;

public class HeadlessAvalonia() : IAsyncInitializer, IAsyncDisposable
{
    HeadlessUnitTestSession? _session;

    public HeadlessUnitTestSession Session
    {
        get
        {
            if (_session == null) throw new NullReferenceException();
            return _session;
        }
        set => _session = value;
    }

    public async Task InitializeAsync()
    {
        _session = HeadlessUnitTestSession.StartNew(typeof(HeadlessAvaloniaApp));
        await Task.CompletedTask;
    }


    public async ValueTask DisposeAsync()
    {
        if (_session is not null)
        {
            _session.Dispose();
        }
        await Task.CompletedTask;
    }
}

public class HeadlessAvaloniaTestExecutor : ITestExecutor
{
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        var headlessAvalonia = context
            .Metadata
            .TestDetails
            .TestClassInjectedPropertyArguments
            .Single((a) => a.Key == nameof(HeadlessAvalonia)).Value as HeadlessAvalonia;

        if (headlessAvalonia == null) throw new NullReferenceException();

        var execution = await headlessAvalonia
            .Session
            .Dispatch(
                async () => await action(),
                context.Execution.CancellationToken
        );

        await execution;
    }
}