using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared.Sich.Sponsors;
using Robust.Server.Player;
using Robust.Shared.Network;
using System.Linq;
using System.Threading.Tasks;
using DbSponsorRank = Content.Server.Database.SponsorRank;
using static Content.Shared.Sich.Sponsors.SponsorsEuiMsg;

namespace Content.Server.Sich.Sponsors.UI;
public sealed class SponsorsAdminEui : BaseEui
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly ISichSponsorManager _sichSponsorManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private readonly ISawmill _sawmill;
    private bool _isLoading;

    private readonly List<(SichSponsor a, string? lastUserName)> _sponsors = new List<(SichSponsor, string? lastUserName)>();
    private readonly List<DbSponsorRank> _sponsorRanks = new();

    public SponsorsAdminEui()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = _logManager.GetSawmill("sponsors.perms");
    }

    public override void Opened()
    {
        base.Opened();

        StateDirty();
        LoadFromDb();
        _adminManager.OnPermsChanged += AdminManagerOnPermsChanged;
    }

    public override void Closed()
    {
        base.Closed();

        _adminManager.OnPermsChanged -= AdminManagerOnPermsChanged;
    }

    private void AdminManagerOnPermsChanged(AdminPermsChangedEventArgs obj)
    {
        // Close UI if user loses +PERMISSIONS.
        if (obj.Player == Player && !UserAdminFlagCheck(AdminFlags.Permissions))
        {
            Close();
        }
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

        switch (msg)
        {
            case AddSponsor ca:
                {
                    await HandleCreateSponsor(ca);
                    break;
                }

            case UpdateSponsor ua:
                {
                    await HandleUpdateSponsor(ua);
                    break;
                }

            case RemoveSponsor ra:
                {
                    await HandleRemoveSponsor(ra);
                    break;
                }

            case AddSponsorRank ar:
                {
                    await HandleAddSponsorRank(ar);
                    break;
                }

            case UpdateSponsorRank ur:
                {
                    await HandleUpdateSponsorRank(ur);
                    break;
                }

            case RemoveSponsorRank ra:
                {
                    await HandleRemoveSponsorRank(ra);
                    break;
                }
        }

        if (!IsShutDown)
        {
            LoadFromDb();
        }
    }

    private async Task HandleRemoveSponsorRank(RemoveSponsorRank rr)
    {
        var rank = await _db.GetSponsorRankAsync(rr.Id);
        if (rank == null)
        {
            return;
        }

        await _db.RemoveSponsorRankAsync(rr.Id);

        //_adminManager.ReloadAdminsWithRank(rr.Id);
        await _sichSponsorManager.ReloadSponsorsAsync();
    }

    private async Task HandleUpdateSponsorRank(UpdateSponsorRank ur)
    {
        var rank = await _db.GetSponsorRankAsync(ur.Id);
        if (rank == null)
        {
            return;
        }

        rank.Name = ur.Name;
        rank.Color = ur.Color.ToHex();

        await _db.UpdateSponsorRankAsync(rank);

        _sawmill.Info($"{Player} updated sponsor rank {rank.Name}.");

        //_adminManager.ReloadAdminsWithRank(ur.Id);
        await _sichSponsorManager.ReloadSponsorsAsync();
    }

    private async Task HandleAddSponsorRank(AddSponsorRank ar)
    {
        var rank = new DbSponsorRank
        {
            Name = ar.Name,
            Color = ar.Color.ToHex(),
        };

        await _db.AddSponsorRankAsync(rank);

        _sawmill.Info($"{Player} added sponsor rank {rank.Name}");

        await _sichSponsorManager.ReloadSponsorsAsync();
    }

    private async Task HandleRemoveSponsor(RemoveSponsor ra)
    {
        var sponsor = await _db.GetSponsorDataForAsync(ra.UserId);
        if (sponsor == null)
        {
            // Doesn't exist.
            return;
        }

        await _db.RemoveSponsorAsync(ra.UserId);

        var record = await _db.GetPlayerRecordByUserId(ra.UserId);
        _sawmill.Info($"{Player} removed sponsor {record?.LastSeenUserName ?? ra.UserId.ToString()}");

        if (_playerManager.TryGetSessionById(ra.UserId, out var player))
        {
            await _sichSponsorManager.LoadData(player, default);
        }
    }

    private async Task HandleUpdateSponsor(UpdateSponsor ua)
    {
        var sponsor = await _db.GetSponsorDataForAsync(ua.UserId);
        if (sponsor == null)
        {
            // Was removed in the mean time I guess?
            return;
        }

        sponsor.SponsorRankId = ua.RankId;

        await _db.UpdateSponsorAsync(sponsor);

        var playerRecord = await _db.GetPlayerRecordByUserId(ua.UserId);
        var (bad, rankName) = await FetchAndCheckRank(ua.RankId);
        if (bad)
        {
            return;
        }

        var name = playerRecord?.LastSeenUserName ?? ua.UserId.ToString();

        _sawmill.Info($"{Player} updated admin {name} to {rankName}");

        if (_playerManager.TryGetSessionById(ua.UserId, out var player))
        {
            await _sichSponsorManager.LoadData(player, default);
        }
    }

    private async Task HandleCreateSponsor(AddSponsor ca)
    {
        string name;
        NetUserId userId;
        if (Guid.TryParse(ca.UserNameOrId, out var guid))
        {
            userId = new NetUserId(guid);
            var playerRecord = await _db.GetPlayerRecordByUserId(userId);
            if (playerRecord == null)
            {
                name = userId.ToString();
            }
            else
            {
                name = playerRecord.LastSeenUserName;
            }
        }
        else
        {
            // Username entered, resolve user ID from DB.
            var dbPlayer = await _db.GetPlayerRecordByUserName(ca.UserNameOrId);
            if (dbPlayer == null)
            {
                // username not in DB.
                // TODO: Notify user.
                _sawmill.Warning($"{Player} tried to add admin with unknown username {ca.UserNameOrId}.");
                return;
            }

            userId = dbPlayer.UserId;
            name = ca.UserNameOrId;
        }

        var existing = await _db.GetSponsorDataForAsync(userId);
        if (existing != null)
        {
            // Already exists.
            return;
        }

        var (bad, rankName) = await FetchAndCheckRank(ca.RankId);
        if (bad)
        {
            return;
        }

        rankName ??= "<no rank>";

        var sponsor = new SichSponsor
        {
            SponsorRankId = ca.RankId,
            UserId = userId.UserId,
        };

        await _db.AddSponsorAsync(sponsor);

        _sawmill.Info($"{Player} added sponsor {name} as {rankName}");

        if (_playerManager.TryGetSessionById(userId, out var player))
        {
            await _sichSponsorManager.LoadData(player, default);
        }
    }

    private async Task<(bool bad, string?)> FetchAndCheckRank(int? rankId)
    {
        string? ret = null;
        if (rankId is { } r)
        {
            var rank = await _db.GetSponsorRankAsync(r);
            if (rank == null)
            {
                // Tried to set to nonexistent rank.
                _sawmill.Warning($"{Player} tried to assign nonexistent admin rank.");
                return (true, null);
            }

            ret = rank.Name;
        }

        return (false, ret);
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

    private bool UserAdminFlagCheck(AdminFlags flags)
    {
        return _adminManager.HasAdminFlag(Player, flags);
    }
}
