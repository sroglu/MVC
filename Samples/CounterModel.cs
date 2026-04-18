using mehmetsrl.MVC.core;

#pragma warning disable CS0618 // Samples demonstrate both legacy and scoped patterns
namespace mehmetsrl.MVC.Samples
{
    /// <summary>
    /// Sample model wrapping <see cref="CounterData"/>. Demonstrates the
    /// Model&lt;T&gt; constructor pattern (auto-registers via RegisterNewModel).
    /// </summary>
    public class CounterModel : Model<CounterData>
    {
        public CounterModel(MvcContext context, CounterData data) : base(context, data) { }

        public CounterModel(CounterData data) : base(data) { }

        public void Increment()
        {
            CurrentData.Value += 1;
            UpdateDescriptionData();
        }

        public void Reset()
        {
            CurrentData.Value = 0;
            UpdateDescriptionData();
        }
    }
}
