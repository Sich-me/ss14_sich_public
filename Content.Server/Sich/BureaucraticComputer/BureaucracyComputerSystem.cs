using Content.Shared.Interaction;
using Content.Shared.Sich.BureaucraticComputer; // Переконайся, що тут латинська 'C'
using Robust.Server.GameObjects; // Для UserInterfaceSystem та ActorComponent
using Robust.Shared.Player;

namespace Content.Server.Sich.BureaucraticComputer;

public sealed class BureaucracyComputerSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Підписуємось на подію взаємодії рукою (клік по об'єкту)
        SubscribeLocalEvent<BureaucracyComputerComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnInteractHand(EntityUid uid, BureaucracyComputerComponent component, InteractHandEvent args)
    {
        // Перевіряємо, чи є у користувача ActorComponent (чи це гравець)
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        // Намагаємось відкрити інтерфейс за ключем, визначеним у Shared
        _uiSystem.OpenUi(uid, BureaucracyUiKey.Key, actor.PlayerSession);
    }
}
