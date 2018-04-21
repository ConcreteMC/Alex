using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Data.Options;
using Alex.API.Services;

namespace Alex.Services
{
    public class OptionsProvider : IOptionsProvider
    {
        public AlexOptions AlexOptions { get; }

        public SoundOptions SoundOptions { get; }

        public OptionsProvider()
        {
            AlexOptions = new AlexOptions();
            SoundOptions = new SoundOptions();
        }
    }
}
