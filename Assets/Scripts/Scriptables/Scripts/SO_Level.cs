using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelObject", menuName = "ScriptableObjects/Levels")]
public class SO_Level : ScriptableObject
{
    public int xSize;
    public int ySize;

    public List<Vector3> CollectablePosition;
}
