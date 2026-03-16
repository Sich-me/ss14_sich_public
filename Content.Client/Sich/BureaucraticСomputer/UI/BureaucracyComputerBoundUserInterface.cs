using Content.Client.Sich.BureaucraticComputer.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Client.Sich.BureaucraticСomputer.UI;

public sealed class BureaucracyComputerBoundUserInterface : BoundUserInterface
{
    private BureaucracyComputerWindow? _window;

    public BureaucracyComputerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _window = new BureaucracyComputerWindow();
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Dispose();
    }
}
