namespace Bbv.Rx.Workshop
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;

    using Bbv.Rx.Workshop.Infrastructure;

    using FluentAssertions;

    using Microsoft.Reactive.Testing;

    using Xunit;

    public class Misc
    {
        [Fact]
        public async void Exercise_ObservablesAreCollectionlike()
        {
            // We want to approximate the largest value X s.t. 0 < X < Math.PI and cos(X) > 0
            const double Precision = 0.01;
            var observable = Observable.Generate(0.0, i => i < Math.PI, i => i + Precision, i => i);

            // TODO: write a LINQ query filtering for x where Math.Cos(x) > 0
            var cosPositive = observable;

            // TODO: get the last value (hint: look at the signature of this test method)
            var lastBeforeZero = 0.0;

            lastBeforeZero.Should().BeApproximately(Math.PI / 2, Precision);
        }

        [Fact]
        public void Exercise_ObservablesAndSchedulers()
        {
            var observable = Observable.Return(1983);
            var observer = new TestSink<int>();
            var firstThreadId = Thread.CurrentThread.ManagedThreadId;

            // TODO: find a way to schedule observations on a different thread
            // HINT: might also be done using an overload when creating the observable
            observable = observable;

            observable.Select(_ => Thread.CurrentThread.ManagedThreadId).Subscribe(observer);

            observable.Wait();
            firstThreadId.Should().NotBe(observer.Values.Single());
        }

        [Fact]
        public void Note_TestingObservables()
        {
            // Testing Rx components is simple in principle (as you've seen) but can be complex for
            // more advanced use cases, including threading, asynchronism, timing, schedulers and so forth.
            // Here, we'll only have a glimpse of how this might be done... Basically, we have a scheduler
            // running arbitrarily fast by always setting its internal clock to the next event.
            // This allows for almost immediate occurrence of future events and replaying of history.
            var now = DateTime.UtcNow;
            var tomorrow = now.AddDays(1);
            var scheduler = new TestScheduler();

            // Concurrency is only introduced using schedulers. All methods needing an IScheduler also have
            // overrides using the default scheduler. For testing, we need to provide a custom one.
            // TODO: pass 'scheduler' at the right place
            var observable = Observable.Return(string.Empty).Delay(tomorrow).Select(_ => DateTime.Now);

            var systemStart = now.AddDays(-1).Ticks;
            var subscriptionAt = now.Ticks;
            var simulateUpTo = now.AddYears(1).Ticks;

            var observer = scheduler.Start(() => observable, systemStart, subscriptionAt, simulateUpTo);

            var simulatedTime = observer.Messages.First().Time;
            var realTime = observer.Messages.First().Value.Value;

            // the simulation can't adjust the system clock
            realTime.Should().BeWithin(now.AddMilliseconds(1) - now);

            // the message is scheduled at time T and observed at T + 1
            simulatedTime.Should().Be(tomorrow.Ticks + 1);
        }
    }
}
