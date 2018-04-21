using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Services
{
    public interface IStorageSystem
    {
        bool TryWrite<T>(string key, T value);
        bool TryRead<T>(string key, out T value);
    }
}
