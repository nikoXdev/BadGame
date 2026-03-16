using System;

namespace Sources.Runtime.Core.MVP.View
{
    public interface ISwitchable
    {
        void Toggle();
        void Show();
        void Hide();
    }
}