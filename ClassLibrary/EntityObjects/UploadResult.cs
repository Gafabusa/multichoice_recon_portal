namespace ClassLibrary.EntityObjects
{
    /// <summary>
    /// Outcome of a manual statement upload (file dropped into the service's
    /// raw folder so the formatter thread can pick it up).
    /// </summary>
    public class UploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SavedFilePath { get; set; }
    }
}
