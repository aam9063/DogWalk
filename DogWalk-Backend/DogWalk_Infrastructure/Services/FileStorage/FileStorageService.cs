using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace DogWalk_Infrastructure.Services.FileStorage
{
    public class FileStorageService
    {
        private readonly FileStorageOptions _options;
        
        public FileStorageService(IOptions<FileStorageOptions> options)
        {
            _options = options.Value;
            
            // Asegurar que el directorio base existe
            if (!Directory.Exists(_options.BasePath))
            {
                Directory.CreateDirectory(_options.BasePath);
            }
        }
        
        public async Task<string> SaveFileAsync(IFormFile file, string subDirectory = "")
        {
            ValidateFile(file);
            
            // Crear directorio si no existe
            string directory = Path.Combine(_options.BasePath, subDirectory);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(directory, fileName);
            
            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Devolver ruta relativa
            return Path.Combine(subDirectory, fileName).Replace("\\", "/");
        }
        
        public void DeleteFile(string relativePath)
        {
            string fullPath = Path.Combine(_options.BasePath, relativePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        
        private void ValidateFile(IFormFile file)
        {
            // Validar tamaño
            if (file.Length > _options.MaxFileSizeBytes)
            {
                throw new ArgumentException($"El archivo excede el tamaño máximo permitido de {_options.MaxFileSizeBytes / 1024 / 1024}MB");
            }
            
            // Validar extensión
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Extensión de archivo no permitida. Las extensiones permitidas son: {string.Join(", ", _options.AllowedExtensions)}");
            }
        }
    }
}