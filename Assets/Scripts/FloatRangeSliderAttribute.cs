using UnityEngine;

public class FloatRangeSliderAttribute : PropertyAttribute
{
    public float Min { get; private set; }
    public float Max { get; private set; }

    public FloatRangeSliderAttribute(float _min, float _max)
    {
        Min = _min;
        Max = _max < _min ? _min : _max;
    }
}