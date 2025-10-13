namespace Calcpad.WebApi.Controllers.DTOs
{
    public class UpdateCpdReadFromData
    {
        public string UniqueId { get; set; }

        public string OldFromPath { get; set; }

        public IFormFile File { get; set; }
    }
}
