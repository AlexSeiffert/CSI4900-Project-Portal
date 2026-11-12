namespace uoProjects.Admin.Insights;

public static class InsightMath
{
    public static int Percent(int part, int whole) => whole == 0 ? 0 : (int)Math.Round(100.0 * part / whole);

    public static string Shade(double value, double max) =>
        $"mono-{(max <= 0 ? 2 : Math.Clamp(2 + (int)Math.Round(3 * value / max), 2, 5))}";

    public static string Clip(string text, int length = 34) => text.Length <= length ? text : text[..(length - 1)].TrimEnd() + "…";
}
