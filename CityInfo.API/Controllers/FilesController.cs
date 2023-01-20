using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        //Add a constructor
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public FilesController(
            FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new System.ArgumentNullException(
                    nameof(fileExtensionContentTypeProvider));
        }

        [HttpGet("{fileId}")]
        public ActionResult GetFile( string fileId)
        {
            //look up the actual file, depending on the fileId
            //make sure the file is copied to the root directory of the project.
            //set the file properties to: Copy to Output directory/copy always.
            //demo code
            var pathToFile = "creating-the-api-and-returning-resources-slides.pdf";

            //check whether the file exists
            if (!_fileExtensionContentTypeProvider.TryGetContentType(
                
                pathToFile, out var contentType))
            {
                //octet-stream is a default media type for arbitrary binary data
                contentType = "application/octet-stream";
            }

            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, contentType, Path.GetFileName(pathToFile));
        }
    }
}
