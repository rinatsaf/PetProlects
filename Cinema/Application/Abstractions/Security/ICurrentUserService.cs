namespace Application.Abstractions.Security;

public interface ICurrentUserService
{
    CurrentUserInfo GetCurrentUser();
}