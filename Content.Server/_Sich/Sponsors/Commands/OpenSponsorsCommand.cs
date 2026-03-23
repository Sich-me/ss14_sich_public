using Content.Server.Administration;
using Content.Server.EUI;
using Content.Server.Sich.Sponsors.UI;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Sich.Sponsors.Commands
{
    [AdminCommand(AdminFlags.Permissions)]
    public sealed class OpenSponsorsCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly EuiManager _euiManager = default!;

        public override string Command => "sponsors";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine(Loc.GetString($"shell-cannot-run-command-from-server"));
                return;
            }

            var ui = new SponsorsAdminEui();
            _euiManager.OpenEui(ui, player);
        }
    }

    [AnyCommand]
    public sealed class OpenSponsorsWindowCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly EuiManager _euiManager = default!;

        public override string Command => "sponsorwindow";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine(Loc.GetString($"shell-cannot-run-command-from-server"));
                return;
            }

            var ui = new SponsorsEui();
            _euiManager.OpenEui(ui, player);
        }
    }
}
