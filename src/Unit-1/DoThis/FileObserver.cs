﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail
{
    public class FileObserver: IDisposable
    {
        private readonly IActorRef _tailActor;
        private readonly string _absoluteFilePath;
        private FileSystemWatcher _watcher;
        private readonly string _fileDir;
        private readonly string _fileNameOnly;

        public FileObserver(IActorRef tailActor, string absolutePath)
        {
            _tailActor = tailActor;
            _absoluteFilePath = absolutePath;
            _fileDir = Path.GetDirectoryName(_absoluteFilePath);
            _fileNameOnly = Path.GetFileName(_absoluteFilePath);
        }

        public void Start()
        {
            _watcher = new FileSystemWatcher(_fileDir, _fileNameOnly);

            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            _watcher.Changed += OnFileChanged;
            _watcher.Error += OnFileError;

            _watcher.EnableRaisingEvents = true;
        }

        private void OnFileError(object sender, ErrorEventArgs e)
        {
            _tailActor.Tell(new TailActor.FileError(_fileNameOnly, e.GetException().Message), ActorRefs.NoSender);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if(e.ChangeType==WatcherChangeTypes.Changed)
            {
                _tailActor.Tell(new TailActor.FileWrite(e.Name),ActorRefs.NoSender);
            }
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
