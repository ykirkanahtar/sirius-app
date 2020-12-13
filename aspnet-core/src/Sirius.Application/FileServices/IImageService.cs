using System.IO;

namespace Sirius.FileServices
{
    public interface IImageService
    {
        void ResizeImage(Stream fileStream, Stream outputStream);
    }
}