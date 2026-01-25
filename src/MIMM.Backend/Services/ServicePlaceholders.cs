namespace MIMM.Backend.Services;

// NOTE: IAuthService is now fully implemented in AuthService.cs
// NOTE: IEntryService is now fully implemented in EntryService.cs
public interface IUserService { }
public interface IAnalyticsService { }
public interface ILastFmService { }
public interface IEmailService { }

// Placeholder implementations - these will be fully implemented later
public class UserService : IUserService { }
public class AnalyticsService : IAnalyticsService { }
public class LastFmService : ILastFmService { }
public class EmailService : IEmailService { }

// Refit HTTP Client placeholder
public interface ILastFmHttpClient { }
