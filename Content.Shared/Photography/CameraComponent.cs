using Robust.Shared.GameObjects;
using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;

namespace Content.Shared.Photography;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CameraComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ImageSize = 3f;
    [DataField, AutoNetworkedField]
    public int TargetWidth = 3;
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
