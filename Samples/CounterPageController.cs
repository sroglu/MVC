using System;
using mehmetsrl.MVC.core;

namespace mehmetsrl.MVC.Samples
{
    /// <summary>
    /// Sample <b>page</b>-type controller. Page controllers grab a pre-designed
    /// view from the <see cref="ViewManager"/>'s pageViews list. There is one
    /// instance per scene; the view is hidden by default and shown via
    /// <c>ViewManager.ShowPageView&lt;CounterView&gt;()</c>.
    ///
    /// Usage:
    /// <code>
    /// var data = new CounterData { Label = "Coins", Value = 0 };
    /// var ctrl = new CounterPageController(new CounterModel(data));
    /// ViewManager.ShowPageView&lt;CounterView&gt;();
    /// ctrl.Add();
    /// </code>
    /// </summary>
    public class CounterPageController : Controller<CounterView, CounterModel>
    {
        public CounterPageController(CounterModel model)
            : base(ControllerType.Page, model) { }

        public void Add()
        {
            Model.Increment();
            View.UpdateView();
            Redirect(SampleActions.CounterChanged);
        }

        public void ResetCounter()
        {
            Model.Reset();
            View.UpdateView();
        }

        protected override void OnActionRedirected(IController source, string action, EventArgs data)
        {
            // Listen to peer controllers if needed.
            // if (action == SampleActions.SomeOtherAction) { ... }
        }
    }
}
