using ImageUpscalerClient.Commands;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;

namespace ImageUpscalerClient.ViewModels
{
    internal class ImageUpscalerViewModel : AbstractViewModel
    {


        private string _filenameInput = string.Empty;

        public string FilenameInput
        {
            get => _filenameInput;
            set
            {
                if (_filenameInput.Equals(value))
                {
                    return;
                }
                _filenameInput = value;
                RaisePropertyChanged(nameof(FilenameInput));
            }
        }

        private string _filenameTemporaryOutput = string.Empty;

        public string FilenameTemporaryOutput
        {
            get => _filenameTemporaryOutput;
            private set
            {
                if (_filenameTemporaryOutput.Equals(value))
                {
                    return;
                }
                _filenameTemporaryOutput = value;
                RaisePropertyChanged(nameof(FilenameTemporaryOutput));
            }
        }

        private string _status = string.Empty;

        public string Status
        {
            get => _status;
            set
            {
                if (_status.Equals(value))
                {
                    return;
                }
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        private string _output = string.Empty;

        public string Output
        {
            get => _output;
            set
            {
                if (_output.Equals(value))
                {
                    return;
                }
                _output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        private bool _isInProgress;

        public bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                if (_isInProgress.Equals(value))
                {
                    return;
                }
                _isInProgress = value;
                RaisePropertyChanged(nameof(IsInProgress));
            }
        }

        public RelayCommand ChooseFileCommand { get; set; }
        public RelayCommand SaveFileCommand { get; set; }
        public AsyncCommand RunImageUpscaleCommand { get; set; }

        public ImageUpscalerViewModel()
        {
            ChooseFileCommand = new RelayCommand(o => ChooseFile(), c => CanChooseFile());
            SaveFileCommand = new RelayCommand(o => SaveFile(), c => CanSaveFile());
            RunImageUpscaleCommand = new AsyncCommand(o => RunImageUpscale(), c => CanRunImageUpscale());

        }

        private bool CanSaveFile()
        {
            return (!IsInProgress
                || !string.IsNullOrEmpty(FilenameInput))
                && File.Exists(FilenameTemporaryOutput);
        }

        private void SaveFile()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save File...";
            saveFileDialog.FileName = Path.GetFileName(FilenameTemporaryOutput);

            if (saveFileDialog.ShowDialog() == true
                && FilenameTemporaryOutput != saveFileDialog.FileName)
            {
                File.Move(FilenameTemporaryOutput, saveFileDialog.FileName);
                File.Delete(FilenameTemporaryOutput);

                Status = "Upscaled image saved:";
            }
        }

        private bool CanChooseFile()
        {
            return !IsInProgress;
        }

        private void ChooseFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose File...";
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                FilenameInput = openFileDialog.FileName;
                FilenameTemporaryOutput = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "upscaled" + Path.GetExtension(FilenameInput));
                File.Delete(FilenameTemporaryOutput);

                Status = "Chosen file:";
                Output = FilenameInput;
            }
        }

        private bool CanRunImageUpscale()
        {
            return !IsInProgress
                || !string.IsNullOrEmpty(FilenameInput);
        }

        private async Task RunImageUpscale()
        {
            IsInProgress = true;
            Status = "Upscaling...:";

            await Task.Run(() =>
            {
                                

                Status = "Image was upscaled!";
                IsInProgress = false;
            });
        }
    }
}
