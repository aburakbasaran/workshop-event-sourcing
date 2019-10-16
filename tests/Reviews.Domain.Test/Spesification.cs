using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Reviews.Core;
using Xunit.Abstractions;

namespace Reviews.Domain.Test
{
    public abstract class Spesification<TAggregate, TCommand>
        where TAggregate : Aggregate, new()
    {

        public Spesification(ITestOutputHelper outputHelper)
        {
            History = Given();
            Command = When();

            var sut = new TAggregate();
            var store = new SpecificationAggregateStore(sut);
            
            try
            {
                sut.Load(History);
                GetHandler(store)(Command).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                CaughtException = e;
            }
            
            RaisedEvents = store.RaisedEvents;
            
            if (CaughtException!=null)
                outputHelper.WriteLine($"Error : {CaughtException.ToString()}");

            Print(outputHelper);

        }

        public object[] RaisedEvents { get; }
        
        public object[] History { get; private set; } 
        public abstract object[] Given();
        public abstract Func<TCommand, Task> GetHandler(SpecificationAggregateStore store);
        
        public TCommand Command { get; }
        public abstract TCommand When();
        
        public Exception CaughtException { get; }

        private void Print(ITestOutputHelper outputHelper)
        {
            outputHelper.WriteLine("Scenario: " + GetType().Name.Replace("_"," "));
            outputHelper.WriteLine("");

            if (History.Length > 0)
            {
                outputHelper.WriteLine("Given");
                foreach (var entry in History)
                {
                    outputHelper.WriteLine($"    {entry}"); 
                } 
            }
            
            outputHelper.WriteLine("When");
            outputHelper.WriteLine($"    {Command}");
          
            outputHelper.WriteLine("Then");
            foreach (var e in RaisedEvents)
            {
                outputHelper.WriteLine($"    {e}");
            }
        }
    }
}