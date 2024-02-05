using PortHub.Api.Tokens.Data.Models.ViewModel;
using PortHub.Api.Tokens.Data.Models.viewModels;

namespace PortHub.Api.Tokens.Interface
{
    public interface ITokenGenerator
    {
        Task<(bool IsSuccess, AuthVM refreshToken, string ErrorMessage)> RefreshTokenGeneratorAsync(ApplicationUserVM userExists);
        Task<(bool isSuccess, string ErrorMessage, AuthVM auth)> RefreshTokenVM(RefreshTokenVM refereshTokenVM);

    }
}
