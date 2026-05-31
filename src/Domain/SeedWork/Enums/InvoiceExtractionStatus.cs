namespace Domain.SeedWork.Enums;

public enum InvoiceExtractionStatus
{
    Uploaded = 0,
    Processing = 1,
    Extracted = 2,
    NeedsReview = 3,
    Approved = 4,
    Rejected = 5,
    Failed = 6
}
