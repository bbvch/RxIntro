namespace Bbv.Rx.Workshop
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Xunit;

    public class Interfacing
    {
        public static event EventHandler<string> MessageReceived;

        [Fact]
        public void ObservablesDoNotSubscribeUnnecessarily()
        {
            var observable = Observable.FromEventPattern<string>(
                ev => MessageReceived += ev,
                ev => MessageReceived -= ev);

            // When there are no observers, there should also be no event handler attached.
            // (FYI, of course there's a way to alter that behaviour, s.t. events don't get lost.)
            // TODO: convince yourself
            MessageReceived.ShouldBeEquivalentTo(string.Empty);
        }

        [Fact]
        public void ObservablesTakeCareOfTheEnvironment()
        {
            var observable = Observable.FromEventPattern<string>(
                ev => MessageReceived += ev,
                ev => MessageReceived -= ev);

            // For resource management reasons, events should be unsubscribed
            // once we are not interested in notifications anymore.
            // TODO: treat the returned subscription according to its interface
            observable.Select(ep => ep.EventArgs).Subscribe(_ => { });
            MessageReceived(this, "making visions work");

            MessageReceived.Should().BeNull();
        }

        [Fact]
        public void ObservablesAreLegacyEventCompatible()
        {
            this.MonitorEvents();

            var eventSource =
                Observable.Return("Hello again") // any regular event
                .Select(_ => new EventPattern<string>(this, _)) // can be turned into a sequence of events
                .ToEventPattern(); // And then into an object having an event

            //// TODO: do the wireing up

            this.ShouldRaise("MessageReceived");
        }

        [Fact]
        public void FromTasks()
        {
            var task = Task.Factory.StartNew(() => { Thread.Sleep(10); return "Done"; });
            
            var observable = task.ToObservable();
            
            // Testing of threaded observables is harder, because now we don't have a virtual scheduler.
            // There are (at least) two options:
            // - accept that we could have potentially long-running tests
            // - introduce the potential for spurious failures
            // We'll go with the second option here, similar to task.Wait(timeout)
            // TODO: Timeout the observable after e.g. 100ms (instead of throwing)
            observable = observable.Concat(Observable.Throw<string>(new NotImplementedException()));

            observable.Invoking(_ => _.Wait()).ShouldNotThrow();
        }
    }
}