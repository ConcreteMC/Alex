using System;
using System.Collections.Generic;
using System.Text;
using Orchestra.Models;
using Orchestra.Services;

namespace Alex.GuiDebugger.Services
{
    internal class AboutInfoService : IAboutInfoService
    {
        public AboutInfo GetAboutInfo()
        {
            var aboutInfo = new AboutInfo(new Uri("pack://application:,,,/Resources/Images/Kennyvv_Logo.png", UriKind.RelativeOrAbsolute),
                                          uriInfo: new UriInfo("https://github.com/kennyvv/Alex", "GitHub Repository"));

            return aboutInfo;
        }
    }
}
