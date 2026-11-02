using System;

namespace ECS
{
    public class TickDriver
    {
        public long CurrentTick { get; private set; }

        public int TickRate { get; private set; }

        public float TickTime { get; private set; }

        private float _timeSinceTick;

        public Action<long, float> _onTick;
        public Action<long, float> _onPostTick;

        public TickDriver(int tickRate)
        {
            TickRate = tickRate;
            TickTime = 1 / (float)tickRate;
        }

        private bool _runSim = false;

        public void StartSim()
        {
            _runSim = true;
        }

        public void StopSim()
        {
            _runSim = false;
        }

        public void Update(float deltaTime)
        {
            if (!_runSim) return;

            _timeSinceTick += deltaTime;

            if (_timeSinceTick >= TickTime)
            {
                _onTick?.Invoke(CurrentTick, TickTime);
                _onPostTick?.Invoke(CurrentTick, TickTime);
                
                CurrentTick++;
                _timeSinceTick = 0;
            }
        }

        public void SetToTick(uint i)
        {
            CurrentTick = i;
        }
    }
}
