using System.Text;

namespace GymBase.BLFile.BLProcessing;

public static class BlurhashEncoder
{
    private static readonly char[] Characters =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#$%*+,-.:;=?@[]^_{|}~".ToCharArray();

    public static string Encode(byte[] rgbPixels, int width, int height, int componentX = 4, int componentY = 3)
    {
        if (componentX < 1 || componentX > 9 || componentY < 1 || componentY > 9)
            throw new ArgumentException("Component counts must be between 1 and 9.");

        var factors = new (double R, double G, double B)[componentX * componentY];

        for (var j = 0; j < componentY; j++)
        {
            for (var i = 0; i < componentX; i++)
            {
                var normalisation = (i == 0 && j == 0) ? 1.0 : 2.0;
                factors[j * componentX + i] = MultiplyBasisFunction(rgbPixels, width, height, normalisation, i, j);
            }
        }

        var dc = factors[0];
        var acCount = componentX * componentY - 1;

        var sb = new StringBuilder();

        var sizeFlag = (componentX - 1) + (componentY - 1) * 9;
        sb.Append(Encode83(sizeFlag, 1));

        double maximumAcComponent;
        if (acCount > 0)
        {
            var actualMax = 0.0;
            for (var idx = 1; idx < factors.Length; idx++)
            {
                actualMax = Math.Max(actualMax, Math.Abs(factors[idx].R));
                actualMax = Math.Max(actualMax, Math.Abs(factors[idx].G));
                actualMax = Math.Max(actualMax, Math.Abs(factors[idx].B));
            }
            var quantisedMaxAc = (int)Math.Max(0, Math.Min(82, Math.Floor(actualMax * 166.0 - 0.5)));
            maximumAcComponent = (quantisedMaxAc + 1.0) / 166.0;
            sb.Append(Encode83(quantisedMaxAc, 1));
        }
        else
        {
            maximumAcComponent = 1.0;
            sb.Append(Encode83(0, 1));
        }

        sb.Append(Encode83(EncodeDc(dc), 4));

        for (var idx = 1; idx < factors.Length; idx++)
        {
            sb.Append(Encode83(EncodeAc(factors[idx], maximumAcComponent), 2));
        }

        return sb.ToString();
    }

    private static (double R, double G, double B) MultiplyBasisFunction(
        byte[] rgb, int width, int height, double normalisation, int compX, int compY)
    {
        double r = 0, g = 0, b = 0;
        var bytesPerPixel = 3;

        for (var y = 0; y < height; y++)
        {
            var cosY = Math.Cos(Math.PI * compY * y / height);
            for (var x = 0; x < width; x++)
            {
                var cosX = Math.Cos(Math.PI * compX * x / width);
                var basis = cosX * cosY;
                var pixelIndex = (y * width + x) * bytesPerPixel;

                var pr = SrgbToLinear(rgb[pixelIndex]);
                var pg = SrgbToLinear(rgb[pixelIndex + 1]);
                var pb = SrgbToLinear(rgb[pixelIndex + 2]);

                r += basis * pr;
                g += basis * pg;
                b += basis * pb;
            }
        }

        var scale = normalisation / (width * height);
        return (r * scale, g * scale, b * scale);
    }

    private static int EncodeDc((double R, double G, double B) value)
    {
        var r = LinearToSrgb(value.R);
        var g = LinearToSrgb(value.G);
        var b = LinearToSrgb(value.B);
        return (r << 16) + (g << 8) + b;
    }

    private static int EncodeAc((double R, double G, double B) value, double maxVal)
    {
        var quantR = (int)Math.Max(0, Math.Min(18, Math.Floor(SignPow(value.R / maxVal, 0.5) * 9.0 + 9.5)));
        var quantG = (int)Math.Max(0, Math.Min(18, Math.Floor(SignPow(value.G / maxVal, 0.5) * 9.0 + 9.5)));
        var quantB = (int)Math.Max(0, Math.Min(18, Math.Floor(SignPow(value.B / maxVal, 0.5) * 9.0 + 9.5)));
        return quantR * 19 * 19 + quantG * 19 + quantB;
    }

    private static string Encode83(long value, int length)
    {
        var buffer = new char[length];
        for (var i = 1; i <= length; i++)
        {
            var digit = (int)((value / (long)Math.Pow(83, length - i)) % 83);
            buffer[i - 1] = Characters[digit];
        }
        return new string(buffer);
    }

    private static double SignPow(double val, double exp) => Math.Sign(val) * Math.Pow(Math.Abs(val), exp);

    private static double SrgbToLinear(byte value)
    {
        var v = value / 255.0;
        return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
    }

    private static int LinearToSrgb(double value)
    {
        var v = Math.Max(0, Math.Min(1, value));
        var s = v <= 0.0031308 ? v * 12.92 : 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055;
        return (int)Math.Round(s * 255.0);
    }
}
