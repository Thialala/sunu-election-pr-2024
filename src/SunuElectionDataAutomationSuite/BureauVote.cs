// Region,Departement,Commune,LieuDeVote,Bureau,Electeurs,Implantation
public class BureauVote
{
    [Index(0)]
    public string Region { get; set; }
    [Index(1)]
    public string Departement { get; set; }
    [Index(2)]

    public string Commune { get; set; }
    [Index(3)]

    public string LieuDeVote { get; set; }
    [Index(4)]

    public string Bureau { get; set; }
    [Index(5)]

    public int? Electeurs { get; set; } = 0;
    [Index(6)]

    public string Implantation { get; set; }
}


public sealed class BureauVoteMap : ClassMap<BureauVote>
{
    public BureauVoteMap()
    {
        Map(member => member.Region).Name("Region");
        Map(member => member.Departement).Name("Departement");
        Map(member => member.Commune).Name("Commune");
        Map(member => member.LieuDeVote).Name("Lieu de vote");
        Map(member => member.Bureau).Name("Bureau");
        Map(member => member.Electeurs).Name("Electeurs");
        Map(member => member.Implantation).Name("Implantation");
        ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace);
    }

    public Func<ShouldSkipRecordArgs, bool> ShouldSkipRecord { get; }
}
