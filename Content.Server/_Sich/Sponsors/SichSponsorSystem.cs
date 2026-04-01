using Content.Server.EUI;
using Content.Server.Sich.Sponsors.UI;
using Content.Server.StationRecords;
using Content.Shared.CCVar;
using Content.Shared.CrewManifest;
using Content.Shared.Sich.Sponsors;
using Robust.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.Sich.Sponsors;
public sealed partial class SichSponsorSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _euiManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RequestSponsorWindowMessage>(OnRequestSponsorWindowMessage);
    }

    private void OnRequestSponsorWindowMessage(RequestSponsorWindowMessage ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession is not { } sessionCast)
        {
            return;
        }

        OpenEui(sessionCast);
    }

    public void OpenEui(ICommonSession session)
    {
        var eui = new SponsorsEui();
        _euiManager.OpenEui(eui, session);
        eui.StateDirty();
    }
}
