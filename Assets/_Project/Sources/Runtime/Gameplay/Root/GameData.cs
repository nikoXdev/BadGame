using UnityEngine;

namespace Sources.Runtime.Gameplay.Root
{
    [CreateAssetMenu(menuName = "Data/Game", fileName = "GameData")]
    public sealed class GameData : ScriptableObject
    {
        [field: SerializeField] public int MinPeople { get; private set; }
        [field: SerializeField] public int MaxPeople { get; private set; }
        [field: SerializeField] public int SlotsCount { get; private set; }
    }
}