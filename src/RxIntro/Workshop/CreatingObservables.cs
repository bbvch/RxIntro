namespace Bbv.Rx.Workshop
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    using Bbv.Rx.Workshop.Infrastructure;

    using FluentAssertions;

    using Microsoft.Reactive.Testing;

    using Xunit;

    public class CreatingObservables
    {
        [Fact]
        public void Exercise_UsingFactoryMethod()
        {
            // The preferred way of creating production observables is through combinators
            // (i.e. Return, Never, Interval, ...). In tests predefined sequences are predominant.
            // For more complex scenarios there is Observable.Create(...) and Observable.Generate(...)
            var scheduler = new TestScheduler();
            var random = new Random(7);

            // TODO: Create an infinite observable generating integers at random times but at least every 100 ticks
            var observable = Observable.Never<int>();

            // Extremely poor mans randomness test, for better variants see Knuth's TAOCP
            var result = scheduler.Start(() => observable, 1000);
            var timings = result.Messages.Select(m => m.Time).Distinct();
            timings.Count().Should().BeGreaterThan(1);
        }

        [Fact]
        public void Exercise_FromSubjects()
        {
            // Subjects are IObservable and IObserver at the same time.
            // Being the mutable variable equivalent of observables they can be used
            // to imperatively send events (e.g. for reactive properties, replacing IPropertyChanged).
            // For a detailed discussion on when subjects should be avoided
            // see http://davesexton.com/blog/post/To-Use-Subject-Or-Not-To-Use-Subject.aspx.
            var subject = new Subject<int>();
            var observer = new TestSink<int>();
            subject.Subscribe(observer);

            //// TODO: send some notifications

            observer.Values.Should().NotBeEmpty();
        }
    }
}
