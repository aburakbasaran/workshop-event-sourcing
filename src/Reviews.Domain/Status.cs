namespace Reviews.Domain
{
    /// <summary>
    /// (Create || Update ) => Draft
    /// (Publish) => PendingApprove
    /// (Approve) => Approved
    /// (Reject) => Rejected
    /// (Update if current status is approved or rejected) => PendingApprove
    /// </summary>
    public enum Status
    {
        Draft,
        PendingApprove,
        Approved,
        Rejected
    }
}