namespace Application.Proposals;

public sealed class ProposalConflictException : Exception
{
    public ProposalConflictException(string message) : base(message)
    {
    }

    public ProposalConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
