using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.World
{
    public enum LoadingState
    {
		ConnectingToServer,
		LoadingChunks,
		GeneratingVertices,
		Spawning
    }
}
