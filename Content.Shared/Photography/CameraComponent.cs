using Robust.Shared.GameObjects;
using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Photography;

[RegisterComponent]
public sealed partial class CameraComponent : Component
{
    [DataField]
    public float ImageSize = 3f;
    [DataField]
    public int TargetWidth = 100;
}


[Serializable, NetSerializable]
public sealed class CameraPhotoCapturedEvent : EntityEventArgs
{
    public NetEntity CameraNetUid;
    public string GeneratedText;
    public bool IsHandled = false;

    public CameraPhotoCapturedEvent(NetEntity cameraUid, string text)
    {
        CameraNetUid = cameraUid;
        GeneratedText = text;
    }
}
