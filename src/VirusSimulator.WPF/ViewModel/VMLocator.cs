using System;
using System.Collections.Generic;
using System.Text;

namespace VirusSimulator.WPF.ViewModel
{
    public class VMLocator
    {
        public MainViewModel Main { get; private set; } = new MainViewModel();
    }
}
