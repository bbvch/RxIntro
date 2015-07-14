namespace Bbv.Rx.Workshop
{
    using System;
    using System.Reactive.Linq;

    using Bbv.Rx.Workshop.Infrastructure;

    using FluentAssertions;

    using Xunit;

    public class Events
    {
        public static event EventHandler<string> MessageReceived;

        [Fact]
        public void ObservablesDoNotSubscribeUnnecessarily()
        {
            var observable = Observable.FromEventPattern<string>(
                ev => MessageReceived += ev,
                ev => MessageReceived -= ev);

            // TODO: replace 'string.Empty' with something more suitable
            MessageReceived.ShouldBeEquivalentTo(string.Empty);
        }

        [Fact]
        public void ObservablesTakeCareOfTheEnvironment()
        {
            var observable = Observable.FromEventPattern<string>(
                ev => MessageReceived += ev,
                ev => MessageReceived -= ev);
            var observer = new TestSink<string>();

            // TODO: treat the returned subscription according to its interface
            observable.Select(ep => ep.EventArgs).Subscribe(observer);
            MessageReceived(this, "making visions work");

            MessageReceived.Should().BeNull();
        }

        [Fact]
        public void ObservablesAreLegacyEventCompatible()
        {
            var observable = Observable.FromEventPattern<string>(
                ev => MessageReceived += ev,
                ev => MessageReceived -= ev);
            var eventSource = observable.ToEvent();

            Assert.Throws<NullReferenceException>(() => MessageReceived(this, "Hello again"));

            //// TODO: restructure test s.t. 'MessageReceived' is not null and 'ShouldRaise' succeeds

            eventSource.MonitorEvents();
            eventSource.ShouldRaise("OnNext");
        }
    }
}