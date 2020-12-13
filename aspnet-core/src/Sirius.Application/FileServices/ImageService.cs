using System;
using System.IO;
using Abp.UI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sirius.FileServices
{
    public class ImageService : IImageService
    {
        public void ResizeImage(Stream fileStream, Stream outputStream)
        {
            try
            {
                using (var image = Image.Load(fileStream))
                {
                    var sizeLimit = 640;
                    var width = (decimal)image.Width;
                    var height = (decimal)image.Height;
                    var longerValue = width;

                    var newWidth = width;
                    var newHeight = height;
                
                    if (width > height)
                    {
                        longerValue = width;
                        if (width > sizeLimit)
                        {
                            var multiplier = width / sizeLimit;
                            newWidth = sizeLimit;
                            newHeight = height / multiplier;
                        }
                    }

                    if (height > width)
                    {
                        longerValue = height;
                        if (height > sizeLimit)
                        {
                            var multiplier = height / sizeLimit;
                            newHeight = sizeLimit;
                            newWidth = width / multiplier;
                        }
                    }
                
                    if (longerValue < sizeLimit)
                    {
                        throw  new UserFriendlyException("Dosya çözünürlüğü minimum 640x480 olmalıdır.");
                    }

                    image.Mutate(x => x
                        .Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.BoxPad,
                            Size = new Size(Convert.ToInt32(newWidth), Convert.ToInt32(newHeight))
                        }).BackgroundColor(new Rgba32(0, 0, 0)));

                    image.SaveAsJpeg(outputStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}