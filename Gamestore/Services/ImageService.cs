using Microsoft.AspNetCore.Hosting;

namespace Gamestore.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _environment;
        private const string ImageDirectory = "games";

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("No image file provided");
            }

            // Ensure the file is an image
            if (!imageFile.ContentType.StartsWith("image/"))
            {
                throw new ArgumentException("File is not an image");
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var imagePath = Path.Combine(_environment.WebRootPath, "games", fileName);

            // Save the file
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Return the relative path
            return $"/games/{fileName}";
        }

        public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fileName = Path.GetFileName(imagePath);
            var fullPath = Path.Combine(_environment.WebRootPath, "games", fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
