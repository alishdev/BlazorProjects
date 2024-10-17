namespace AnalyzePodcastEpisodes;

public class PodcastMetadata
{
    public string? StudentName { get; set; }
    public string? EpisodeTitle { get; set; }
    public List<string> CollegesApplied { get; set; }
    public List<string> CollegesAccepted { get; set; }
    public double GPAWeighted { get; set; }
    public double GPAUnweighted { get; set; }
    public Dictionary<string, int> SATScores { get; set; }
    public Dictionary<string, int> ACTScores { get; set; }
    public List<string> ExtracurricularActivities { get; set; }
    public List<string> EssayTopics { get; set; }
    public List<string> InterviewExperiences { get; set; }
    public List<string> ScholarshipsReceived { get; set; }
    
    public PodcastMetadata()
    {
        GPAUnweighted = GPAUnweighted = 0.0;
        CollegesApplied = new List<string>();
        CollegesAccepted = new List<string>();
        SATScores = new Dictionary<string, int>();
        ACTScores = new Dictionary<string, int>();
        ExtracurricularActivities = new List<string>();
        EssayTopics = new List<string>();
        InterviewExperiences = new List<string>();
        ScholarshipsReceived = new List<string>();
    }
}