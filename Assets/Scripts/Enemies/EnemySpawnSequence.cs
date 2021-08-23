using UnityEngine;

[System.Serializable]
public class EnemySpawnSequence
{
    [SerializeField] private EnemyFactory factory;

    [SerializeField] private EnemyType type = EnemyType.Medium;

    [SerializeField] [Range(1, 100)] private int amount = 1;

    [SerializeField] [Range(0.1f, 10.0f)] private float cooldown = 1.0f;

    public State Begin() => new State(this);

    [System.Serializable]
    public struct State
    {
        private EnemySpawnSequence sequence;

        private int count;

        private float cooldown;

        public State(EnemySpawnSequence _sequence)
        {
            sequence = _sequence;
            count = 0;
            cooldown = sequence.cooldown;
        }

        public float Progress(float _deltaTime)
        {
            cooldown += _deltaTime;

            while (cooldown >= sequence.cooldown)
            {
                cooldown -= sequence.cooldown;

                if (count >= sequence.amount)
                {
                    return cooldown;
                }

                count += 1;

                Game.SpawnEnemy(sequence.factory, sequence.type);
            }

            return -1.0f;
        }
    }
}