namespace AvaloniaXKCD.Tests.TestBases;

[TestExecutor<HeadlessAvaloniaTestExecutor>]
public abstract class AvaloniaBaseTest
{
    [ClassDataSource<HeadlessAvalonia>(Shared = SharedType.PerTestSession)]
    public required HeadlessAvalonia HeadlessAvalonia { get; init; }
}
