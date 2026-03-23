using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Sich.Sponsors;

public sealed class SponsorGhostSystem : EntitySystem
{
    [Dependency] private readonly ISichSponsorManager _sichSponsorManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GhostComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<GhostComponent, SpawnGhostForPlayerEvent>(OnSpawnGhostForPlayerEventHandler);
    }

    private void OnSpawnGhostForPlayerEventHandler(Entity<GhostComponent> ent, ref SpawnGhostForPlayerEvent args)
    {
        SetGhostOOCColor(ent.Owner, ent.Comp);
    }

    private void OnMindAdded(Entity<GhostComponent> ent, ref MindAddedMessage args)
    {
        SetGhostOOCColor(ent.Owner, ent.Comp);
    }

    private void SetGhostOOCColor(EntityUid uid, GhostComponent component)
    {
        var color = TryGetOOCColorForGhost(uid);
        if (string.IsNullOrEmpty(color))
        {
            return;
        }
        var c = Color.FromHex(color);
        var msg = new SetGhostColorMsg()
        {
            Color = c
        };
        RaiseLocalEvent(uid, msg);
    }

    private string? TryGetOOCColorForGhost(EntityUid uid)
    {
        if (!TryGetPlayerSessionFromEntity(uid, out var session))
        {
            return null;
        }

        var sponsor = _sichSponsorManager.GetSichSponsorOrNull(session.UserId);
        if (sponsor is null)
        {
            return null;
        }
        if(sponsor.SponsorRank == null) return null;
        return sponsor.SponsorRank.Color;
    }

    private bool TryGetPlayerSessionFromEntity(EntityUid uid, [NotNullWhen(true)] out ICommonSession? session)
    {
        session = null;
        if (!TryComp<ActorComponent>(uid, out var actor))
            return false;

        session = actor.PlayerSession;
        return true;
    }
}

public struct SpawnGhostForPlayerEvent
{
    public readonly EntityUid? Entity;
    public SpawnGhostForPlayerEvent(EntityUid? entity = null)
    {
        Entity = entity;
    }
}
