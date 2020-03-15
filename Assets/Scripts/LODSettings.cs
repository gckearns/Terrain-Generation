using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LODSettings {

    public int vertexIncrement;
    public float maxViewDist;

    public LODSettings(int vertexIncrement, float maxViewDist)
    {
        this.vertexIncrement = vertexIncrement;
        this.maxViewDist = maxViewDist;
    }
}
