using server.DAL.Entities;

namespace server.Utils
{
    public static class GeneralUtils
    {
        public static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" or ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" or ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" or ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }

        public static string GetAzureFileName(AppFile appFile)
        {
            return appFile.Id.ToString() + Path.GetExtension(appFile.Name);
        }

        public static string FormatDateOnly(DateOnly date)
        {
            return date.ToString("dd-MM-yyy");
        }

        public static string FormatDateOnly(DateTime date)
        {
            return date.AddHours(3).ToString("dd-MM-yyy");
        }

        public static string FormatDateTime(DateTime date)
        {
            return date.AddHours(3).ToString("dd-MM-yyy HH:mm");
        }
    }
}
