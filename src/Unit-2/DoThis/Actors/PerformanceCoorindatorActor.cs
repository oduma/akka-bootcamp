using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartApp.Actors
{
    public class PerformanceCoorindatorActor:ReceiveActor
    {
        public class Watch
        {
            public CounterType Counter { get; private set; }

            public Watch(CounterType counter)
            {
                Counter = counter;
            }
        }

        public class UnWatch
        {
            public CounterType Counter { get; private set; }

            public UnWatch(CounterType counter)
            {
                Counter = counter;
            }

        }

        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators =
            new Dictionary<CounterType, Func<PerformanceCounter>>()
            {
                {CounterType.Cpu, ()=> new PerformanceCounter("Processor", "% Processor Time","_Total", true)},
                {CounterType.Memory, ()=> new PerformanceCounter("Memory", "% Committed Bytes in Use", true)},
                {CounterType.Disk, ()=> new PerformanceCounter("LogicalDisk", "% Disk Time","_Total", true)},

            };
        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries =
            new Dictionary<CounterType, Func<Series>>()
            {
                {CounterType.Cpu,()=> new Series(CounterType.Cpu.ToString())
                    {
                        ChartType=SeriesChartType.SplineArea,
                        Color=Color.DarkGreen
                    }
                },
                {CounterType.Memory,()=> new Series(CounterType.Memory.ToString())
                    {
                        ChartType=SeriesChartType.SplineArea,
                        Color=Color.MediumBlue
                    }
                },
                {CounterType.Disk,()=> new Series(CounterType.Disk.ToString())
                    {
                        ChartType=SeriesChartType.SplineArea,
                        Color=Color.DarkRed
                    }
                },

            };
        private Dictionary<CounterType, IActorRef> _counterActors;

        private IActorRef _chartingActor;

        public PerformanceCoorindatorActor(IActorRef chartingActor):
            this(chartingActor, new Dictionary<CounterType, IActorRef>())
        {

        }

        public PerformanceCoorindatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> counterActors)
        {
            _chartingActor = chartingActor;
            _counterActors = counterActors;

            Receive<Watch>(watch =>
                {
                    if(!_counterActors.ContainsKey(watch.Counter))
                    {
                        var counterActor = Context.ActorOf(Props.Create(() => new PerformanceCounterActor(watch.Counter.ToString(), CounterGenerators[watch.Counter])));
                        _counterActors[watch.Counter] = counterActor;
                    }
                    _chartingActor.Tell(new ChartingActor.AddSeries(
                        CounterSeries[watch.Counter]()));
                    _counterActors[watch.Counter].Tell(new SubscribeCounter(watch.Counter, _chartingActor));
                }
                );
            Receive<UnWatch>(unWatch=>
                {
                    if(!_counterActors.ContainsKey(unWatch.Counter))
                    {
                        return;
                    }
                    _counterActors[unWatch.Counter].Tell(new UnsubscribeCounter(unWatch.Counter, _chartingActor));
                    _chartingActor.Tell(new ChartingActor.RemoveSeries(unWatch.Counter.ToString()));
                }
                );
        }
    }
}
