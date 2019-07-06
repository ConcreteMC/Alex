using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Catel.Fody;
using Catel.MVVM;
using Orchestra.Models;
using Orchestra.Services;

namespace Alex.GuiDebugger.ViewModels
{
    public class StatusBarViewModel : ViewModelBase
    {
        private readonly IAboutInfoService _aboutInfoService;

        [Model]
        [Expose(nameof(Orchestra.Models.AboutInfo.ProductName))]
        [Expose(nameof(Orchestra.Models.AboutInfo.Version))]
        public AboutInfo AboutInfo { get; private set; }

        public StatusBarViewModel(IAboutInfoService aboutInfoService)
        {
            _aboutInfoService = aboutInfoService;
        }


        protected override async Task InitializeAsync()
        {
            AboutInfo = _aboutInfoService.GetAboutInfo();
        }
    }
}
