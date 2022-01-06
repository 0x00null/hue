using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroNull.Hue.Tests.Control.CollectingSink
{
    [TestClass]
    public class CollectingSinkTests
    {
        /// <summary>
        /// Given: A collecting sink
        /// When: Waiting for input
        /// Then: The Sink must first be started
        /// </summary>
        [TestMethod]
        public async Task SinkMustBeStarted()
        {
            var sink = new ZeroNull.Hue.Control.CollectingSink(TestLogger.Instance);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await sink.WaitForInput());
        }

        /// <summary>
        /// Given: A collecting sink
        /// When: Waiting for input and sending simple, slow, discreet events
        /// Then: events are receieved as expected
        /// </summary>
        [TestMethod]
        public async Task SimpleEvents()
        {
            var sink = new ZeroNull.Hue.Control.CollectingSink(TestLogger.Instance);
            var cst = new CancellationTokenSource();
            
            var sinkTask = sink.Start(cst.Token);

            // send three events, with 500ms between each one
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "1",
                Type = Hue.Control.ControlEventType.ShortPress
            });
            await Task.Delay(500);
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "2",
                Type = Hue.Control.ControlEventType.ShortPress
            });
            await Task.Delay(500);
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "3",
                Type = Hue.Control.ControlEventType.ShortPress
            });


            // We should then get three events out when waiting for input...
            var e1 = await sink.WaitForInput(100);
            var e2 = await sink.WaitForInput(100);
            var e3 = await sink.WaitForInput(100);
            var e4 = await sink.WaitForInput(100);

            Assert.AreEqual("1", e1.InputId);
            Assert.AreEqual("2", e2.InputId);
            Assert.AreEqual("3", e3.InputId);
            Assert.IsNull(e4);

            cst.Cancel();
            await sinkTask;
        }


        /// <summary>
        /// Given: A collecting sink
        /// When: Waiting for input and sending simple, slow, discreet events
        /// Then: events are receieved as expected
        /// </summary>
        [TestMethod]
        public async Task FrequentEvents()
        {
            var sink = new ZeroNull.Hue.Control.CollectingSink(TestLogger.Instance);
            var cst = new CancellationTokenSource();

            var sinkTask = sink.Start(cst.Token);

            // send six events - four rapid events, followed by a delayed event, followed by an event for a different input
            // this should result in four events: the first event, the fourth event, the fifth event and the sixth event

            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "1",
                Type = Hue.Control.ControlEventType.ScalarChanged,
                Value = 0x01
            });
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "1",
                Type = Hue.Control.ControlEventType.ScalarChanged,
                Value = 0x02,
            });
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "1",
                Type = Hue.Control.ControlEventType.ScalarChanged,
                Value = 0x03,
            });
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "1",
                Type = Hue.Control.ControlEventType.ScalarChanged,
                Value = 0x04,
            });
            await Task.Delay(200);
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "1",
                Type = Hue.Control.ControlEventType.ScalarChanged,
                Value = 0x05,
            });
            sink.NotifyEvent(new Hue.Control.ControlEvent()
            {
                InputId = "2",
                Type = Hue.Control.ControlEventType.ShortPress
            });


            // We should then get three events out when waiting for input...
            var e1 = await sink.WaitForInput(100);
            var e2 = await sink.WaitForInput(100);
            var e3 = await sink.WaitForInput(100);
            var e4 = await sink.WaitForInput(100);
            var e5 = await sink.WaitForInput(100);

            Assert.AreEqual(0x01, e1.Value);
            Assert.AreEqual(0x04, e2.Value);
            Assert.AreEqual(0x05, e3.Value);
            Assert.AreEqual("2", e4.InputId);

            Assert.IsNull(e5);


            cst.Cancel();
            await sinkTask;
        }
    }
}
