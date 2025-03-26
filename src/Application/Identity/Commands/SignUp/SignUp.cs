using MediatR;

namespace Application.Identity.Commands.SignUp;

public class SignUpCommand : IRequest<SignUpResult>
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class SignUpHandler : IRequestHandler<SignUpCommand, SignUpResult>
{
    public SignUp(UserManager)
    {
        
    }

    public Task<SignUpResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {

    }
}
