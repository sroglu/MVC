namespace mehmetsrl.MVC.Samples
{
    /// <summary>
    /// Centralised action-name constants. Demonstrates a workaround for the
    /// string-based <see cref="mehmetsrl.MVC.core.ControllerBase"/>.Redirect
    /// API — keep names in one place to avoid typos until type-safe overloads
    /// land (see MVC/MODULE.md "Known Limitations").
    /// </summary>
    public static class SampleActions
    {
        public const string CounterChanged = nameof(CounterChanged);
    }
}
