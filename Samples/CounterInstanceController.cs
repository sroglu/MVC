using System;
using mehmetsrl.MVC.core;

#pragma warning disable CS0618 // Samples demonstrate both legacy and scoped patterns
namespace mehmetsrl.MVC.Samples
{
    /// <summary>
    /// Sample <b>instance</b>-type controller. Instance controllers ask the
    /// <see cref="ViewManager"/> to instantiate a view prefab (picked from
    /// instancePrefabs[] by matching type). One controller ⇄ one view ⇄ one
    /// GameObject. Typical use: inventory slots, enemy units, list items.
    ///
    /// Usage:
    /// <code>
    /// foreach (var cfg in questConfigs)
    /// {
    ///     var data = new CounterData { Label = cfg.Name, Value = cfg.Starting };
    ///     var ctrl = new CounterInstanceController(new CounterModel(data));
    ///     // ctrl.View is a freshly instantiated GameObject; position it, parent it.
    /// }
    /// </code>
    /// </summary>
    public class CounterInstanceController : Controller<CounterView, CounterModel>
    {
        public CounterInstanceController(MvcContext context, CounterModel model)
            : base(context, ControllerType.Instance, model) { }

        public CounterInstanceController(CounterModel model)
            : base(ControllerType.Instance, model) { }

        public void Add(int amount)
        {
            for (int i = 0; i < amount; i++) Model.Increment();
            View.UpdateView();
        }

        protected override void OnActionRedirected(IController source, string action, EventArgs data)
        {
            if (action == SampleActions.CounterChanged)
            {
                // Another counter changed — could refresh or react.
                View.UpdateView();
            }
        }
    }
}
