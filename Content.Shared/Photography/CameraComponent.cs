using Content.Shared.Paper;
using Content.Shared.Paper; // Щоб взяти звідти PaperAction
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

using static Content.Shared.Paper.PaperComponent;

namespace Content.Shared.Photography;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CameraComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ImageSize = 3f;
    [DataField, AutoNetworkedField]
    public int TargetWidth = 3;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PhotographComponent : Component
{
    [DataField, AutoNetworkedField]
    public string ImageText = "";
}

[Serializable, NetSerializable]
public sealed class CameraPhotoCapturedEvent : HandledEntityEventArgs
{
    public NetEntity CameraNetUid;
    public string GeneratedText;

    public CameraPhotoCapturedEvent(NetEntity cameraUid, string text)
    {
        CameraNetUid = cameraUid;
        GeneratedText = text;
    }
}




[Serializable, NetSerializable]
public enum PolaroidUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PolaroidBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly string PhotoRichText;
    public readonly string CaptionText;
    public readonly PaperAction Mode;
    public readonly List<StampDisplayInfo> StampedBy;

    public PolaroidBoundUserInterfaceState(string photoRichText, string captionText, PaperAction mode, List<StampDisplayInfo> stampedBy)
    {
        PhotoRichText = photoRichText;
        CaptionText = captionText;
        Mode = mode;
        StampedBy = stampedBy;
    }
}
