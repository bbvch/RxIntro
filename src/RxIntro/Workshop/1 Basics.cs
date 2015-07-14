namespace Bbv.Rx.Workshop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using Bbv.Rx.Workshop.Infrastructure;

    using FluentAssertions;

    using Microsoft.Reactive.Testing;

    using Xunit;

    public class Basics : ReactiveTest
    {

        [Fact]
        public void ObservableAndObserver()
        {
            // TODO: create an 'Observable' that 'return's an integer
            var observable = Observable.Empty<int>();
            var observer = new TestSink<int>();

            //// TODO: subscribe to the observable

            observer.Values.Single().Should().Be(42);
        }

        [Fact]
        public void FromCollection()
        {
            var collection = new[] { "Hello", "world!" };

            // TODO: create an observable from the collection
            var observable = collection;

            observable.Should().BeAssignableTo<IObservable<string>>();
        }

        [Fact]
        public void ToLazy()
        {
            var observable = Observable.Repeat("FooBar");

            // TODO: create an enumerable from the observable
            var lazyEnumerable = observable;

            lazyEnumerable.Should().BeAssignableTo<IEnumerable<string>>();
        }

        [Fact]
        public async void ToStrict()
        {
            var observable = Observable.Repeat("FooBar");

            // TODO: create a strict collection from the observable
            // Hint: also find a way to terminate the infinite stream of elements (e.g. after a fixed number)
            var strictCollection = observable;

            strictCollection.Should().BeAssignableTo<ICollection<string>>();
        }

        [Fact]
        public void ErrorsShouldTerminateObservables()
        {
            var scheduler = new TestScheduler();

            var cutOffObservable = scheduler.CreateHotObservable(
                OnNext(100, "OK"),
                OnError<string>(300, new ArithmeticException()),
                OnNext(400, "Still good to go"));

            var results = scheduler.Start(() => cutOffObservable);
            var lastMessageKind = results.Messages.Last().Value.Kind;

            lastMessageKind.Should().Be(NotificationKind.OnNext, "MS implements non-standard IObservables");
        } 
    }
}
