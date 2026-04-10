using UnityEngine;

[CreateAssetMenu(fileName = "GameString", menuName = "Core/GameString")]
public class GameString : ScriptableObject
{
    [SerializeField] private string m_value;

    public string Value { get => m_value; }
}