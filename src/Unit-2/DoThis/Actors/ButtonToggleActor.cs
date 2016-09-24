using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Akka.Actor;

namespace ChartApp.Actors
{
    public class ButtonToggleActor:UntypedActor
    {
        public class Toggle { }

        private readonly CounterType _counterType;
        private bool _isToggledOn;
        private readonly Button _button;
        private readonly IActorRef _coordinatorActor;

        public ButtonToggleActor(IActorRef coordinatoActor, Button button, CounterType counterType,
            bool isToggledOn = false)
        {
            _button = button;
            _coordinatorActor = coordinatoActor;
            _counterType = counterType;
            _isToggledOn = isToggledOn;
        }

        protected override void OnReceive(object message)
        {
            if (message is Toggle && _isToggledOn)
            {
                _coordinatorActor.Tell(new PerformanceCoorindatorActor.UnWatch(_counterType));
                FlipToggle();
            }
            else if (message is Toggle && !_isToggledOn)
            {
                _coordinatorActor.Tell(new PerformanceCoorindatorActor.Watch(_counterType));
                FlipToggle();
            }
            else
            {
                Unhandled(message);
            }
        }

        private void FlipToggle()
        {
            _isToggledOn = !_isToggledOn;
            _button.Text=string.Format("{0} {1}",_counterType.ToString().ToUpperInvariant(),_isToggledOn?"ON":"OFF")
            ;
        }
    }
}
