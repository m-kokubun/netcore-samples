using MLNetWpfApp.Inputs;
using MLNetWpfApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MLNetWpfApp.ViewModels
{
    public class MainViewModel : NotifyObject
    {
        private readonly MobileNetContext context;
        private string imagePath;
        private string result;

        public string ImagePath
        {
            get { return imagePath; }
            set { SetValue(ref imagePath, value); }
        }

        public string Result
        {
            get { return result; }
            set { SetValue(ref result, value); }
        }

        public RelayCommand Infer { get; private set; }

        public RelayCommand Clear { get; private set; }

        public MainViewModel()
        {
            ImagePath = string.Empty;
            Result = "?";
            Infer = new RelayCommand(async () => await InferTypeAsync());
            Clear = new RelayCommand(() => ClearImage());
            context = new MobileNetContext();
        }

        private async Task InferTypeAsync()
        {
            if (!CanDetect())
            {
                return;
            }

            Result = await context.InferAsync(ImagePath);
        }

        private void ClearImage()
        {
            ImagePath = string.Empty;
            Result = "?";
        }

        private bool CanDetect()
            => !string.IsNullOrEmpty(ImagePath) && System.IO.File.Exists(ImagePath);
    }
}
