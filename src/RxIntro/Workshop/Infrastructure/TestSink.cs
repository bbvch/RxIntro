namespace Bbv.Rx.Workshop.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class TestSink<T> : IObserver<T>
    {
        private readonly object sync = new object();

        private volatile IImmutableList<T> values = ImmutableList.Create<T>();

        public IEnumerable<T> Values
        {
            get
            {
                lock (this.sync)
                {
                    return this.values;
                }
            }
        }

        public void OnNext(T value)
        {
            lock (this.sync)
            {
                this.values = this.values.Add(value);
            }
        }

        public void OnError(Exception error)
        {
            Assert.Fail(error.Message);
        }

        public void OnCompleted()
        {
        }
    }
}