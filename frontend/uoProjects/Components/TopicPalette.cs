namespace uoProjects.Components;

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

    public static IEnumerable<string> Names => Order;

    public static string IconFor(string topicName) => Find(topicName).Icon;

    public static string Describe(string topicName) => Find(topicName).Covers;

    public static int OrderOf(string topicName)
    {
        var index = Order.FindIndex(name => name.Equals(topicName.Trim(), StringComparison.OrdinalIgnoreCase));
        return index < 0 ? Order.Count : index;
    }
}
