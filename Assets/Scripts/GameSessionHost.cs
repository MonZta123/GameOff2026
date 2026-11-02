using ECS;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameSessionHost : MonoBehaviour
    {
        [SerializeField]
        private int stepsPerSecond;

        [SerializeField]
        private int maxSeconds;

        [SerializeField]
        private SnapshotRenderer snapshotRenderer;

        [SerializeField]
        private GameObjectPool pool;
        
        public TickDriver _tickDriver;
        public World _world;

        public Snapshot[] _snapshots;
        public Intent[] _intents;

        private void Awake()
        {
            var ticksTotal = maxSeconds * stepsPerSecond;

            // Show Loadingscreen here

            _world = new World();
            _intents = new Intent[ticksTotal];
            _snapshots = new Snapshot[ticksTotal];
            _tickDriver = new TickDriver(stepsPerSecond);

            _tickDriver._onTick += OnTick;
            _tickDriver._onPostTick += OnPostTick;
            
            pool.Prewarm();

            for (var i = 0; i < ticksTotal; i++)
            {
                var snapshot = new Snapshot();
                snapshot.Initialize();
                _snapshots[i] = snapshot;

                var intent = new Intent();
                _intents[i] = intent;
            }
            
            snapshotRenderer.SetSnapshot(_snapshots[0]);

            // Hide Loadingscreen here
        }

        private long _lastSnapshotTick;

        private void OnDestroy()
        {
            _tickDriver.StopSim();
            _tickDriver._onTick -= OnTick;
            _tickDriver._onPostTick -= OnPostTick;
            _tickDriver = null;
        }

        public void Start()
        {
            _tickDriver.StartSim();
        }

        private void Update()
        {
            WriteInput();
            _tickDriver.Update(Time.deltaTime);
        }

        private void WriteInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                _spawnDown = true;

            if (Input.GetKeyDown(KeyCode.Q))
                _despawn1Down = true;

            if (Input.GetKeyDown(KeyCode.W))
                _despawn2Down = true;

            if (Input.GetKeyDown(KeyCode.E))
                _despawn3Down = true;

            if (Input.GetKeyDown(KeyCode.R))
                _despawn4Down = true;
        }

        public Intent BuildIntent()
        {
            var intent = new Intent
            {
                spawnPlayer = _spawnDown,
                despawnPlayer1 = _despawn1Down,
                despawnPlayer2 = _despawn2Down,
                despawnPlayer3 = _despawn3Down,
                despawnPlayer4 = _despawn4Down
            };
            
            _spawnDown = false;
            _despawn1Down = false;
            _despawn2Down = false;
            _despawn3Down = false;
            _despawn4Down = false;

            return intent;
        }

        private bool _spawnDown;
        private bool _despawn1Down;
        private bool _despawn2Down;
        private bool _despawn3Down;
        private bool _despawn4Down;

        private void OnTick(long tick, float deltaTime)
        {
            if (tick == _snapshots.Length)
                return;

            var buildIntent = BuildIntent();

            _world.SetState(_snapshots[tick]);
            _world.DoStep(buildIntent, tick, deltaTime);

            _intents[tick] = buildIntent;
            
            _snapshots[tick + 1].BuildFrom(_world.GetSnapshot());
        }

        private void OnPostTick(long tick, float deltaTime)
        {
            snapshotRenderer.SetSnapshot(_snapshots[_tickDriver.CurrentTick + 1]);

            if (tick == _snapshots.Length - 1)
            {
                _tickDriver.StopSim();
            }
        }

        private void OnGUI()
        {
            GUILayout.Label($"Tick: {_tickDriver.CurrentTick}");
            GUILayout.Label($"Space: {_spawnDown}");
        }

        public void Rewind()
        {
            _tickDriver.SetToTick(20);
        }
    }
}
