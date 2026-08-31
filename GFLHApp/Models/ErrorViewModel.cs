namespace GFLHApp.Models
{
    /// <summary>
    /// View model used by the shared error view to display diagnostic request IDs.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

