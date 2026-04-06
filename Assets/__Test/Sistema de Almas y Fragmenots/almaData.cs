using UnityEngine;

[CreateAssetMenu(fileName = "AlmaData", menuName = "Core/AlmaData")]
public class AlmaData : ScriptableObject
{
    public enum TipoDrop { Alma, FragmentoDeAlma }

    public TipoDrop tipo;
    public int cantidad = 1;
    public GameObject prefabDrop; // El prefab visual que aparece en el suelo
}
