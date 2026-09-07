using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Supabase.Gotrue;

namespace ToDoWeb.Shared.Services
{
    public class SupabaseAuthenticationStateProvider : AuthenticationStateProvider
    {
        private const string AccessTokenKey = "supabase_session_access";
        private const string RefreshTokenKey = "supabase_session_refresh";

        private readonly Supabase.Client _client;
        private readonly IJSRuntime _js;
        private ClaimsPrincipal? _currentUser;
        private bool _listenerAttached;

        public SupabaseAuthenticationStateProvider(Supabase.Client client, IJSRuntime js)
        {
            _client = client;
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_currentUser is null)
            {
                _currentUser = await LoadUserAsync();
            }
            return new AuthenticationState(_currentUser);
        }

        public async Task<Session?> LoginAsync(string email, string password)
        {
            var session = await _client.Auth.SignIn(email, password);
            if (!string.IsNullOrEmpty(session?.AccessToken))
            {
                AttachStateListener();
                await PersistSessionAsync(session);
                SetAuthenticated(session);
            }
            return session;
        }

        public async Task<Session?> RegisterAsync(string email, string password)
        {
            var session = await _client.Auth.SignUp(email, password);
            if (!string.IsNullOrEmpty(session?.AccessToken))
            {
                AttachStateListener();
                await PersistSessionAsync(session);
                SetAuthenticated(session);
            }
            return session;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _client.Auth.SignOut();
            }
            catch
            {
            }
            await ClearSessionAsync();
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private async Task<ClaimsPrincipal> LoadUserAsync()
        {
            try
            {
                var access = await GetItemAsync(AccessTokenKey);
                if (string.IsNullOrEmpty(access))
                {
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }

                var refresh = await GetItemAsync(RefreshTokenKey);
                try
                {
                    await _client.InitializeAsync();
                    await _client.Auth.SetSession(access, refresh ?? string.Empty, false);

                    AttachStateListener();

                    var current = _client.Auth.CurrentSession;
                    if (current is not null && !string.IsNullOrEmpty(current.AccessToken))
                    {
                        await PersistSessionAsync(current);
                    }

                    var user = _client.Auth.CurrentUser;
                    if (user is not null)
                    {
                        return BuildPrincipal(user);
                    }
                }
                catch
                {
                    await ClearSessionAsync();
                }

                return new ClaimsPrincipal(new ClaimsIdentity());
            }
            catch
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }
        }

        private void AttachStateListener()
        {
            if (_listenerAttached)
            {
                return;
            }
            _listenerAttached = true;
            _client.Auth.AddStateChangedListener((_, _) =>
            {
                try
                {
                    var session = _client.Auth.CurrentSession;
                    if (session is not null && !string.IsNullOrEmpty(session.AccessToken))
                    {
                        _ = PersistSessionAsync(session);
                    }
                }
                catch
                {
                }
            });
        }

        private async Task PersistSessionAsync(Session session)
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, session.AccessToken);
                await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, session.RefreshToken ?? string.Empty);
            }
            catch
            {
            }
        }

        private async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await _js.InvokeAsync<string>("localStorage.getItem", key);
            }
            catch
            {
                return null;
            }
        }

        private async Task ClearSessionAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
            }
            catch
            {
            }
        }

        private void SetAuthenticated(Session session)
        {
            if (session.User is null)
            {
                return;
            }
            _currentUser = BuildPrincipal(session.User);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private static ClaimsPrincipal BuildPrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Email ?? user.Id ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, "supabase");
            return new ClaimsPrincipal(identity);
        }
    }
}