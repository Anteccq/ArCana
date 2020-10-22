using System;
using System.Collections.Generic;
using System.Text;

namespace ArCana.Network.Messages
{
    public interface IMessage
    {
        Message ToMessage();
    }
}
