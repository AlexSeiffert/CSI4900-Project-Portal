using MudBlazor;

namespace uoProjects.Theming;

public static class AppTheme
{
    public static MudTheme Brand { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#8f001a",
            PrimaryDarken = "#7b0016",
            PrimaryContrastText = "#ffffff",
            Background = "#3a3a37",
            BackgroundGray = "#2d2d2c",
            Surface = "#42423f",
            AppbarBackground = "#2d2d2c",
            AppbarText = "#f2f2f2",
            DrawerBackground = "#2d2d2c",
            DrawerText = "#f2f2f2",
            TextPrimary = "#f2f2f2",
            TextSecondary = "#ccc5c0",
            TextDisabled = "#a39c96",
            ActionDefault = "#ccc5c0",
            ActionDisabled = "#757575",
            ActionDisabledBackground = "#c4c4c4",
            Divider = "#62625d",
            LinesDefault = "#62625d",
            LinesInputs = "#a39c96",
            Success = "#0ca30c",
            Error = "#d03b3b"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography() { FontFamily = ["Work Sans", "Arial", "sans-serif"] },
            H1 = new H1Typography() { FontSize = "3rem", FontWeight = "700", LineHeight = "1.15", LetterSpacing = "-0.035em" },
            H2 = new H2Typography() { FontSize = "2rem", FontWeight = "700", LineHeight = "1.25", LetterSpacing = "-0.025em" },
            H3 = new H3Typography() { FontSize = "1.5rem", FontWeight = "700", LineHeight = "1.3" },
            H6 = new H6Typography() { FontWeight = "600" },
            Body1 = new Body1Typography() { FontSize = "1rem", LineHeight = "1.65" },
            Button = new ButtonTypography() { FontSize = "0.875rem", FontWeight = "600", TextTransform = "none", LetterSpacing = "0" },
            Caption = new CaptionTypography() { FontSize = "0.75rem" },
            Overline = new OverlineTypography() { FontSize = "0.75rem", FontWeight = "700", LetterSpacing = "0.12em" }
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "12px" }
    };
}
