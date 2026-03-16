#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace LitePinBoard.Sources.Editor.Data
{
    public sealed class PinBoardData : ScriptableObject
    {
        public List<PinBoardItem> items = new List<PinBoardItem>();
    }
}

#endif