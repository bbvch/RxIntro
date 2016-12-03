<Query Kind="Program">
  <NuGetReference>FluentAssertions</NuGetReference>
  <NuGetReference>Microsoft.Reactive.Testing</NuGetReference>
  <NuGetReference>System.Collections.Immutable</NuGetReference>
  <NuGetReference>xunit</NuGetReference>
  <Namespace>FluentAssertions</Namespace>
  <Namespace>Microsoft.Reactive.Testing</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>Xunit</Namespace>
</Query>

public class TestBench : ReactiveTest
{
	// paste your 'Fact' here
}

void Main()
{
	var testInstance = new TestBench();
	foreach(var test in typeof(TestBench).GetMethods().Where(m => m.GetCustomAttribute<FactAttribute>(false) != null))
	{
		test.Invoke(testInstance, new object[] {});
	}
}

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
       Assert.True(false, error.Message);
   }

   public void OnCompleted()
   {
   }
}