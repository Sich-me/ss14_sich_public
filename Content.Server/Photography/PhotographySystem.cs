using Content.Server.Paper; // Перевірте, чи такий using у вашому форку для PaperSystem
using Content.Shared.Paper;
using Content.Shared.Photography;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.Photography;

public sealed class PhotographySystem : EntitySystem
{
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CameraPhotoCapturedEvent>(OnPhotoCaptured);
    }

    private void OnPhotoCaptured(CameraPhotoCapturedEvent ev, EntitySessionEventArgs args)
    {
        if (ev.Handled)
            return;
        ev.Handled = true;

        var player = args.SenderSession.AttachedEntity;

        if (player == null)
            return;

        var cameraUid = GetEntity(ev.CameraNetUid);

        if (!Exists(cameraUid))
            return;

        var coords = Transform(player.Value).Coordinates;

        var photoEntity = Spawn("Paper", coords);

        // Змінюємо назву та опис створеного предмета
        _metaDataSystem.SetEntityName(photoEntity, "фотографія");
        _metaDataSystem.SetEntityDescription(photoEntity, "Маленьке піксельне фото, щойно видрукуване з камери.");

        if (TryComp<PaperComponent>(photoEntity, out var paperComp))
        {
            _paperSystem.SetContent(photoEntity, ev.GeneratedText);
        }
    }
}
