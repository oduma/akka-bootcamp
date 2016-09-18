using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail
{
    public class TailActor:UntypedActor
    {
        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; private set; }
        }

        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }

            public string FileName { get; private set; }

            public string Reason { get; private set; }
        }

        public class InitialRead
        {
            public InitialRead( string fileName,string text)
            {
                FileName = fileName;
                Text = text;
            }

            public string FileName { get; private set; }

            public string Text { get; private set; }
        }

        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private FileObserver _fileObserver;
        private Stream _fileStream;
        private StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;
        }

        protected override void PreStart()
        {
            _fileObserver = new FileObserver(Self, Path.GetFullPath(_filePath));
            _fileObserver.Start();

            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void PostStop()
        {
            _fileObserver.Dispose();
            _fileObserver = null;
            _fileStreamReader.Close();
            _fileStream.Dispose();
            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            if(message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();
                if(!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }
            else if(message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell(string.Format("Tail error: {0}", fe.Reason));
            }
            else if (message is InitialRead)
            {
                var ir =message as InitialRead;
                _reporterActor.Tell(ir.Text);
            }
        }
    }
}
