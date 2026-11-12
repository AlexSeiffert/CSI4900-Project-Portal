namespace uoProjects.Components;

internal static class LucideIcons
{
    public static string Svg(string name, double strokeWidth = 2) =>
        $"<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"{strokeWidth.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{Markup[name]}</g>";

    public static readonly IReadOnlyDictionary<string, string> Markup = new Dictionary<string, string>
    {
        ["arrow-left"] = @"<path d=""m12 19-7-7 7-7""/> <path d=""M19 12H5""/>",
        ["bookmark"] = @"<path d=""m19 21-7-4-7 4V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16z""/>",
        ["brain-circuit"] = @"<path d=""M12 5a3 3 0 1 0-5.997.125 4 4 0 0 0-2.526 5.77 4 4 0 0 0 .556 6.588A4 4 0 1 0 12 18Z""/> <path d=""M9 13a4.5 4.5 0 0 0 3-4""/> <path d=""M6.003 5.125A3 3 0 0 0 6.401 6.5""/> <path d=""M3.477 10.896a4 4 0 0 1 .585-.396""/> <path d=""M6 18a4 4 0 0 1-1.967-.516""/> <path d=""M12 13h4""/> <path d=""M12 18h6a2 2 0 0 1 2 2v1""/> <path d=""M12 8h8""/> <path d=""M16 8V5a2 2 0 0 1 2-2""/> <circle cx=""16"" cy=""13"" r="".5""/> <circle cx=""18"" cy=""3"" r="".5""/> <circle cx=""20"" cy=""21"" r="".5""/> <circle cx=""20"" cy=""8"" r="".5""/>",
        ["check"] = @"<path d=""M20 6 9 17l-5-5""/>",
        ["chevron-down"] = @"<path d=""m6 9 6 6 6-6""/>",
        ["chevron-up"] = @"<path d=""m18 15-6-6-6 6""/>",
        ["chevrons-up-down"] = @"<path d=""m7 15 5 5 5-5""/> <path d=""m7 9 5-5 5 5""/>",
        ["code-xml"] = @"<path d=""m18 16 4-4-4-4""/> <path d=""m6 8-4 4 4 4""/> <path d=""m14.5 4-5 16""/>",
        ["database"] = @"<ellipse cx=""12"" cy=""5"" rx=""9"" ry=""3""/> <path d=""M3 5V19A9 3 0 0 0 21 19V5""/> <path d=""M3 12A9 3 0 0 0 21 12""/>",
        ["download"] = @"<path d=""M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4""/> <polyline points=""7 10 12 15 17 10""/> <line x1=""12"" x2=""12"" y1=""15"" y2=""3""/>",
        ["folder-open"] = @"<path d=""m6 14 1.5-2.9A2 2 0 0 1 9.24 10H20a2 2 0 0 1 1.94 2.5l-1.54 6a2 2 0 0 1-1.95 1.5H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h3.9a2 2 0 0 1 1.69.9l.81 1.2a2 2 0 0 0 1.67.9H18a2 2 0 0 1 2 2v2""/>",
        ["gamepad-2"] = @"<line x1=""6"" x2=""10"" y1=""11"" y2=""11""/> <line x1=""8"" x2=""8"" y1=""9"" y2=""13""/> <line x1=""15"" x2=""15.01"" y1=""12"" y2=""12""/> <line x1=""18"" x2=""18.01"" y1=""10"" y2=""10""/> <path d=""M17.32 5H6.68a4 4 0 0 0-3.978 3.59c-.006.052-.01.101-.017.152C2.604 9.416 2 14.456 2 16a3 3 0 0 0 3 3c1 0 1.5-.5 2-1l1.414-1.414A2 2 0 0 1 9.828 16h4.344a2 2 0 0 1 1.414.586L17 18c.5.5 1 1 2 1a3 3 0 0 0 3-3c0-1.545-.604-6.584-.685-7.258-.007-.05-.011-.1-.017-.151A4 4 0 0 0 17.32 5z""/>",
        ["globe"] = @"<circle cx=""12"" cy=""12"" r=""10""/> <path d=""M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20""/> <path d=""M2 12h20""/>",
        ["inbox"] = @"<polyline points=""22 12 16 12 14 15 10 15 8 12 2 12""/> <path d=""M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z""/>",
        ["graduation-cap"] = @"<path d=""M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z""/> <path d=""M22 10v6""/> <path d=""M6 12.5V16a6 3 0 0 0 12 0v-3.5""/>",
        ["layout-dashboard"] = @"<rect width=""7"" height=""9"" x=""3"" y=""3"" rx=""1""/> <rect width=""7"" height=""5"" x=""14"" y=""3"" rx=""1""/> <rect width=""7"" height=""9"" x=""14"" y=""12"" rx=""1""/> <rect width=""7"" height=""5"" x=""3"" y=""16"" rx=""1""/>",
        ["log-out"] = @"<path d=""M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4""/> <polyline points=""16 17 21 12 16 7""/> <line x1=""21"" x2=""9"" y1=""12"" y2=""12""/>",
        ["user-round"] = @"<circle cx=""12"" cy=""8"" r=""5""/> <path d=""M20 21a8 8 0 0 0-16 0""/>",
        ["palette"] = @"<circle cx=""13.5"" cy=""6.5"" r="".5"" fill=""currentColor""/> <circle cx=""17.5"" cy=""10.5"" r="".5"" fill=""currentColor""/> <circle cx=""8.5"" cy=""7.5"" r="".5"" fill=""currentColor""/> <circle cx=""6.5"" cy=""12.5"" r="".5"" fill=""currentColor""/> <path d=""M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.926 0 1.648-.746 1.648-1.688 0-.437-.18-.835-.437-1.125-.29-.289-.438-.652-.438-1.125a1.64 1.64 0 0 1 1.668-1.668h1.996c3.051 0 5.555-2.503 5.555-5.554C21.965 6.012 17.461 2 12 2z""/>",
        ["plus"] = @"<path d=""M5 12h14""/> <path d=""M12 5v14""/>",
        ["search"] = @"<circle cx=""11"" cy=""11"" r=""8""/> <path d=""m21 21-4.3-4.3""/>",
        ["send"] = @"<path d=""M14.536 21.686a.5.5 0 0 0 .937-.024l6.5-19a.496.496 0 0 0-.635-.635l-19 6.5a.5.5 0 0 0-.024.937l7.93 3.18a2 2 0 0 1 1.112 1.11z""/> <path d=""m21.854 2.147-10.94 10.939""/>",
        ["server"] = @"<rect width=""20"" height=""8"" x=""2"" y=""2"" rx=""2"" ry=""2""/> <rect width=""20"" height=""8"" x=""2"" y=""14"" rx=""2"" ry=""2""/> <line x1=""6"" x2=""6.01"" y1=""6"" y2=""6""/> <line x1=""6"" x2=""6.01"" y1=""18"" y2=""18""/>",
        ["shapes"] = @"<path d=""M8.3 10a.7.7 0 0 1-.626-1.079L11.4 3a.7.7 0 0 1 1.198-.043L16.3 8.9a.7.7 0 0 1-.572 1.1Z""/> <rect x=""3"" y=""14"" width=""7"" height=""7"" rx=""1""/> <circle cx=""17.5"" cy=""17.5"" r=""3.5""/>",
        ["shield-check"] = @"<path d=""M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z""/> <path d=""m9 12 2 2 4-4""/>",
        ["sigma"] = @"<path d=""M18 7V5a1 1 0 0 0-1-1H6.5a.5.5 0 0 0-.4.8l4.5 6a2 2 0 0 1 0 2.4l-4.5 6a.5.5 0 0 0 .4.8H17a1 1 0 0 0 1-1v-2""/>",
        ["tags"] = @"<path d=""m15 5 6.3 6.3a2.4 2.4 0 0 1 0 3.4L17 19""/> <path d=""M9.586 5.586A2 2 0 0 0 8.172 5H3a1 1 0 0 0-1 1v5.172a2 2 0 0 0 .586 1.414L8.29 18.29a2.426 2.426 0 0 0 3.42 0l3.58-3.58a2.426 2.426 0 0 0 0-3.42z""/> <circle cx=""6.5"" cy=""9.5"" r="".5"" fill=""currentColor""/>",
        ["users"] = @"<path d=""M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2""/> <circle cx=""9"" cy=""7"" r=""4""/> <path d=""M22 21v-2a4 4 0 0 0-3-3.87""/> <path d=""M16 3.13a4 4 0 0 1 0 7.75""/>",
    };
}
