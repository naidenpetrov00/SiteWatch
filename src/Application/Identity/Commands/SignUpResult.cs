namespace Application.Identity.Commands;

public class SignUpResult
{
    public bool Succeeded { get; set; }
    public string Token { get; set; }
    public string[] Errors { get; set; }
}
