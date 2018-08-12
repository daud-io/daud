namespace Game.Engine.Core
{
    using Game.Engine.Core.Actors.Bots;
    using Game.Engine.Networking;
    using Game.Models;
    using Game.Models.Messages;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class World : IDisposable
    {
        public long Time { get; private set; } = 0;

        public Hook Hook { get; set; } = null;

        private Timer Heartbeat = null;

        private readonly List<IDisposable> Disposables = new List<IDisposable>();

        public List<ProjectedBody> Bodies = new List<ProjectedBody>();
        public List<IActor> Actors = new List<IActor>();

        public World()
        {
            Hook = Hook.Default;
            
            InitializeStepTimer();

            var tender = new RobotTender();
            tender.Init(this);

        }

        public void Step()
        {
            lock (this.Bodies)
            {
                Time = DateTime.Now.Ticks / 10000;

                foreach (var body in Bodies)
                    body.Project(Time);

                foreach (var actor in Actors.ToArray())
                    actor.Step();

                foreach (var body in Bodies)
                    if (body.IsDirty)
                    {
                        body.DefinitionTime = this.Time;
                        body.OriginalPosition = body.Position;
                        body.IsDirty = false;
                    }
                }
        }

        private void InitializeStepTimer()
        {
            Heartbeat = new Timer((state) =>
            {
                Step();
                ConnectionHeartbeat.Step();

            }, null, 0, Hook.StepTime);
            Disposables.Add(Heartbeat);
        }

        private long _id = 0;
        public long NextID()
        {
            return _id++;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                    foreach (var d in Disposables)
                        d.Dispose();
            disposedValue = true;
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
