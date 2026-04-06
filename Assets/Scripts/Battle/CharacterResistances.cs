using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterResistances : MonoBehaviour
{
    public List<ElementType> strong;
    public List<ElementType> wakness;

    internal int GetResistanceValue(ElementType element)
    {
        if (strong.Contains(element))
            return 50;
        else if (wakness.Contains(element))
            return  -50;
        else return 0;
    }
}
