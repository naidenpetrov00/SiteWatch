namespace Application.Proposals;

public sealed record ProposalFileResponse(
    Stream Stream,
    string FileName,
    string ContentType,
    long ContentLength);
