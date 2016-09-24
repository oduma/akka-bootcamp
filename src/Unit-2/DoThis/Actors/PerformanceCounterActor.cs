using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartApp.Actors
{
    public class PerformanceCounterActor:UntypedActor
    {
        private readonly string _seriesName;

        private readonly Func<PerformanceCounter> _performanceCounterGenerator;
        private PerformanceCounter _counter;

        private readonly HashSet<IActorRef> _subscriptions;
        private readonly ICancelable _cancelPublishing;

        public PerformanceCounterActor(string seriesName, Func<PerformanceCounter> performanceCounterGenerator)
        {
            _seriesName = seriesName;
            _performanceCounterGenerator = performanceCounterGenerator;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable(Context.System.Scheduler);
        }

        protected override void PreStart()
        {
            _counter = _performanceCounterGenerator();
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(250),TimeSpan.FromMilliseconds(250),Self, new GatherMetrics(), Self, _cancelPublishing);
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel(false);
                _counter.Dispose();
            }
            catch
            {

            }
            finally
            {
                base.PostStop();
            }
        }

        protected override void OnReceive(object message)
        {
            if(message is GatherMetrics)
            {
                var metric = new Metric(_seriesName, _counter.NextValue());
                foreach (var sub in _subscriptions)
                    sub.Tell(metric);
            }
            else if(message is SubscribeCounter)
            {
                var subscriptionAsked = message as SubscribeCounter;
                _subscriptions.Add(subscriptionAsked.Subscriber);
            }
            else if(message is UnsubscribeCounter)
            {
                var unsubscriptionAsked = message as UnsubscribeCounter;
                _subscriptions.Remove(unsubscriptionAsked.Subscriber);
            }
        }
    }
}
