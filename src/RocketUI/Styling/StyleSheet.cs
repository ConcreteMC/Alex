using System;
using System.Collections.Generic;
using System.Text;
using RocketUI.IO.Serialization;

namespace RocketUI.Styling
{
    public class StyleSheet
    {

        public IReadOnlyCollection<StyledProperty> Properties { get; } = new List<StyledProperty>();

        public StyleSheet()
        {

        }

        
    }
}
