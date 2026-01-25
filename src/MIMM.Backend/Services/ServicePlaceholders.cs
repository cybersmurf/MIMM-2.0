namespace MIMM.Backend.Services;

// NOTE: IAuthService is now fully implemented in AuthService.cs
// NOTE: IEntryService is now fully implemented in EntryService.cs
public interface IUserService { }
public interface IAnalyticsService { }

public interface ILastFmService
{
	Task<string> GetAuthUrlAsync(Guid userId, string requestBaseUrl, CancellationToken cancellationToken = default);
	Task<(bool Success, string? Username, string? Error)> HandleCallbackAsync(string state, string token, CancellationToken cancellationToken = default);
	Task<(bool Success, string? Error)> ScrobbleAsync(Guid userId, string songTitle, string? artistName, string? albumName, DateTime? timestamp, CancellationToken cancellationToken = default);
}

public interface IEmailService { }

// Placeholder implementations - these will be fully implemented later (except LastFmService defined in separate file)
public class UserService : IUserService { }
public class AnalyticsService : IAnalyticsService { }
public class EmailService : IEmailService { }

// Refit HTTP Client placeholder (not used in scaffold)
public interface ILastFmHttpClient { }
