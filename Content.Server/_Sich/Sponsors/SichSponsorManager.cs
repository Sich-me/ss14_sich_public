using Content.Server.Construction.Completions;
using Content.Server.Database;
using Content.Shared.Preferences;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.Sich.Sponsors;

/// <summary>
/// Sends <see cref="MsgPreferencesAndSettings"/> before the client joins the lobby.
/// Receives <see cref="MsgSelectCharacter"/> and <see cref="MsgUpdateCharacter"/> at any time.
/// </summary>
public sealed class SichSponsorManager : ISichSponsorManager, IPostInjectInit
{
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    // Cache player prefs on the server so we don't need as much async hell related to them.
    private readonly Dictionary<NetUserId, PlayerSponsorData> _cachedPlayerPrefs =
        new();

    private ISawmill _sawmill = default!;

    public void Init()
    {
        _netManager.RegisterNetMessage<MsgPreferencesAndSettings>();
        _sawmill = _log.GetSawmill("sponsorPrefs");
    }

    // Should only be called via UserDbDataManager.
    public async Task<PlayerSponsorData> LoadData(ICommonSession session, CancellationToken cancel = default)
    {
        if (!ShouldStorePrefs(session.Channel.AuthType))
        {
            // Don't store data for guests.
            var sponsorData = new PlayerSponsorData
            {
                SponsorLoaded = true,
                Sponsor = null
            };

            _cachedPlayerPrefs[session.UserId] = sponsorData;
            return sponsorData;
        }
        else
        {
            var sponsorData = new PlayerSponsorData();
            var loadTask = LoadPrefs();
            _cachedPlayerPrefs[session.UserId] = sponsorData;

            await loadTask;

            async Task LoadPrefs()
            {
                var spons = await GetOrCreateSponsorAsync(session.UserId, cancel);
                sponsorData.Sponsor = spons;
            }
            return sponsorData;
        }
    }

    public async Task ReloadSponsorsAsync()
    {
        _cachedPlayerPrefs.Clear();
        var chanels = _netManager.Channels.ToList();
        foreach (var chanel in chanels)
        {
            if (chanel.IsConnected == false)
                continue;

            var session = _playerManager.GetSessionByChannel(chanel);
            if (session == null)
                continue;
            await LoadData(session);
        }
    }

    public void FinishLoad(ICommonSession session)
    {
        var sponsData = _cachedPlayerPrefs[session.UserId];
        sponsData.SponsorLoaded = true;
    }

    public void OnClientDisconnected(ICommonSession session)
    {
        _cachedPlayerPrefs.Remove(session.UserId);
    }

    public bool HavePreferencesLoaded(ICommonSession session)
    {
        return _cachedPlayerPrefs.ContainsKey(session.UserId);
    }


    /// <summary>
    /// Tries to get the preferences from the cache
    /// </summary>
    /// <param name="userId">User Id to get preferences for</param>
    /// <param name="playerSponsor">The user preferences if true, otherwise null</param>
    /// <returns>If preferences are not null</returns>
    public bool TryGetCachedSponsor(NetUserId userId,
        [NotNullWhen(true)] out SichSponsor? playerSponsor)
    {
        if (_cachedPlayerPrefs.TryGetValue(userId, out var spons))
        {
            playerSponsor = spons.Sponsor;
            return spons.Sponsor != null;
        }

        playerSponsor = null;
        return false;
    }

    /// <summary>
    /// Retrieves preferences for the given username from storage.
    /// </summary>
    public SichSponsor GetSponsor(NetUserId userId)
    {
        var spons = _cachedPlayerPrefs[userId].Sponsor;
        if (spons == null)
        {
            throw new InvalidOperationException("Preferences for this player have not loaded yet.");
        }

        return spons;
    }

    /// <summary>
    /// Retrieves preferences for the given username from storage or returns null.
    /// </summary>
    public SichSponsor? GetSichSponsorOrNull(NetUserId? userId)
    {
        if (userId == null)
            return null;

        if (_cachedPlayerPrefs.TryGetValue(userId.Value, out var spons))
            return spons.Sponsor;
        return null;
    }

    private async Task<SichSponsor?> GetOrCreateSponsorAsync(NetUserId userId, CancellationToken cancel)
    {
        var prefs = await _db.GetSponsorDataForAsync(userId, cancel);
        return prefs;
    }

    internal static bool ShouldStorePrefs(LoginType loginType)
    {
        return loginType.HasStaticUserId();
    }

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnFinishLoad(FinishLoad);
        _userDb.AddOnPlayerDisconnect(OnClientDisconnected);
    }
}

public sealed class PlayerSponsorData
{
    public bool SponsorLoaded;
    public SichSponsor? Sponsor;
}
