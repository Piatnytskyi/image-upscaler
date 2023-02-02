using System;

namespace ImageUpscalerClient
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
