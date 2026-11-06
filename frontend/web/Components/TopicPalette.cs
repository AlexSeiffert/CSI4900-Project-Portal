namespace ProjectPortal.Web.Components;

/// <summary>
/// The ten topic tags (db/migrations/0012_topic_tags.sql): each one's Lucide icon and what it covers.
/// Tags are all the same neutral grey, told apart by their icon and name; colour is kept for data.
/// Unknown names are shown as Other.
/// </summary>
public static class TopicPalette
{
    private sealed record Tag(string Icon, string Covers);

    private static readonly Dictionary<string, Tag> Tags = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AI/ML"] = new("brain-circuit", "Machine learning, deep learning, NLP, computer vision, LLMs, reinforcement learning, data mining"),
        ["Software Engineering"] = new("code-xml", "Software design, testing and verification, DevOps, programming languages, compilers, developer tools"),
        ["Web & Mobile"] = new("globe", "Web apps, mobile apps, full-stack systems, APIs, cross-platform development"),
        ["Security & Privacy"] = new("shield-check", "Cybersecurity, cryptography, network security, privacy, blockchain"),
        ["Data & Databases"] = new("database", "Databases, big data, data engineering, analytics, visualization, information retrieval"),
        ["Systems & Networks"] = new("server", "Operating systems, distributed and cloud computing, networking, embedded systems, IoT, high-performance computing"),
        ["HCI & Design"] = new("palette", "User experience, accessibility, usability studies, human-centred design, VR/AR"),
        ["Games & Graphics"] = new("gamepad-2", "Game development, computer graphics, simulation, animation, multimedia"),
        ["Theory & Algorithms"] = new("sigma", "Algorithms, complexity, formal methods, computational math, quantum computing, bioinformatics"),
        ["Other"] = new("shapes", "Education tech, robotics, ethics and society, and anything that doesn't fit above"),
    };

    private static readonly List<string> Order = [.. Tags.Keys];

    private static Tag Find(string topicName) => Tags.GetValueOrDefault(topicName.Trim()) ?? Tags["Other"];

    /// <summary>The tag names, in display order.</summary>
    public static IEnumerable<string> Names => Order;

    /// <summary>The topic's Lucide icon name, for &lt;Icon /&gt;.</summary>
    public static string IconFor(string topicName) => Find(topicName).Icon;

    /// <summary>What the tag covers, for tooltips and pickers.</summary>
    public static string Describe(string topicName) => Find(topicName).Covers;

    /// <summary>Sorts topics into the palette's order rather than alphabetically.</summary>
    public static int OrderOf(string topicName)
    {
        var index = Order.FindIndex(name => name.Equals(topicName.Trim(), StringComparison.OrdinalIgnoreCase));
        return index < 0 ? Order.Count : index;
    }
}
