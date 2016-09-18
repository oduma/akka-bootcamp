using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail
{
    public class FileValidatorActor:UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;
        
        public FileValidatorActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                _consoleWriterActor.Tell(new NullInputError("Input was blank. Please try again.\n"));
                Sender.Tell(new ContinueProcessing());
            }
            else
            {
                var valid = IsFileUri(msg);
                if (valid)
                {
                    _consoleWriterActor.Tell(new InputSuccess(string.Format("Starting processing for {0}", msg)));
                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    _consoleWriterActor.Tell(new ValidationError(string.Format("{0} is not an existing URI on the disk",msg)));
                    Sender.Tell(new ContinueProcessing());
                }
            }
        }
        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }

    }
}
