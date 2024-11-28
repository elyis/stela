using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stela_file_server.Core;
using Stela_file_server.Core.Enums;
using Stela_file_server.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Stela_file_server.Api.Controllers
{
    [ApiController]
    [Route("api/memorial")]
    public class MemorialController : ControllerBase
    {
        private readonly INotifyService _notifyService;
        private readonly IFileUploaderService _fileUploaderService;

        private readonly string[] _supportedImageExtensions = new string[]
        {
            "gif",
            "jpg",
            "jpeg",
            "jfif",
            "png",
            "svg"
        };

        public MemorialController(
            INotifyService notifyService,
            IFileUploaderService fileUploaderService)
        {
            _notifyService = notifyService;
            _fileUploaderService = fileUploaderService;
        }

        [HttpPost("upload"), Authorize]
        [SwaggerOperation(Summary = "Загрузка изображения мемориала", Description = "Загрузка изображения мемориала")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение мемориала загружено")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Некорректный формат файла")]
        public async Task<IActionResult> Upload(
            [SwaggerParameter(Description = "Файл изображения", Required = true)] IFormFile file,
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery, Required] Guid memorialId)
        {
            var uploadResult = await _fileUploaderService.UploadFileAsync(Constants.LocalPathToStorages, file.OpenReadStream(), _supportedImageExtensions);
            if (!uploadResult.IsSuccess)
                return StatusCode((int)uploadResult.StatusCode);

            var body = new
            {
                MemorialId = memorialId,
                FileName = uploadResult.Body
            };
            _notifyService.Publish(body, ContentUploaded.MemorialImage);
            return Ok();
        }

        [HttpGet("download/{filename}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение скачано", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Изображение не найдено")]
        public async Task<IActionResult> Download(string filename)
        {
            var response = await _fileUploaderService.GetStreamAsync(Constants.LocalPathToStorages, filename);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode);

            return File(response.Body, "application/octet-stream", filename);
        }
    }
}