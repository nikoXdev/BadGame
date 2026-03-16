using Sources.Runtime.Gameplay.Entities;
using UnityEngine.EventSystems;

namespace Sources.Runtime.Features.PersonDragger
{
    public interface IPersonDragger : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        void BindToSlot(Slot slot);
    }
}