using System;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace eFMS.API.Common.Helpers
{
    public class ImageHelper
    {
        public enum ImageFormat
        {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            unknown
        }

        public static ImageFormat GetImageFormat(byte[] bytes)
        {
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };              // PNG
            var tiff = new byte[] { 73, 73, 42 };                  // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };                 // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 };          // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 };         // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpeg;

            return ImageFormat.unknown;
        }
        public static bool CheckIfImageFile(IFormFile file)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return GetImageFormat(fileBytes) != ImageFormat.unknown;
        }
        public static async Task SaveImage(string fileName, string folderName, IFormFile file)
        {
            var physicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\" + folderName, fileName); // lưu vào folder

            using (var bits = new FileStream(physicPath, FileMode.Create))
            {
                await file.CopyToAsync(bits);
            }
        }
        public static void CreateDirectoryImage(string folderName)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\"));
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images"));
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images" + folderName)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\" + folderName));
            }
        }
        public static void CreateDirectoryFile(string folderName, string objectId)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\"));
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName));
            }

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName + "\\files\\")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName + "\\files\\"));
            }
            if(!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName + "\\files\\" + objectId)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName + "\\files\\" + objectId));
            }
        }
        public static async Task SaveFile(string fileName, string folderName, string objectId, IFormFile file)
        {
            var physicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName + "\\files\\" + objectId + "\\", fileName); // lưu vào folder

            using (var bits = new FileStream(physicPath, FileMode.Create))
            {
                await file.CopyToAsync(bits);
            }
        }

        public static async Task<bool> DeleteFile(string fileName, string folderName, string option="files")
        {
          
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + option + "\\" + folderName, fileName);
            if (!System.IO.File.Exists(path)) return false;
            try
            {
                System.IO.File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }


        }
    }
}
