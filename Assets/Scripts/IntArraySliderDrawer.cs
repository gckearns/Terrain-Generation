using UnityEngine;

public class IntArraySlider : PropertyAttribute {

    public readonly int[] array;

    public IntArraySlider(int[] array)
    {
        this.array = array;
    }
}
