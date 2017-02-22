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

            // TODO: Use LINQ query syntax or extension methods to return only the terms where the length is greather than 7
            // HINT: http://reactivex.io/documentation/operators/filter.html
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

            // HINT: http://reactivex.io/documentation/operators/merge.html
            var expected = new char[] { /* TODO: list the expected values with the correct order here */ };

            observer.Values.ShouldAllBeEquivalentTo(expected);
        }

        [Fact]
        public void Exercise_OneAfterTheOtherWhenCold()
        {
            var scheduler = new TestScheduler();

            /* source: http://reactivex.io/documentation/observable.html
             * “Hot” and “Cold” Observables
             * When does an Observable begin emitting its sequence of items? It depends on the Observable. A “hot” Observable
             * may begin emitting items as soon as it is created, and so any observer who later subscribes to that Observable
             * may start observing the sequence somewhere in the middle. A “cold” Observable, on the other hand, waits until 
             * an observer subscribes to it before it begins to emit items, and so such an observer is guaranteed to see the 
             * whole sequence from the beginning. (Remark: Their timings are relative to the subscription.)
             * In some implementations of ReactiveX, there is also something called a “Connectable” Observable. Such an Observable
             * does not begin emitting items until its Connect method is called, whether or not any observers have subscribed to it. 
             */
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
            // HINT: http://reactivex.io/documentation/operators/concat.html
            var firstAThenB = Observable.Empty<int>();

            const int CreateObservableAt = 0;
            const int SubscribeAt = 10;
            const int UnsubscribeAt = 1000;

            var events = scheduler.Start(() => firstAThenB, CreateObservableAt, SubscribeAt, UnsubscribeAt);

            // NOTE: the messages are shifted by SubscribeAt due to the observable
            // creating its values on demand when an observer subscribes.
            // The second observable then starts after the first one completes at 400.
            events.Messages.AssertEqual(
                OnNext(110, 1),
                OnNext(210, 2),
                OnNext(310, 3),
                OnNext(620, 10),
                OnNext(720, 20),
                OnNext(820, 30));
        }

        [Fact]
        public void Exercise_OneAfterTheOtherWhenHot()
        {
            var scheduler = new TestScheduler();

            // Hot observables run "in the background" (think touching the CPU when no subscribers are around).
            // Their timings are relative to their creation. A typical instance of a hot observable are mouse events:
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

            // TODO: Add the missing OnNext statement(s) at the end of the AssertEqual statement
            // HINT 1: http://reactivex.io/documentation/operators/concat.html
            // HINT 2: (re)read the second part of the initial comment.
            // HINT 3: think about what happens when observableA completes.
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

            // TODO: ignore elements preceeding another in less than or equal 100 ticks
            // HINT 1: http://reactivex.io/documentation/operators/debounce.html
            // HINT 2: do not forget to pass the scheduler
            // HINT 3: in Rx.NET the method to use is named differently (see end of HINT 1)
            var throttledObservable = fastObservable;

            var observer = scheduler.Start(() => throttledObservable, 0, 0, 1000);

            // How do this values relate to the setup above?
            observer.Messages.AssertEqual(
                OnNext(551, 3),
                OnNext(701, 4));
        }

        [Fact]
        public async void Exercise_OnlyNewStuff()
        {
            var temperature = new[] { 20, 20, 21, 22, 22, 21, 24 }.ToObservable();

            // As with records from a database, we might only be interested in distinct notifications.
            // For reactive streams, having strictly distinct values implies waiting for completion and
            // caching all intermediate results. A more common use case is to only let through values that
            // changed relatively to the previous one.
            // TODO: only return changed temperature values (includes the first, new one)
            // HINT: http://reactivex.io/documentation/operators/distinct.html (almost)
            var temperatureChangesToShow = temperature;

            var result = await temperatureChangesToShow.ToList();
            result.ShouldAllBeEquivalentTo(new[] { 20, 21, 22, 21, 24 });
        }

        [Fact]
        public void Exercise_Batching()
        {
            var scheduler = new TestScheduler();
            var cableCar = new TestSink<IEnumerable<string>>();

            var tourists = scheduler.CreateColdObservable(
                OnNext(300, "Alice"),
                OnNext(430, "Bob"),
                OnNext(481, "Carol"),
                OnNext(501, "Dave"),
                OnNext(505, "Eric"),
                OnNext(505, "Floyd"),
                OnCompleted<string>(700));

            // - The cable car leaves: 
            //   a) when there are 2 persons in the car
            //      -- or --
            //   b) when 120 ticks passed
            //
            // - The cable car should only leave, when tourists are present
            //
            // TODO: Split the tourists into suitable batches
            // HINT 1: http://reactivex.io/documentation/operators/buffer.html (suitable overload)
            // HINT 2: use LINQ to filter empty batches
            // HINT 3: don't forget to pass the scheduler
            var touristBatches = Observable.Empty<IEnumerable<string>>();
            var nonEmptyBatches = touristBatches;

            nonEmptyBatches.Subscribe(cableCar);

            scheduler.Start();

            cableCar.Values.ShouldAllBeEquivalentTo(
                new[] { new[] { "Alice" }, new[] { "Bob" }, new[] { "Carol", "Dave" }, new[] { "Eric", "Floyd" } });
        }

        [Fact]
        public void Exercise_MovingAverage()
        {
            var scheduler = new TestScheduler();
            var observer = new TestSink<double>();
            var thermoSensor = new Random(42);

            var temperatureReadings =
                Observable.Interval(TimeSpan.FromMilliseconds(100), scheduler).Select(_ => thermoSensor.Next(-273, 100));

            // TODO: calculate the moving (every second) average (over one minute) of the temperature readings.
            // Step 1) build batches of the temperature readings
            //         - batch length = 1 minute
            //         - time shift   = 1 second
            // Step 2) calculate the average value of the batches
            //
            // HINT 1: http://reactivex.io/documentation/operators/buffer.html (suitable overload)
            // HINT 2: use a suitable overload of the same method used in "Batching"
            // and then calculate the average of each batch using LINQ
            // HINT 3: don't forget to pass the scheduler
            var slidingBuffers = Observable.Empty<IEnumerable<double>>();
            var averages = Observable.Empty<double>();

            averages.Subscribe(observer);
            scheduler.AdvanceTo(TimeSpan.TicksPerMinute * 2);

            observer.Values.Should().HaveCount(61);
        }

        [Fact]
        public void Exercise_Interruptible()
        {
            var scheduler = new TestScheduler();
            var observer = new TestSink<Tuple<long, long>>();

            // For a news site offering articles about trending topics we want to keep reading about
            // a topic until a new trend appears, when we want to hear about the latest news there.
            // For the sake of simplicity, we assume fixed rates for topics and articles:
            var articlesInTopics =
                Observable.Interval(TimeSpan.FromTicks(100), scheduler)
                .Select(topic => Observable.Interval(TimeSpan.FromTicks(40), scheduler)
                    .Select(article => new Tuple<long, long>(topic, article)));

            // TODO: always switch to the latest topic
            // HINT: http://reactivex.io/documentation/operators/switch.html
            var latestInteresting = Observable.Empty<Tuple<long, long>>();

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
