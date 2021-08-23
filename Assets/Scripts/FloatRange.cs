using UnityEngine;

[System.Serializable]
public class FloatRange
{
    [SerializeField] private float min;
    [SerializeField] private float max;

    public float Min => min;
    public float Max => max;

    public float RandomValueInRange => Random.Range(min, max);

    public FloatRange(float _value)
    {
        min = max = _value;
    }

    public FloatRange(float _min, float _max)
    {
        min = _min;
        max = _max < _min ? _min : _max;
    }
}