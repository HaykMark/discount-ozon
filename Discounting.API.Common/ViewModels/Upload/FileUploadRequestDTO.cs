using Discounting.API.Common.CustomAttributes;
using Microsoft.AspNetCore.Http;

namespace Discounting.API.Common.ViewModels
{
    public class FileUploadRequestDTO : UploadRequestDTO
    {
        [CustomRequired]
        [AllowedExtension]
        public IFormFile File { get; set; }
    }
    
    public class FilesUploadRequestDTO : UploadRequestDTO
    {
        [CustomRequired]
        [AllowedExtension]
        public IFormFile[] Files { get; set; }
    }
}