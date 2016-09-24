using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartApp.Actors
{
    public class GatherMetrics { }

    public class Metric
    {
        public Metric (string series, float counterValue)
        {
            CounterValue = counterValue;
            Series = series;
        }

        public string Series { get; private set; }

        public float CounterValue { get; private set; }
    }

    public enum CounterType
    {
        Cpu,
        Memory,
        Disk
    }

    public class SubscribeCounter
    {
        public SubscribeCounter(CounterType counterType, IActorRef subscriber)
        {
            CounterType = counterType;

            Subscriber = subscriber;
        }

        public CounterType CounterType { get; private set; }

        public IActorRef Subscriber { get; private set; }
    }

    public class UnsubscribeCounter
    {
        public UnsubscribeCounter(CounterType counterType, IActorRef subscriber)
        {
            CounterType = counterType;

            Subscriber = subscriber;
        }

        public CounterType CounterType { get; private set; }

        public IActorRef Subscriber { get; private set; }
    }

}
