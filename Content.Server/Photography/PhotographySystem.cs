using Content.Shared.Paper;
using Content.Shared.Photography;
using Robust.Server.GameObjects;

namespace Content.Server.Photography;

public sealed class PhotographySystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CameraPhotoCapturedEvent>(OnPhotoCaptured);
        SubscribeLocalEvent<PhotographComponent, BoundUIOpenedEvent>(OnUIOpened);
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

        var photoEntity = Spawn("PalaroidPaper", coords);

        _metaDataSystem.SetEntityName(photoEntity, "фотографія");
        _metaDataSystem.SetEntityDescription(photoEntity, "Маленьке піксельне фото, щойно видрукуване з камери.");

        if (TryComp<PhotographComponent>(photoEntity, out var photoComp))
        {
            photoComp.ImageText = ev.GeneratedText;
            Dirty(photoEntity, photoComp);
        }
    }

    private void OnUIOpened(EntityUid uid, PhotographComponent photo, BoundUIOpenedEvent args)
    {
        // Перевіряємо, що відкривається саме наш полароїд (ключ з Shared)
        if (args.UiKey is not PolaroidUiKey.Key)
            return;

        UpdateUserInterface(uid, photo);
    }

    private void UpdateUserInterface(EntityUid uid, PhotographComponent photo)
    {
        if (!TryComp<PaperComponent>(uid, out var paper))
            return;

        // Збираємо наш кастомний стан
        var state = new PolaroidBoundUserInterfaceState(
            photo.ImageText,
            paper.Content,
            paper.Mode,
            paper.StampedBy
        );

        _uiSystem.SetUiState(uid, PolaroidUiKey.Key, state);
    }
}
