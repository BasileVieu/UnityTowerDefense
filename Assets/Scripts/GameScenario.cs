using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject
{
    [SerializeField] private EnemyWave[] waves = {};

    [SerializeField] [Range(0, 10)] private int cycles = 1;

    [SerializeField] [Range(0.0f, 1.0f)] private float cycleSpeedUp = 0.5f;
    
    public State Begin() => new State(this);

    [System.Serializable]
    public struct State
    {
        private GameScenario scenario;

        private int index;
        private int cycle;

        private float timeScale;

        private EnemyWave.State wave;

        public State(GameScenario _scenario)
        {
            scenario = _scenario;
            index = 0;
            cycle = 0;
            timeScale = 1.0f;

            Debug.Assert(scenario.waves.Length > 0, "Empty scenario !");

            wave = scenario.waves[0].Begin();
        }

        public bool Progress()
        {
            float deltaTime = wave.Progress(timeScale * Time.deltaTime);

            while (deltaTime >= 0.0f)
            {
                if (++index >= scenario.waves.Length)
                {
                    if (++cycle >= scenario.cycles
                        && scenario.cycles > 0)
                    {
                        return false;
                    }

                    index = 0;

                    timeScale += scenario.cycleSpeedUp;
                }

                wave = scenario.waves[index].Begin();
                deltaTime = wave.Progress(deltaTime);
            }

            return true;
        }
    }
}