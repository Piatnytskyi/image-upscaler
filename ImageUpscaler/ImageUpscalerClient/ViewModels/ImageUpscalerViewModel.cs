using ImageUpscalerClient.Commands;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ImageUpscalerClient.Enums;
using System.Collections.Generic;
using ImageUpscalerClient.Facades;
using System.Windows;
using System;
using System.Linq;
using System.Diagnostics;

namespace ImageUpscalerClient.ViewModels
{
    internal class ImageUpscalerViewModel : AbstractViewModel
    {
        private static Dictionary<Scale, Algorithm[]> _scalesToAlgorithms =
            new Dictionary<Scale, Algorithm[]>
            {
                {
                    Scale.x2, new[]
                    {
                        Algorithm.Bicubic,
                        Algorithm.Bilinear,
                        Algorithm.EDSR,
                        Algorithm.ESPCN,
                        Algorithm.FSRCNN,
                        Algorithm.LAPSRN
                    }
                },
                {
                    Scale.x3, new[]
                    {
                        Algorithm.Bicubic,
                        Algorithm.Bilinear,
                        Algorithm.EDSR,
                        Algorithm.ESPCN,
                        Algorithm.FSRCNN
                    }
                },
                {
                    Scale.x4, new[]
                    {
                        Algorithm.Bicubic,
                        Algorithm.Bilinear,
                        Algorithm.EDSR,
                        Algorithm.ESPCN,
                        Algorithm.FSRCNN,
                        Algorithm.LAPSRN
                    }
                },
                {
                    Scale.x8, new[]
                    {
                        Algorithm.LAPSRN
                    }
                }
            };

        private ObservableCollection<Scale> _scales =
            new ObservableCollection<Scale>(Enum.GetValues(typeof(Scale)).Cast<Scale>());

        public ObservableCollection<Scale> Scales
        {
            get => _scales;
            private set
            {
                if (_scales.Equals(value))
                {
                    return;
                }
                _scales = value;
                RaisePropertyChanged(nameof(Scales));
            }
        }

        private Scale _scale = Scale.x2;

        public Scale Scale
        {
            get => _scale;
            set
            {
                if (_scale.Equals(value))
                {
                    return;
                }
                _scale = value;
                RaisePropertyChanged(nameof(Scale));
            }
        }

        private ObservableCollection<Algorithm> _algorithms =
            new ObservableCollection<Algorithm>(Enum.GetValues(typeof(Algorithm)).Cast<Algorithm>());

        public ObservableCollection<Algorithm> Algorithms
        {
            get => _algorithms;
            private set
            {
                if (_algorithms.Equals(value))
                {
                    return;
                }
                _algorithms = value;
                RaisePropertyChanged(nameof(Algorithms));
            }
        }

        private Algorithm _algorithm = Algorithm.Bilinear;

        public Algorithm Algorithm
        {
            get => _algorithm;
            set
            {
                if (_algorithm.Equals(value))
                {
                    return;
                }
                _algorithm = value;
                RaisePropertyChanged(nameof(Algorithm));
            }
        }

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
        public RelayCommand ScaleChangedCommand { get; set; }
        public RelayCommand OpenOutputCommand { get; set; }

        public ImageUpscalerViewModel()
        {
            ChooseFileCommand = new RelayCommand(o => ChooseFile(), c => CanChooseFile());
            SaveFileCommand = new RelayCommand(o => SaveFile(), c => CanSaveFile());
            RunImageUpscaleCommand = new AsyncCommand(o => RunImageUpscale(), c => CanRunImageUpscale());
            ScaleChangedCommand = new RelayCommand(o => ScaleChanged());
            OpenOutputCommand = new RelayCommand(o => OpenOutput(), c => CanOpenOutput());
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
                    Path.GetDirectoryName(FilenameInput)!,
                    "upscaled-" + Path.GetFileName(FilenameInput));
                File.Delete(FilenameTemporaryOutput);

                Status = "Chosen file:";
                Output = FilenameInput;
            }
        }

        private bool CanRunImageUpscale()
        {
            return !IsInProgress
                && !string.IsNullOrEmpty(FilenameInput);
        }

        private async Task RunImageUpscale()
        {
            IsInProgress = true;
            Status = "Upscaling...:";

            await Task.Run(async () =>
            {
                try
                {
                    await ImageUpscalerFacade.RunImageUpscaler(
                        FilenameInput,
                        Algorithm,
                        Scale,
                        FilenameTemporaryOutput);
                }
                catch (Exception ex)
                {
                    Status = ex.Message;
                    IsInProgress = false;

                    MessageBox.Show(
                        "Image Upscaler failed to execute!",
                        "Image Upscaler",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.Yes);

                    return;
                }

                Status = "Image was upscaled!";
                Output = FilenameTemporaryOutput;
                IsInProgress = false;
            });
        }

        private void ScaleChanged()
        {
            Algorithms = new ObservableCollection<Algorithm>(_scalesToAlgorithms[Scale]);
            Algorithm = _scalesToAlgorithms[Scale].First();
        }

        private bool CanOpenOutput()
        {
            return !IsInProgress && File.Exists(Output);
        }

        private void OpenOutput()
        {
            using (Process explorer = new Process())
            {
                explorer.StartInfo.FileName = "explorer";
                explorer.StartInfo.Arguments = "\"" + Output + "\"";
                explorer.Start();
            }
        }
    }
}
