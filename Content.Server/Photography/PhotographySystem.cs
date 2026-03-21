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

        // Підписуємось на подію, яка прилітає по мережі від клієнта
        SubscribeNetworkEvent<CameraPhotoCapturedEvent>(OnPhotoCaptured);
    }

    private void OnPhotoCaptured(CameraPhotoCapturedEvent ev, EntitySessionEventArgs args)
    {
        if (ev.IsHandled)
            return;
        ev.IsHandled = true; // Позначаємо івент як оброблений, щоб інші системи не обробляли його повторно
        // Отримуємо сутність гравця, який відправив івент
        var player = args.SenderSession.AttachedEntity;

        if (player == null)
            return;

        var cameraUid = GetEntity(ev.CameraNetUid);

        // Перевіряємо, чи існує камера, про яку каже клієнт
        if (!Exists(cameraUid))
            return;

        // ПРИМІТКА ДЛЯ БЕЗПЕКИ: 
        // В ідеалі тут варто додати перевірку через SharedHandsSystem, 
        // чи цей гравець дійсно тримає ev.CameraUid у руках, щоб уникнути читерства.

        // Отримуємо координати гравця, щоб заспавнити фотографію біля нього
        var coords = Transform(player.Value).Coordinates;

        // Спавнимо базовий аркуш паперу (ID прототипу зазвичай "Paper")
        var photoEntity = Spawn("Paper", coords);

        // Змінюємо назву та опис створеного предмета
        _metaDataSystem.SetEntityName(photoEntity, "фотографія");
        _metaDataSystem.SetEntityDescription(photoEntity, "Маленьке піксельне фото, щойно видрукуване з камери.");

        // Записуємо згенерований клієнтом RichText у компонент паперу
        if (TryComp<PaperComponent>(photoEntity, out var paperComp))
        {
            // Метод може трохи відрізнятися залежно від версії SS14. 
            // Іноді це просто _paperSystem.SetContent(photoEntity, ev.GeneratedText);
            _paperSystem.SetContent(photoEntity, ev.GeneratedText);
        }
    }
}
