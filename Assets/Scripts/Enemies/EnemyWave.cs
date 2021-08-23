using UnityEngine;

[CreateAssetMenu]
public class EnemyWave : ScriptableObject
{
    [SerializeField]EnemySpawnSequence[] spawnSequences =
    {
            new EnemySpawnSequence()
    };
    
    public State Begin() => new State(this);

    [System.Serializable]
    public struct State
    {
        private EnemyWave wave;

        private int index;

        private EnemySpawnSequence.State sequence;

        public State(EnemyWave _wave)
        {
            wave = _wave;
            index = 0;

            Debug.Assert(wave.spawnSequences.Length > 0, "Empty wave !");

            sequence = wave.spawnSequences[0].Begin();
        }

        public float Progress(float _deltaTime)
        {
            _deltaTime = sequence.Progress(_deltaTime);

            while (_deltaTime >= 0.0f)
            {
                if (++index >= wave.spawnSequences.Length)
                {
                    return _deltaTime;
                }

                sequence = wave.spawnSequences[index].Begin();
                _deltaTime = sequence.Progress(_deltaTime);
            }

            return -1.0f;
        }
    }
}