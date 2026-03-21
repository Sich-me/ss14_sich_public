using System.IO; // Додаємо для MemoryStream
using System.Text;
using System.Threading.Tasks;
using Content.Shared.Interaction;
using Content.Shared.Photography;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client.Photography;

public sealed class PhotographySystem : EntitySystem
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

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

        if (!args.ClickLocation.IsValid(EntityManager))
            return;

        await CaptureWorldImage(uid, args.ClickLocation, component);
    }

    private async Task CaptureWorldImage(EntityUid cameraUid, EntityCoordinates targetCoords, CameraComponent camera)
    {
        var mapCoords = _transformSystem.ToMapCoordinates(targetCoords);
        var screenPos = _eyeManager.WorldToScreen(mapCoords.Position);

        float zoom = _eyeManager.CurrentEye.Zoom.X;
        float ppu = 32f;
        int boxSize = (int)((3 * ppu) / zoom);

        int startX = Math.Max(0, (int)screenPos.X - (boxSize / 2));
        int startY = Math.Max(0, (int)screenPos.Y - (boxSize / 2));

        var subRegion = UIBox2i.FromDimensions(startX, startY, boxSize, boxSize);

        using var worldImage = await _clyde.ScreenshotAsync(ScreenshotType.Final, subRegion);

        if (worldImage == null)
            return;

        string generatedRichText = ProcessImageToRichText(worldImage, targetWidth: camera.TargetWidth, fontSize: camera.ImageSize);

        RaiseNetworkEvent(new CameraPhotoCapturedEvent(GetNetEntity(cameraUid), generatedRichText));
    }

    private string ProcessImageToRichText(Image<Rgb24> image, int targetWidth, float fontSize)
    {
        using var ms = new MemoryStream();
        image.SaveAsBmp(ms);
        byte[] bmpBytes = ms.ToArray();

        int dataOffset = BitConverter.ToInt32(bmpBytes, 10);
        int width = BitConverter.ToInt32(bmpBytes, 18);
        int height = Math.Abs(BitConverter.ToInt32(bmpBytes, 22));
        short bpp = BitConverter.ToInt16(bmpBytes, 28);

        int bytesPerPixel = bpp / 8;
        int rowStride = ((width * bytesPerPixel) + 3) & ~3;

        int targetHeight = (int)(height * ((float)targetWidth / width));

        var sb = new StringBuilder();
        sb.AppendLine($"[font=\"Picture\" size={fontSize}]");

        for (int y = 0; y < targetHeight; y++)
        {
            int srcY = y * height / targetHeight;

            int bmpY = height - 1 - srcY;

            int count = 0;
            string currentHex = "";

            for (int x = 0; x < targetWidth; x++)
            {
                int srcX = x * width / targetWidth;

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
