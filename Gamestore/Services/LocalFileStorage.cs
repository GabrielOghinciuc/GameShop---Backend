using Microsoft.AspNetCore.Http;

namespace Gamestore.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileStorage(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> Store(string containerName, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        string folder = Path.Combine(_env.WebRootPath, containerName);

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string path = Path.Combine(folder, fileName);
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            await File.WriteAllBytesAsync(path, ms.ToArray());
        }

        var currentUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
        var pathForDb = Path.Combine(currentUrl, containerName, fileName).Replace("\\", "/");

        return pathForDb;
    }

    public async Task<string> Edit(string path, string containerName, IFormFile file)
    {
        if (!string.IsNullOrEmpty(path))
        {
            await Delete(path, containerName);
        }

        return await Store(containerName, file);
    }

    public Task Delete(string path, string containerName)
    {
        if (string.IsNullOrEmpty(path))
        {
            return Task.CompletedTask;
        }

        var fileName = Path.GetFileName(path);
        var fileDirectory = Path.Combine(_env.WebRootPath, containerName, fileName);

        if (File.Exists(fileDirectory))
        {
            File.Delete(fileDirectory);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> Get(string containerName, string path)
    {
        var filePath = Path.Combine(_env.WebRootPath, containerName, path);
        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream>(null);
        }

        return Task.FromResult<Stream>(File.OpenRead(filePath));
    }
}
