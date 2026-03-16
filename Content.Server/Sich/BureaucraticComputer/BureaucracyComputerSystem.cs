using Content.Server.Cargo.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Paper;
using Content.Shared.PDA;
using Content.Shared.Sich.BureaucraticComputer;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;


namespace Content.Server.Sich.BureaucraticComputer;

public sealed class BureaucracyComputerSystem : SharedBureacraticComputerSystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BureaucracyComputerComponent, InteractHandEvent>(OnInteractHand);

        Subs.BuiEvents<BureaucracyComputerComponent>(BureaucracyUiKey.Key, subs =>
        {
            subs.Event<BureaucracyPrintMessage>(OnPrintMessage);
        });
    }

    private void OnInteractHand(EntityUid uid, BureaucracyComputerComponent component, InteractHandEvent args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        // Збираємо дані для автозаповнення
        var stationName = "Невідома станція";
        if (_station.GetOwningStation(uid) is { } station)
            stationName = Name(station);

        var charName = Name(args.User);
        var charJob = "Невідома посада";

        // Шукаємо посаду в ID-картці або КПК
        if (_inventory.TryGetSlotEntity(args.User, "id", out var idEntity))
        {
            if (TryComp<PdaComponent>(idEntity, out var pda) && pda.ContainedId != null)
            {
                if (TryComp<IdCardComponent>(pda.ContainedId, out var pdaId))
                    charJob = pdaId.JobTitle ?? charJob;
            }
            else if (TryComp<IdCardComponent>(idEntity, out var id))
            {
                charJob = id.JobTitle ?? charJob;
            }
        }

        // Відкриваємо UI та надсилаємо стан
        _uiSystem.OpenUi(uid, BureaucracyUiKey.Key, actor.PlayerSession);

        var state = new BureaucracyAutoFillState(stationName, charName, charJob);
        _uiSystem.SetUiState(uid, BureaucracyUiKey.Key, state);
    }

    private void OnPrintMessage(EntityUid uid, BureaucracyComputerComponent component, BureaucracyPrintMessage args)
    {
        if (Timing.CurTime < component.NextPrintTime)
            return;

        if (_station.GetOwningStation(uid) is not { } station)
            return;

        var paper = Spawn(component.PaperId, Transform(uid).Coordinates);
        component.NextPrintTime = Timing.CurTime + component.PrintDelay;

        // Передаємо args повністю, щоб мати доступ до args.Fields
        SetupBountyLabel(paper, station, args);

        _audio.PlayPvs(component.PrintSound, uid);
    }

    public void SetupBountyLabel(EntityUid uid, EntityUid stationId, BureaucracyPrintMessage args, PaperComponent? paper = null)
    {
        var prototype = _prototypeManager.Index<BureaucraticDocumentPrototype>(args.PrototypeId);

        // Правильна перевірка наявності компонента (стандарт RobustToolbox)
        if (!Resolve(uid, ref paper, false))
            return;

        // Замінюємо теги на реальний текст з полів
        var finalString = prototype.Text;
        foreach (var (key, value) in args.Fields)
        {
            finalString = finalString.Replace($"[field={key}]", value);
        }

        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(finalString);
        _paperSystem.SetContent((uid, paper), msg.ToMarkup());
    }
}
