using MudBlazor;

namespace Longstone.Web.Theme;

public static class LongstoneTheme
{
    public static readonly MudTheme Instance = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1A237E",
            PrimaryDarken = "#0D1654",
            PrimaryLighten = "#3949AB",
            Secondary = "#1565C0",
            SecondaryDarken = "#0D47A1",
            SecondaryLighten = "#1E88E5",
            Tertiary = "#00897B",
            AppbarBackground = "#1A237E",
            AppbarText = Colors.Shades.White,
            DrawerBackground = "#F5F5F5",
            DrawerText = "#424242",
            DrawerIcon = "#616161",
            Background = "#FAFAFA",
            Surface = Colors.Shades.White,
            TextPrimary = "#212121",
            TextSecondary = "#757575",
            ActionDefault = "#616161",
            ActionDisabled = "#BDBDBD",
            ActionDisabledBackground = "#E0E0E0",
            Success = "#2E7D32",
            Warning = "#F57F17",
            Error = "#C62828",
            Info = "#1565C0",
            Divider = "#E0E0E0",
            TableHover = "#E8EAF6",
            TableStriped = "#F5F5F5",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#5C6BC0",
            PrimaryDarken = "#3949AB",
            PrimaryLighten = "#7986CB",
            Secondary = "#42A5F5",
            SecondaryDarken = "#1E88E5",
            SecondaryLighten = "#64B5F6",
            Tertiary = "#4DB6AC",
            AppbarBackground = "#1A1A2E",
            AppbarText = "#E0E0E0",
            DrawerBackground = "#1E1E2E",
            DrawerText = "#B0BEC5",
            DrawerIcon = "#90A4AE",
            Background = "#121212",
            Surface = "#1E1E1E",
            TextPrimary = "#E0E0E0",
            TextSecondary = "#9E9E9E",
            ActionDefault = "#B0BEC5",
            ActionDisabled = "#616161",
            ActionDisabledBackground = "#2C2C2C",
            Success = "#66BB6A",
            Warning = "#FFA726",
            Error = "#EF5350",
            Info = "#42A5F5",
            Divider = "#424242",
            TableHover = "#283593",
            TableStriped = "#1A1A2E",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.5",
            },
            H3 = new H3Typography
            {
                FontSize = "1.75rem",
                FontWeight = "500",
            },
            H5 = new H5Typography
            {
                FontSize = "1.25rem",
                FontWeight = "500",
            },
            H6 = new H6Typography
            {
                FontSize = "1rem",
                FontWeight = "500",
            },
            Body1 = new Body1Typography
            {
                FontSize = "0.875rem",
                LineHeight = "1.5",
            },
            Body2 = new Body2Typography
            {
                FontSize = "0.8125rem",
                LineHeight = "1.5",
            },
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "260px",
            AppbarHeight = "56px",
        },
    };
}
