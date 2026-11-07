using System;

namespace ECS
{
    public class TickDriver
    {
        public int CurrentTick { get; private set; }

        public int TickRate { get; private set; }

        public float TickTime { get; private set; }

        private float _timeSinceTick;

        public Action<int, float> _onTick;
        public Action<int, float> _onPostTick;

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

            if (_timeSinceTick >= TickTime * 8f)
                _timeSinceTick = TickTime * 8f;

            while (_timeSinceTick >= TickTime && _runSim)
            {
                _onTick?.Invoke(CurrentTick, TickTime);
                _onPostTick?.Invoke(CurrentTick, TickTime);

                CurrentTick++;
                _timeSinceTick -= TickTime;
            }
        }

        public void SetToTick(int i)
        {
            CurrentTick = i;
            _timeSinceTick = 0;
        }
    }
}
