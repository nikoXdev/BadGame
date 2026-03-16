using System;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Entities
{
    public enum PersonAction
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
    
    [CreateAssetMenu(menuName = "Data/Person", fileName = "PersonData")]
    public sealed class PersonData : ScriptableObject
    {
        [field: SerializeField] public Sprite NormalSprite  { get; private set; }
        [field: SerializeField] public Sprite OutlineSprite { get; private set; }
        [field: SerializeField] public int Number { get; private set; }
        [field: SerializeField] public PersonAction Action { get; private set; }
        
        public float Apply(float current) => Action switch
        {
            PersonAction.Add => current + Number,
            PersonAction.Subtract => current - Number,
            PersonAction.Multiply => current * Number,
            PersonAction.Divide => Number != 0 ? current / Number : current,
            _ => current
            };
    }
}