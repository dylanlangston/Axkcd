using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Argon;
using VerifyTests;

namespace AvaloniaXKCD.Tests.VerifyPlugins;

public static class VerifyAssertionsPlugin
{
    private class AssertionContext
    {
        public required object Target { get; init; }
        public required VerifySettings Settings { get; init; }
    }

    private static readonly AsyncLocal<AssertionContext?> CurrentAssertionContext = new();
    private static readonly AsyncLocal<bool> AssertionsRun = new();

    internal static void SetAssertionTarget(object target, VerifySettings settings)
    {
        CurrentAssertionContext.Value = new() { Target = target, Settings = settings };
    }

    static readonly List<Action<object>> SharedAsserts = [];
    static readonly List<Func<object, Task>> SharedAsyncAsserts = [];

    public static void Assert<T>(Action<T> assert) =>
        SharedAsserts.Add(Wrap(assert));

    public static void Assert<T>(Func<T, Task> assert) =>
        SharedAsyncAsserts.Add(Wrap(assert));

    public static bool Initialized { get; private set; }

    public static void Initialize()
    {
        if (Initialized) throw new("Already Initialized");
        Initialized = true;

        InnerVerifier.ThrowIfVerifyHasBeenRun();

        VerifierSettings.OnVerify(
            before: () => RunAssertionsBeforeVerify().GetAwaiter().GetResult(),
            after: () => { AssertionsRun.Value = false; }); // Reset the flag after each verification

        VerifierSettings.AddExtraSettings(_ => _.Serializing += Serializing);
    }

    private static async Task RunAssertionsBeforeVerify()
    {
        var context = CurrentAssertionContext.Value;
        if (context != null)
        {
            await ExecuteAssertions(context.Target, context.Settings.Context);
            CurrentAssertionContext.Value = null;
        }
    }

    [Pure]
    public static SettingsTask Assert<T>(this SettingsTask settings, Action<T> assert)
    {
        settings.CurrentSettings.Assert(assert);
        return settings;
    }

    [Pure]
    public static SettingsTask Assert<T>(this SettingsTask settings, Func<T, Task> assert)
    {
        settings.CurrentSettings.Assert(assert);
        return settings;
    }

    public static void Assert<T>(this VerifySettings settings, Action<T> assert)
    {
        var context = settings.Context;
        if (TryGetAsserts(context, out var asserts))
        {
            asserts.Add(Wrap(assert));
            return;
        }
        context["AssertList"] = new List<Action<object>> { Wrap(assert) };
    }

    public static void Assert<T>(this VerifySettings settings, Func<T, Task> assert)
    {
        var context = settings.Context;
        if (TryGetAsyncAsserts(context, out var asserts))
        {
            asserts.Add(Wrap(assert));
            return;
        }
        context["AsyncAssertList"] = new List<Func<object, Task>> { Wrap(assert) };
    }

    public static async Task ExecuteAssertions(object target, IReadOnlyDictionary<string, object> context)
    {
        if (AssertionsRun.Value)
        {
            return;
        }
        AssertionsRun.Value = true;

        HandleAsserts(SharedAsserts, target);
        if (TryGetAsserts(context, out var asserts))
        {
            HandleAsserts(asserts, target);
        }

        await HandleAsyncAsserts(SharedAsyncAsserts, target);
        if (TryGetAsyncAsserts(context, out var asyncAsserts))
        {
            await HandleAsyncAsserts(asyncAsserts, target);
        }
    }

    static void Serializing(JsonWriter writer, object target)
    {
        if (CurrentAssertionContext.Value == null)
        {
            var verifyWriter = (VerifyJsonWriter)writer;
            ExecuteAssertions(target, verifyWriter.Context).GetAwaiter().GetResult();
        }
    }

    static Action<object> Wrap<T>(Action<T> assert) => _ => { if (_ is T t) assert(t); };

    static Func<object, Task> Wrap<T>(Func<T, Task> assert) => async _ => { if (_ is T t) await assert(t); };

    static bool TryGetAsserts(IReadOnlyDictionary<string, object> context, [NotNullWhen(true)] out List<Action<object>>? value)
    {
        if (context.TryGetValue("AssertList", out var list))
        {
            value = (List<Action<object>>)list;
            return true;
        }
        value = null;
        return false;
    }

    static bool TryGetAsyncAsserts(IReadOnlyDictionary<string, object> context, [NotNullWhen(true)] out List<Func<object, Task>>? value)
    {
        if (context.TryGetValue("AsyncAssertList", out var list))
        {
            value = (List<Func<object, Task>>)list;
            return true;
        }
        value = null;
        return false;
    }

    static void HandleAsserts(List<Action<object>> actions, object target)
    {
        foreach (var action in actions) action(target);
    }

    static async Task HandleAsyncAsserts(List<Func<object, Task>> actions, object target)
    {
        foreach (var action in actions) await action(target);
    }

    public static AssertionVerificationTask<T> Verify<T>(T target, VerifySettings? settings = null)
    where T : notnull
    {
        var verifier = Verifier.Verify(target, settings);
        return new AssertionVerificationTask<T>(target, verifier);
    }

    public static TAssertionType Assert<T, TAssertionType>(this TAssertionType task, Action<T> assert)
        where T : notnull
        where TAssertionType : AssertionVerificationTask<T>
    {
        task.SettingsTask.Assert(assert);
        return task;
    }

    public static TAssertionType Assert<T, TAssertionType>(this TAssertionType task, Func<T, Task> assert)
        where T : notnull
        where TAssertionType : AssertionVerificationTask<T>
    {
        task.SettingsTask.Assert(assert);
        return task;
    }

    public static AssertionVerificationTask<T> Assert<T>(this AssertionVerificationTask<T> task, Action<T> assert)
        where T : notnull
        => Assert<T, AssertionVerificationTask<T>>(task, assert);

    public static AssertionVerificationTask<T> Assert<T>(this AssertionVerificationTask<T> task, Func<T, Task> assert)
        where T : notnull
        => Assert<T, AssertionVerificationTask<T>>(task, assert);
}


public class AssertionVerificationTask<T> where T : notnull
{
    public T Target { get; init; }
    internal SettingsTask SettingsTask { get; private set; }

    public AssertionVerificationTask(T target, SettingsTask settingsTask)
    {
        Target = target;
        SettingsTask = settingsTask;
    }

    private async Task<VerifyResult> AsTask()
    {
        await VerifyAssertionsPlugin.ExecuteAssertions(Target, SettingsTask.CurrentSettings.Context);
        return await SettingsTask;
    }

    public TaskAwaiter<VerifyResult> GetAwaiter()
    {
        return AsTask().GetAwaiter();
    }

    public AssertionVerificationTask<T> UpdateSettings(Func<SettingsTask, SettingsTask> update)
    {
        SettingsTask = update(SettingsTask);
        return this;
    }

    public static implicit operator Task<VerifyResult>(AssertionVerificationTask<T> verificationTask) => verificationTask.AsTask();
}