using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelObject", menuName = "ScriptableObjects/Levels")]
public class SO_Level : ScriptableObject
{
    public int xSize;
    public int ySize;

    public List<Vector3> CollectablePosition;

    // Barreiras verticais (posição centralizada na borda esquerda ou direita da tile)
    public List<Vector3> VerticalBarriers;

    // Barreiras horizontais (posição centralizada na borda superior ou inferior da tile)
    public List<Vector3> HorizontalBarriers;
}
