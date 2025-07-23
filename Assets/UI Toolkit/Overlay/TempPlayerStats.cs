using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class TempPlayerStats 
{
    public float MaxValue = 100;
    [SerializeField]
    private float m_value = 50;
    public float Value
    {
        get => m_value;
        set
        {
            m_value = value;
            if (m_value < 0 || m_value > MaxValue)
            {
                m_value = Math.Clamp(m_value, 0, MaxValue);
            }
        }
    }
    [CreateProperty]
    public StyleLength ValuePersentage
    {
        get
        {
            if (Value < 0 || Value > MaxValue)
            {
                Value = Math.Clamp(Value, 0, MaxValue);
            }
            float persentage = (Value / MaxValue) * 100;
            var length = new Length(persentage, LengthUnit.Percent);
            return new StyleLength(length);
        }
    }
}
