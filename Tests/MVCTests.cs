using System;
using System.Collections.Generic;
using System.Reflection;
using mehmetsrl.MVC.core;
using NUnit.Framework;

namespace mehmetsrl.MVC.Tests
{
    // ---- Test fixtures share these helpers ----

    internal class StubModel : IModel
    {
        public uint InstanceId(int subModelIndex = 0) => 0;
        public void Dispose() { }
    }

    internal class StubController : ControllerBase
    {
        public readonly List<(IController source, string action, EventArgs data)> Received = new();

        public StubController(ControllerType type) : base(type) { }

        public override IModel GetModel() => new StubModel();
        public override ViewBase GetView() => null;
        public override void Dispose() { }

        protected override void OnActionRedirected(IController source, string action, EventArgs data)
        {
            Received.Add((source, action, data));
        }

        // Expose protected Redirect overloads for testing.
        public void Broadcast(string action, EventArgs data = null) => Redirect(action, data);
        public void TargetedRedirect(string action, string controllerName, EventArgs data = null)
            => Redirect(action, controllerName, data);
    }

    // -----------------------------------------------------------------------

    [TestFixture]
    public class ControllerTypeTests
    {
        [Test]
        public void EnumValues_AreInPascalCase()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ControllerType), "Page"));
            Assert.IsTrue(Enum.IsDefined(typeof(ControllerType), "Instance"));
            Assert.AreEqual(2, Enum.GetValues(typeof(ControllerType)).Length);
        }
    }

    // -----------------------------------------------------------------------

    [TestFixture]
    public class ControllerRedirectTests
    {
        // ControllerBase keeps a static delegate of all subscriber controllers.
        // Reset it between tests to avoid cross-test contamination.
        [TearDown]
        public void ClearStaticRedirect()
        {
            var field = typeof(ControllerBase)
                .GetField("RedirectToAction", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field, "ControllerBase.RedirectToAction field shape changed — update test.");
            field.SetValue(null, null);
        }

        [Test]
        public void Broadcast_DeliversToEverySubscribedController()
        {
            var c1 = new StubController(ControllerType.Page);
            var c2 = new StubController(ControllerType.Instance);
            var c3 = new StubController(ControllerType.Instance);

            c1.Broadcast("Hello");

            Assert.AreEqual(1, c1.Received.Count);
            Assert.AreEqual(1, c2.Received.Count);
            Assert.AreEqual(1, c3.Received.Count);
            Assert.AreEqual("Hello", c2.Received[0].action);
            Assert.AreSame(c1, c2.Received[0].source);
        }

        [Test]
        public void TargetedRedirect_DeliversOnlyToMatchingControllerType()
        {
            var c1 = new StubController(ControllerType.Page);
            var c2 = new StubController(ControllerType.Instance);

            c1.TargetedRedirect("PingC2", c2.GetType().ToString());

            // c1 type-name doesn't match (it's the same StubController type, so it WILL match).
            // Both c1 and c2 are StubController — this verifies the by-type-name filter logic.
            Assert.AreEqual(1, c1.Received.Count, "Self of same type should match the type-name filter.");
            Assert.AreEqual(1, c2.Received.Count);
            Assert.AreEqual("PingC2", c2.Received[0].action);
        }

        [Test]
        public void TargetedRedirect_NoMatchingType_DeliversNothing()
        {
            var c1 = new StubController(ControllerType.Page);
            var c2 = new StubController(ControllerType.Instance);

            c1.TargetedRedirect("Ghost", "DoesNotExist.Type");

            Assert.AreEqual(0, c1.Received.Count);
            Assert.AreEqual(0, c2.Received.Count);
        }

        [Test]
        public void Broadcast_PassesEventArgsThrough()
        {
            var c1 = new StubController(ControllerType.Page);
            var c2 = new StubController(ControllerType.Instance);
            var args = new EventArgs();

            c1.Broadcast("WithData", args);

            Assert.AreSame(args, c1.Received[0].data);
            Assert.AreSame(args, c2.Received[0].data);
        }
    }

    // -----------------------------------------------------------------------

    [TestFixture]
    public class ModelRegistryTests
    {
        private class IntData : ICloneable
        {
            public int X;
            public object Clone() => new IntData { X = X };
        }

        private class IntModel : Model<IntData>
        {
            public IntModel(IntData data) : base(data) { }
        }

        [SetUp]
        public void Reset() => ModelBase.ResetDictionary();

        [Test]
        public void NewSingleModel_GetsIncrementalInstanceId()
        {
            var m1 = new IntModel(new IntData { X = 1 });
            var m2 = new IntModel(new IntData { X = 2 });

            Assert.AreEqual(1u, m1.instanceId);
            Assert.AreEqual(2u, m2.instanceId);
            Assert.IsTrue(m1.Initiated);
        }

        [Test]
        public void GetModelByInstanceId_ReturnsTheRegisteredModel()
        {
            var m = new IntModel(new IntData { X = 42 });

            var fetched = ModelBase.GetModelByInstanceId(m.instanceId);

            Assert.AreSame(m, fetched);
        }

        [Test]
        public void Dispose_UnregistersSingleModelFromDictionary()
        {
            var m = new IntModel(new IntData { X = 9 });
            var id = m.instanceId;

            m.Dispose();

            Assert.Throws<KeyNotFoundException>(() => ModelBase.GetModelByInstanceId(id));
        }

        [Test]
        public void Update_ReplacesCurrentData()
        {
            var m = new IntModel(new IntData { X = 1 });

            m.Update(new IntData { X = 7 });

            Assert.AreEqual(7, m.CurrentData.X);
        }
    }

    // -----------------------------------------------------------------------

    [TestFixture]
    public class EmptyComponentTests
    {
        [SetUp]
        public void Reset() => ModelBase.ResetDictionary();

        [Test]
        public void EmptyModel_RegistersAndDisposesCleanly()
        {
            var m = new EmptyModel(new EmptyData());

            Assert.AreEqual(1u, m.instanceId);
            Assert.IsTrue(m.Initiated);
            Assert.DoesNotThrow(() => m.Dispose());
        }
    }
}
