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

    // HOW TO USE THIS WORKSHOP
    // Goal: make all test pass by just implementing the missing parts indicated with TODOs
    // The code you should change or add is always right below the TODOs.
    // Try starting at the TODOs and only then read the rest of code. Normally it's not needed
    // to know the details of the test arrangement before you know the task (sometimes even not at all).
    // Try solving first without looking at the test output. Debugging should not be needed.
    // -
    // For looking at live values, LINQPad is a great tool. Open TestBench.linq from the Infrastructure folder.
    // This will download some nuGet packages. From there on you can just paste a 'Fact' into LINQPad,
    // add some 'Dump()' statements, press F5 and enjoy (or hate) the outcome.
    public class Basics : ReactiveTest
    {
        [Fact]
        public void Exercise_ObservableAndObserver()
        {
            // TODO: create an 'Observable' that 'return's an integer
            var observable = Observable.Empty<int>();
            var observer = new TestSink<int>();

            //// TODO: subscribe to the observable

            observer.Values.Single().Should().Be(42);
        }

        [Fact]
        public void Exercise_ObservableAndAction()
        {
            var observable = Observable.Throw<decimal>(new MissingMethodException());
            var errorHappened = false;

            // Instead of having an observer, we can also supply
            // a method to be called when something happens.
            // TODO extend the subscription
            observable.Subscribe(_ => { });

            errorHappened.Should().BeTrue();
        }

        [Fact]
        public void Exercise_FromCollection()
        {
            // Note that this should only be used
            // - for tests
            // - when an external API is reactive-only
            // as hiding the synchronous character of collections is considered bad practice
            var collection = new[] { "Hello", "world!" };

            // TODO: create an observable from the collection
            var observable = collection;

            observable.Should().BeAssignableTo<IObservable<string>>();
        }

        [Fact]
        public void Exercise_ToLazy()
        {
            // Leaving the monad* (the world of observables) is mostly useful
            // when testing or building non-reactive datastructures for passing
            // them on to non-reactive APIs.
            var observable = Observable.Repeat("FooBar");

            // TODO: create an enumerable from the observable
            var lazyEnumerable = observable;

            lazyEnumerable.Should().BeAssignableTo<IEnumerable<string>>();

            // * Freaky detail: observables define a so called monad through the
            // 'Return' and 'SelectMany' operations. Fortunately, using these powerful
            // tools does not require any knowledge thereof.
        }

        [Fact]
        public async void Exercise_ToStrict()
        {
            var observable = Observable.Repeat("FooBar");

            // TODO: create a strict collection from the observable
            // HINT 1:
            // http://reactivex.io/documentation/operators/to.html
            // http://reactivex.io/documentation/operators/take.html
            // HINT 2: did you notice the test's signature?
            var strictCollection = observable;

            strictCollection.Should().BeAssignableTo<ICollection<string>>();
        }

        [Fact]
        public void Note_ErrorsShouldTerminateObservables()
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
