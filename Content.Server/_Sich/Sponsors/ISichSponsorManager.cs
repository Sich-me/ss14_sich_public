using Content.Server.Database;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.Sich.Sponsors;

public interface ISichSponsorManager
{
    void Init();

    Task<PlayerSponsorData> LoadData(ICommonSession session, CancellationToken cancel);
    void FinishLoad(ICommonSession session);
    void OnClientDisconnected(ICommonSession session);

    bool TryGetCachedSponsor(NetUserId userId, [NotNullWhen(true)] out SichSponsor? playerPreferences);
    SichSponsor GetSponsor(NetUserId userId);
    SichSponsor? GetSichSponsorOrNull(NetUserId? userId);
    bool HavePreferencesLoaded(ICommonSession session);

    Task ReloadSponsorsAsync();
}
