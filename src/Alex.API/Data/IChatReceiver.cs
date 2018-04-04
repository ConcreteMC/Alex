using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Data
{
    public interface IChatReceiver
    {
	    void Receive(string message);
    }
}
