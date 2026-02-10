using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IslandPostPOS.ViewModels
{
    public partial class LoadingViewModel : ObservableObject
    {
        [ObservableProperty]
        private string statusMessage = "Starting...";

        [ObservableProperty]
        private double progressValue = 0;

        [ObservableProperty]
        private bool isLoading = true; // controls ProgressRing
    }
}
