using Content.Server.Database;
using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.Sich.Sponsors;
using Robust.Shared.Network;
using System.Linq;
using DbSponsorRank = Content.Server.Database.SponsorRank;

namespace Content.Server.Sich.Sponsors.UI;
public sealed class SponsorsEui : BaseEui
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private readonly ISawmill _sawmill;
    private bool _isLoading;

    private readonly List<(SichSponsor a, string? lastUserName)> _sponsors = new List<(SichSponsor, string? lastUserName)>();
    private readonly List<DbSponsorRank> _sponsorRanks = new();

    public SponsorsEui()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = _logManager.GetSawmill("sponsors.view");
    }

    public override void Opened()
    {
        base.Opened();

        StateDirty();
        LoadFromDb();
    }

    public override void Closed()
    {
        base.Closed();
    }

    public override EuiStateBase GetNewState()
    {
        if (_isLoading)
        {
            return new SponsorsEuiState
            {
                IsLoading = true
            };
        }

        return new SponsorsEuiState
        {
            Sponsors = _sponsors.Select(p => new SponsorsEuiState.SponsorData
            {
                RankId = p.a.SponsorRankId,
                UserId = new NetUserId(p.a.UserId),
                UserName = p.lastUserName,
            }).ToArray(),

            SponsorRanks = _sponsorRanks.ToDictionary(a => a.Id, a => new SponsorsEuiState.SponsorRankData
            {
                Name = a.Name,
                Color = Color.FromHex(a.Color)
            })
        };
    }

    public override async void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!IsShutDown)
        {
            LoadFromDb();
        }
    }

    private async void LoadFromDb()
    {
        StateDirty();
        _isLoading = true;
        var (sponsors, ranks) = await _db.GetAllSichSponsorsAsync();

        _sponsors.Clear();
        _sponsors.AddRange(sponsors);
        _sponsorRanks.Clear();
        _sponsorRanks.AddRange(ranks);

        _isLoading = false;
        StateDirty();
    }
}
