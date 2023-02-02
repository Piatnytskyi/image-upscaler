using ImageUpscalerClient.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ImageUpscalerClient.Facades
{
    public static class ImageUpscalerFacade
    {
        public static async Task RunImageUpscaler(
            string imagePath,
            Algorithm algorithm,
            Scale scale,
            string imageOutputPath = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = "dist\\ImageUpscaler.exe";
            startInfo.Arguments = $"\"{imagePath}\" {algorithm.ToString().ToLower()} {(int)scale} ";

            if (algorithm >= Algorithm.EDSR)
                startInfo.Arguments += $"\"{algorithm.ToString().ToUpper()}_{scale}.pb\"";

            Process process;
            using (process = new Process())
            {
                process.StartInfo = startInfo;
                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    throw;
                }
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                    throw new Exception("Failed to upscale.");
            }

            if (!string.IsNullOrEmpty(imageOutputPath))
                File.Move($"upscaled{Path.GetExtension(imagePath)}", imageOutputPath, true);
        }
    }
}
