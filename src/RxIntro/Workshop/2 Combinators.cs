namespace Bbv.Rx.Workshop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

    using Bbv.Rx.Workshop.Infrastructure;

    using FluentAssertions;

    using Microsoft.Reactive.Testing;

    using Xunit;

    public class Combinators : ReactiveTest
    {
        [Fact]
        public void Filtering()
        {
            var observable = Observable.Create<string>(
                o =>
                {
                    o.OnNext("This");
                    o.OnNext("is yet");
                    o.OnNext("another"); // length = 7
                    o.OnNext("way of");
                    o.OnNext("constructing");
                    o.OnNext("observables");
                    o.OnCompleted();
                    return Disposable.Empty;
                });

            // TODO: using LINQ query syntax or extension methods filter s.t. the variable name is descriptive
            var constructingObservables = observable;

            var observer = new TestSink<string>();
            constructingObservables.Subscribe(observer);

            observer.Values.ShouldAllBeEquivalentTo(new[] { "constructing", "observables" });
        }

        [Fact]
        public void Merging()
        {
            var scheduler = new TestScheduler();

            var observableA = scheduler.CreateHotObservable(
                OnNext(100, 'A'),
                OnNext(300, 'C'));

            var observableB = scheduler.CreateHotObservable(
                OnNext(200, 'B'));

            var interleaved = observableA.Merge(observableB);

            var observer = new TestSink<char>();
            interleaved.Subscribe(observer);

            scheduler.Start(() => interleaved, 0, 10, 500);

            // TODO: construct an array containing some characters from the beginning of the alphabet
            char[] expected = null;

            observer.Values.ShouldAllBeEquivalentTo(expected);
        }

        [Fact]
        public void OneAfterTheOther()
        {
            var scheduler = new TestScheduler();

            var observableA = scheduler.CreateColdObservable(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(300, 3),
                OnCompleted<int>(400));

            var observableB = scheduler.CreateColdObservable(
                OnNext(210, 10),
                OnNext(310, 20),
                OnNext(410, 30));

            // TODO: concatenate the two observables
            IObservable<int> firstAThenB = null;

            var events = scheduler.Start(() => firstAThenB, 0, 10, 1000);

            events.Messages.AssertEqual(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(300, 3),
                OnNext(620, 10),
                OnNext(720, 20),
                OnNext(820, 30));
        }

        [Fact]
        public void Debounce()
        {
            var scheduler = new TestScheduler();

            var fastObservable = scheduler.CreateColdObservable(
                OnNext(300, 1),
                OnNext(350, 2),
                OnNext(450, 3),
                OnNext(600, 4));

            // TODO: ignore elements following each other in less than or equal 100 ticks
            // Hint: do not forget to pass the scheduler
            var throttledObservable = fastObservable;

            var observer = scheduler.Start(() => throttledObservable, 0, 0, 1000);

            observer.Messages.AssertEqual(
                OnNext(450, 3),
                OnNext(600, 4));
        }

        [Fact]
        public void Batching()
        {
            var scheduler = new TestScheduler();
            var tourists = scheduler.CreateColdObservable(
                OnNext(300, "Alice"),
                OnNext(430, "Bob"),
                OnNext(481, "Carol"),
                OnNext(501, "Dave"),
                OnNext(505, "Eric"),
                OnNext(505, "Floyd"),
                OnCompleted<string>(700));
            var cableCar = new TestSink<IEnumerable<string>>();

            // - The cable car leaves as soon as it is full (2 persons) or every 120 ticks
            // - The cable car should only leave, when tourists are present
            // TODO: Split the tourists into suitable batches
            // (hint: 2 steps, waiting areas are buffers) and subscribe the cableCar
            //// tourists. BUFFER . ONLY WHEN NOT EMPTY . SUBSCRIBE

            scheduler.Start();

            cableCar.Values.ShouldAllBeEquivalentTo(
                new[] { new[] { "Alice" }, new[] { "Bob" }, new[] { "Carol", "Dave" }, new[] { "Eric", "Floyd" } });
        }

        [Fact]
        public void Scrum()
        {
            var scheduler = new TestScheduler();
            var thermoSensor = new Random(42);

            var temperatureReadings =
                Observable.Interval(TimeSpan.FromMilliseconds(100), scheduler).Select(_ => thermoSensor.Next(-273, 100));

            // For infinite 'bouncing' series we cannot use the "Debounce" solution.
            // TODO: calculate the moving (every second) average (over one minute) of the temperature readings.
            // Hint: use a suitable overload of the same method used in "Batching" and calculate the average using LINQ
            var averages =
                temperatureReadings.Average(/* TODO: replace, only here for type checking */);

            var observer = new TestSink<double>();
            averages.Subscribe(observer);

            scheduler.AdvanceTo(TimeSpan.TicksPerMinute * 2);

            observer.Values.Count().Should().Be(61);
        }

        [Fact]
        public void Interruptible()
        {
            var scheduler = new TestScheduler();

            var observable =
                Observable.Interval(TimeSpan.FromTicks(100), scheduler)
                .Select(i => Observable.Interval(TimeSpan.FromTicks(40), scheduler).Select(_ => i));

            var observer = new TestSink<long>();

            observable.Switch().Subscribe(observer);

            scheduler.AdvanceTo(300);

            // TODO: looking at the code above, try to infer the expected value (hint: two pairs)
            observer.Values.ShouldAllBeEquivalentTo(Enumerable.Empty<long>());
        }

        [Fact]
        public void AfterYou()
        {
            var scheduler = new TestScheduler();

            // Opposed to cold observables that generate their values everytime an observer subscribes
            // hot observables run "in the background" (think touching the CPU when no subscribers are around).
            var observableA = scheduler.CreateHotObservable(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(300, 3),
                OnCompleted<int>(400));

            var observableB = scheduler.CreateHotObservable(
                OnNext(210, 10),
                OnNext(310, 20),
                OnNext(410, 30));

            var firstAThenB = observableA.Concat(observableB);

            var events = scheduler.Start(() => firstAThenB, 0, 10, 1000);

            // TODO: try making the test pass (without looking at the test output)
            // Hint: some events might already have passed once the subscription gets to the second observable.
            events.Messages.AssertEqual(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(300, 3));
        }
    }
}
