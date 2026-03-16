using System.Collections.Generic;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Data/AllPeople", fileName = "AllPeopleData")]
    public sealed class AllPeopleData : ScriptableObject
    {
        public IReadOnlyList<PersonData> Get() => _people;

        [SerializeField] private List<PersonData> _people;
    }
}