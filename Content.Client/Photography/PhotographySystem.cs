using Content.Client.Viewport;
using Content.Shared.Interaction;
using Content.Shared.Photography;
using Robust.Client.Graphics;
using Robust.Client.State;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Numerics;
using System.Text;

namespace Content.Client.Photography;

public sealed class PhotographySystem : EntitySystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CameraComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private async void OnAfterInteract(EntityUid uid, CameraComponent component, AfterInteractEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;

        if (!_timing.IsFirstTimePredicted || !args.ClickLocation.IsValid(EntityManager))
            return;

        CaptureWorldImage(uid, args.ClickLocation, component);
    }

    private void CaptureWorldImage(EntityUid cameraUid, EntityCoordinates targetCoords, CameraComponent camera)
    {
        if (_stateManager.CurrentState is not IMainViewportState state)
        {
            Logger.Error("Camera error: Current state is not IMainViewportState");
            return;
        }

        if (state.Viewport.Viewport is not ScalingViewport scalingViewport)
        {
            Logger.Error("Camera error: Viewport is not ScalingViewport");
            return;
        }

        var mapCoords = _transformSystem.ToMapCoordinates(targetCoords);

        var screenPos = scalingViewport.WorldToScreen(mapCoords.Position);

        Matrix3x2.Invert(scalingViewport.GetLocalToScreenMatrix(), out var invMatrix);

        var localPos = Vector2.Transform(screenPos, invMatrix);

        float zoom = _eyeManager.CurrentEye.Zoom.X;
        float renderScale = scalingViewport.CurrentRenderScale;
        float ppu = 32f;

        int boxSize = (int)((2 * ppu * renderScale) / zoom);

        int startX = Math.Max(0, (int)localPos.X - (boxSize / 2));
        int startY = Math.Max(0, (int)localPos.Y - (boxSize / 2));

        scalingViewport.Screenshot(worldImage =>
        {
            if (worldImage == null)
                return;

            string generatedRichText = ProcessImageToRichText(
                worldImage,
                cropX: startX,
                cropY: startY,
                cropWidth: boxSize,
                cropHeight: boxSize,
                targetWidth: camera.TargetWidth,
                fontSize: camera.ImageSize
            );

            var ev = new CameraPhotoCapturedEvent(GetNetEntity(cameraUid), generatedRichText);
            RaiseNetworkEvent(ev);
        });
    }

    private string ProcessImageToRichText(Image<Rgba32> image, int cropX, int cropY, int cropWidth, int cropHeight, int targetWidth, float fontSize)
    {
        using var ms = new MemoryStream();
        image.SaveAsBmp(ms);
        byte[] bmpBytes = ms.ToArray();

        int dataOffset = BitConverter.ToInt32(bmpBytes, 10);
        int imgWidth = BitConverter.ToInt32(bmpBytes, 18);
        int imgHeight = Math.Abs(BitConverter.ToInt32(bmpBytes, 22));
        short bpp = BitConverter.ToInt16(bmpBytes, 28);

        int bytesPerPixel = bpp / 8;
        int rowStride = ((imgWidth * bytesPerPixel) + 3) & ~3;

        cropX = Math.Clamp(cropX, 0, imgWidth - 1);
        cropY = Math.Clamp(cropY, 0, imgHeight - 1);
        cropWidth = Math.Clamp(cropWidth, 1, imgWidth - cropX);
        cropHeight = Math.Clamp(cropHeight, 1, imgHeight - cropY);

        int targetHeight = (int)(cropHeight * ((float)targetWidth / cropWidth));

        var sb = new StringBuilder();
        sb.AppendLine($"[font=\"Picture\" size={fontSize}]");

        for (int y = 0; y < targetHeight; y++)
        {
            int srcY = cropY + (y * cropHeight / targetHeight);
            int bmpY = imgHeight - 1 - srcY;

            int count = 0;
            string currentHex = "";

            for (int x = 0; x < targetWidth; x++)
            {
                int srcX = cropX + (x * cropWidth / targetWidth);
                int pixelIndex = dataOffset + (bmpY * rowStride) + (srcX * bytesPerPixel);

                byte b = bmpBytes[pixelIndex];
                byte g = bmpBytes[pixelIndex + 1];
                byte r = bmpBytes[pixelIndex + 2];

                string hexColor = $"{r:X2}{g:X2}{b:X2}";

                if (x == 0)
                {
                    currentHex = hexColor;
                    count = 1;
                }
                else if (hexColor == currentHex)
                {
                    count++;
                }
                else
                {
                    AppendColorBlock(sb, currentHex, count);
                    currentHex = hexColor;
                    count = 1;
                }
            }
            AppendColorBlock(sb, currentHex, count);
            sb.AppendLine();
        }

        sb.AppendLine("[/font]");
        return sb.ToString();
    }

    private void AppendColorBlock(StringBuilder sb, string hexColor, int count)
    {
        sb.Append($"[color=#{hexColor}]");
        sb.Append('0', count);
        sb.Append("[/color]");
    }
}
