namespace MIMM.Backend.Services;

public interface IAuthService { }
public interface IUserService { }
public interface IEntryService { }
public interface IAnalyticsService { }
public interface ILastFmService { }
public interface IMusicSearchService { }
public interface IEmailService { }

// Placeholder implementations - these will be fully implemented later
public class AuthService : IAuthService { }
public class UserService : IUserService { }
public class EntryService : IEntryService { }
public class AnalyticsService : IAnalyticsService { }
public class LastFmService : ILastFmService { }
public class MusicSearchService : IMusicSearchService { }
public class EmailService : IEmailService { }

// Refit HTTP Client placeholder
public interface ILastFmHttpClient { }
