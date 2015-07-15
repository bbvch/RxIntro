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
        public async void Exercise_Filtering()
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

            // TODO: using LINQ query syntax or extension methods filter
            // s.t. only the terms with a length of 8 and more are returned.
            var constructingObservables = observable;

            var result = await constructingObservables.ToList();
            result.ShouldAllBeEquivalentTo(new[] { "constructing", "observables" });
        }

        [Fact]
        public void Exercise_Merging()
        {
            var scheduler = new TestScheduler();

            var observableA = scheduler.CreateHotObservable(
                OnNext(100, 'A'),
                OnNext(300, 'C'));

            var observableB = scheduler.CreateColdObservable(
                OnNext(200, 'B'));

            var interleaved = observableA.Merge(observableB);

            var observer = new TestSink<char>();
            interleaved.Subscribe(observer);

            scheduler.Start(() => interleaved, 0, 10, 500);

            // TODO: this should be obvious
            var expected = new char[] { };

            observer.Values.ShouldAllBeEquivalentTo(expected);
        }

        [Fact]
        public void Exercise_OneAfterTheOtherWhenCold()
        {
            var scheduler = new TestScheduler();

            // Cold observables generate their values everytime an observer subscribes
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
        public void Exercise_OneAfterTheOtherWhenHot()
        {
            var scheduler = new TestScheduler();

            // Hot observables run "in the background" (think touching the CPU when no subscribers are around).
            // I.e. cold observables timings are relative to the subscription whereas hot observables timings
            // are relative to their creation. A typical instance of a hot observable are mouse events:
            // they happen regardless of subscriptions and once they've happened, they're gone (unless 'replay'ed).
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

            // TODO: try making the test pass by adding OnNext statement(s) at the end of the AssertEqual statement
            // Hint: (re)read the second part of the initial comment.
            events.Messages.AssertEqual(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(300, 3));
        }

        [Fact]
        public void Exercise_Debounce()
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
        public async void Exercise_OnlyNewStuff()
        {
            var temperature = new[] { 20, 20, 21, 22, 22, 21, 24 }.ToObservable();

            // As with records from a database, we might only be interested in distinct notifications.
            // For reactive streams, having strictly distinct values implies waiting for completion and
            // caching all intermediate results. A more common use case is to only let through values that
            // changed relatively to the previous one.
            // TODO: only return changed respectivelly new temperature values
            var temperatureChangesToShow = temperature;

            var result = await temperatureChangesToShow.ToList();
            result.ShouldAllBeEquivalentTo(new[] { 20, 21, 22, 21, 24 });
        }

        [Fact]
        public void Exercise_Batching()
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
        public void Exercise_Scrum()
        {
            var scheduler = new TestScheduler();
            var thermoSensor = new Random(42);

            var temperatureReadings =
                Observable.Interval(TimeSpan.FromMilliseconds(100), scheduler).Select(_ => thermoSensor.Next(-273, 100));

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
        public void Exercise_Interruptible()
        {
            var scheduler = new TestScheduler();

            // For a news site offering articles about trending topics we want to keep reading about
            // a topic until a new trend appears, when we want to hear about the latest news there.
            // For the sake of simplicity, we assume fixed rates for topics and articles:
            var articlesInTopics =
                Observable.Interval(TimeSpan.FromTicks(100), scheduler)
                .Select(topic => Observable.Interval(TimeSpan.FromTicks(40), scheduler)
                    .Select(article => new Tuple<long, long>(topic, article)));

            var observer = new TestSink<Tuple<long, long>>();

            // TODO: always switch to the latest topic
            var latestInteresting = (IObservable<Tuple<long, long>>)articlesInTopics;

            latestInteresting.Subscribe(observer);

            scheduler.AdvanceTo(300);

            observer.Values.ShouldAllBeEquivalentTo(new[]
                                                        {
                                                            new Tuple<long, long>(0, 0),
                                                            new Tuple<long, long>(0, 1),
                                                            new Tuple<long, long>(1, 0),
                                                            new Tuple<long, long>(1, 1),
                                                        });
        }
    }
}
