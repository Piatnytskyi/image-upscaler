using ImageUpscalerClient.Enums;
using System.Threading.Tasks;

namespace ImageUpscalerClient.Facades
{
    public class ImageUpscalerFacade
    {
        public async Task<int> RunImageUpscaler(
            string imagePath,
            Algorithm algorithm,
            Scale scale,
            string modelPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "python.exe";
            startInfo.Arguments = $"ImageUpscaler.py \"{imagePath}\" {algorithm.ToString().ToLower()} {scale} \"{modelPath}\"";

            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode;
        }
    }
}
