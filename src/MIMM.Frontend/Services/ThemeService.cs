using Microsoft.JSInterop;
using MudBlazor;

namespace MIMM.Frontend.Services;

/// <summary>
/// Service for managing application themes (multi-theme support)
/// Supports: dark, midnight, twilight, ocean, forest
/// with localStorage persistence
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Current active theme
    /// </summary>
    string CurrentTheme { get; }
    
    /// <summary>
    /// Available themes
    /// </summary>
    string[] AvailableThemes { get; }
    
    /// <summary>
    /// Theme metadata (name, description)
    /// </summary>
    IReadOnlyDictionary<string, ThemeMetadata> ThemeMetadata { get; }

    /// <summary>
    /// Current MudBlazor theme
    /// </summary>
    MudTheme CurrentMudTheme { get; }
    
    /// <summary>
    /// Event raised when theme changes
    /// </summary>
    event EventHandler<string>? ThemeChanged;
    
    /// <summary>
    /// Initialize theme service and load saved preference
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Cycle to next theme
    /// </summary>
    Task CycleThemeAsync();
    
    /// <summary>
    /// Set specific theme
    /// </summary>
    Task SetThemeAsync(string theme);
}

/// <summary>
/// Theme metadata including name and description
/// </summary>
public record ThemeMetadata
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Icon { get; init; }
    public required string AccentColor { get; init; }
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "dark";
    private const string THEME_STORAGE_KEY = "mimm-theme-preference";

    // Available themes: dark (default), midnight (ultra dark), twilight (purple), ocean (cyan), forest (green)
    private static readonly string[] AVAILABLE_THEMES = ["dark", "midnight", "twilight", "ocean", "forest"];
    
    private static readonly Dictionary<string, ThemeMetadata> THEME_METADATA = new()
    {
        {
            "dark", new ThemeMetadata
            {
                Name = "Dark",
                Description = "Classic dark mode",
                Icon = "dark_mode",
                AccentColor = "#3b82f6"
            }
        },
        {
            "midnight", new ThemeMetadata
            {
                Name = "Midnight",
                Description = "Ultra dark with electric accents",
                Icon = "brightness_2",
                AccentColor = "#00d9ff"
            }
        },
        {
            "twilight", new ThemeMetadata
            {
                Name = "Twilight",
                Description = "Cozy purple theme",
                Icon = "light_mode",
                AccentColor = "#e879f9"
            }
        },
        {
            "ocean", new ThemeMetadata
            {
                Name = "Ocean",
                Description = "Cool cyan theme",
                Icon = "waves",
                AccentColor = "#06b6d4"
            }
        },
        {
            "forest", new ThemeMetadata
            {
                Name = "Forest",
                Description = "Nature-inspired green",
                Icon = "landscape",
                AccentColor = "#10b981"
            }
        }
    };

    private static readonly IReadOnlyDictionary<string, MudTheme> THEMES = new Dictionary<string, MudTheme>
    {
        ["dark"] = new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#3b82f6",
                Secondary = "#8b5cf6",
                Background = "#0f172a",
                Surface = "#111827",
                AppbarBackground = "#0f172a",
                DrawerBackground = "#0b1220",
                TextPrimary = "#f8fafc",
                TextSecondary = "#d1d5db",
                TextDisabled = "#6b7280"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px"
            }
        },
        ["midnight"] = new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#00d9ff",
                Secondary = "#22d3ee",
                Background = "#05070f",
                Surface = "#0b1020",
                AppbarBackground = "#05070f",
                DrawerBackground = "#070b16",
                TextPrimary = "#f1f5f9",
                TextSecondary = "#cbd5f5",
                TextDisabled = "#64748b"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px"
            }
        },
        ["twilight"] = new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#e879f9",
                Secondary = "#a855f7",
                Background = "#1a1025",
                Surface = "#231234",
                AppbarBackground = "#1a1025",
                DrawerBackground = "#170d22",
                TextPrimary = "#f8fafc",
                TextSecondary = "#e2e8f0",
                TextDisabled = "#94a3b8"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px"
            }
        },
        ["ocean"] = new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#06b6d4",
                Secondary = "#22d3ee",
                Background = "#08141a",
                Surface = "#0b1f29",
                AppbarBackground = "#08141a",
                DrawerBackground = "#061018",
                TextPrimary = "#f8fafc",
                TextSecondary = "#d1d5db",
                TextDisabled = "#6b7280"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px"
            }
        },
        ["forest"] = new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#10b981",
                Secondary = "#22c55e",
                Background = "#0b1410",
                Surface = "#0f1f17",
                AppbarBackground = "#0b1410",
                DrawerBackground = "#0a120e",
                TextPrimary = "#f8fafc",
                TextSecondary = "#d1d5db",
                TextDisabled = "#6b7280"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px"
            }
        }
    };

    public string CurrentTheme => _currentTheme;
    public string[] AvailableThemes => AVAILABLE_THEMES;
    public IReadOnlyDictionary<string, ThemeMetadata> ThemeMetadata => THEME_METADATA.AsReadOnly();
    public MudTheme CurrentMudTheme => THEMES.TryGetValue(_currentTheme, out var theme) ? theme : new MudTheme();
    public event EventHandler<string>? ThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Try to load saved theme from localStorage
            var savedTheme = await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem",
                THEME_STORAGE_KEY
            );

            if (!string.IsNullOrEmpty(savedTheme) && AVAILABLE_THEMES.Contains(savedTheme))
            {
                _currentTheme = savedTheme;
            }
            else
            {
                // Check system preference as fallback
                var prefersDark = await _jsRuntime.InvokeAsync<bool>(
                    "eval",
                    "window.matchMedia('(prefers-color-scheme: dark)').matches"
                );
                _currentTheme = prefersDark ? "dark" : "dark"; // Default to dark for now
            }

            // Apply theme to document
            await ApplyThemeAsync(_currentTheme);
            ThemeChanged?.Invoke(this, _currentTheme);
        }
        catch
        {
            // If JS interop fails (e.g., during prerendering), use default
            _currentTheme = "dark";
        }
    }

    public async Task CycleThemeAsync()
    {
        var currentIndex = Array.IndexOf(AVAILABLE_THEMES, _currentTheme);
        var nextIndex = (currentIndex + 1) % AVAILABLE_THEMES.Length;
        var nextTheme = AVAILABLE_THEMES[nextIndex];
        await SetThemeAsync(nextTheme);
    }

    public async Task SetThemeAsync(string theme)
    {
        if (!AVAILABLE_THEMES.Contains(theme))
        {
            throw new ArgumentException($"Theme must be one of: {string.Join(", ", AVAILABLE_THEMES)}", nameof(theme));
        }

        _currentTheme = theme;

        // Save to localStorage
        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                THEME_STORAGE_KEY,
                theme
            );
        }
        catch
        {
            // Ignore localStorage errors (e.g., privacy mode)
        }

        // Apply theme to document
        await ApplyThemeAsync(theme);

        // Notify listeners
        ThemeChanged?.Invoke(this, theme);
    }

    private async Task ApplyThemeAsync(string theme)
    {
        try
        {
            // Set data-theme attribute on document root
            await _jsRuntime.InvokeVoidAsync(
                "eval",
                $"document.documentElement.setAttribute('data-theme', '{theme}')"
            );
        }
        catch
        {
            // Ignore JS interop errors during prerendering
        }
    }
}
