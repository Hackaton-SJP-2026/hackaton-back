using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Hackaton.Domain.Interface;

namespace HackatonApi.Auth
{
    public sealed class HttpContextUserResolver : IUserResolver
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpContextUserResolver(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Guid GetCurrentUserId()
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return Guid.Empty;

            var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }
}
