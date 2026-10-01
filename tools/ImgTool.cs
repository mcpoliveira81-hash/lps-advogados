using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class ImgTool
{
    static int Dist(Color a, Color b)
    {
        int dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
        return (int)Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    static bool IsSkin(Color p)
    {
        int r = p.R, g = p.G, b = p.B;
        if (r < 80) return false;
        if (r <= g) return false;
        if (r <= b) return false;
        if ((r - b) < 12) return false;
        if ((r - g) < 8) return false;
        return true;
    }

    // Detects skin-tone blobs (faces/hands) to help plan crop boxes.
    public static string Analyze(string src, int splitX)
    {
        Bitmap bmp = new Bitmap(src);
        int W = bmp.Width, H = bmp.Height;
        long sxL = 0, syL = 0, nL = 0, sxR = 0, syR = 0, nR = 0;
        int minXL = W, maxXL = 0, minYL = H, maxYL = 0;
        int minXR = W, maxXR = 0, minYR = H, maxYR = 0;
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                Color p = bmp.GetPixel(x, y);
                if (!IsSkin(p)) continue;
                if (x < splitX)
                {
                    sxL += x; syL += y; nL++;
                    if (x < minXL) minXL = x;
                    if (x > maxXL) maxXL = x;
                    if (y < minYL) minYL = y;
                    if (y > maxYL) maxYL = y;
                }
                else
                {
                    sxR += x; syR += y; nR++;
                    if (x < minXR) minXR = x;
                    if (x > maxXR) maxXR = x;
                    if (y < minYR) minYR = y;
                    if (y > maxYR) maxYR = y;
                }
            }
        }
        bmp.Dispose();
        return string.Format(
            "LEFT  n={0} center=({1},{2}) box=({3},{4})-({5},{6})\nRIGHT n={7} center=({8},{9}) box=({10},{11})-({12},{13})",
            nL, (nL > 0 ? sxL / nL : 0), (nL > 0 ? syL / nL : 0), minXL, minYL, maxXL, maxYL,
            nR, (nR > 0 ? sxR / nR : 0), (nR > 0 ? syR / nR : 0), minXR, minYR, maxXR, maxYR);
    }

    // Crops a region, resizes with high quality, applies mild sharpening, saves JPEG.
    public static string CropResize(string src, int cx, int cy, int cw, int ch, int outW, int outH, string dst, int quality)
    {
        using (Bitmap bmp = new Bitmap(src))
        {
            if (cx < 0) cx = 0;
            if (cy < 0) cy = 0;
            if (cx + cw > bmp.Width) cw = bmp.Width - cx;
            if (cy + ch > bmp.Height) ch = bmp.Height - cy;
            using (Bitmap crop = bmp.Clone(new Rectangle(cx, cy, cw, ch), bmp.PixelFormat))
            using (Bitmap outBmp = new Bitmap(outW, outH))
            {
                using (Graphics g = Graphics.FromImage(outBmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(crop, new Rectangle(0, 0, outW, outH));
                }
                Sharpen(outBmp, 0.35f);
                SaveJpeg(outBmp, dst, quality);
            }
        }
        return dst;
    }

    static void Sharpen(Bitmap bmp, float amount)
    {
        // 3x3 unsharp-style kernel: center boosted, neighbors subtracted.
        float[,] k = new float[,] {
            { 0, -amount, 0 },
            { -amount, 1 + 4 * amount, -amount },
            { 0, -amount, 0 }
        };
        Bitmap src = (Bitmap)bmp.Clone();
        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData d = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        BitmapData s = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        int stride = d.Stride;
        int w = bmp.Width * 3;
        unsafe
        {
            byte* p = (byte*)d.Scan0;
            byte* q = (byte*)s.Scan0;
            for (int y = 1; y < bmp.Height - 1; y++)
            {
                for (int x = 3; x < w - 3; x++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        float val =
                            k[1, 1] * q[y * stride + x + c] +
                            k[0, 1] * q[(y - 1) * stride + x + c] +
                            k[2, 1] * q[(y + 1) * stride + x + c] +
                            k[1, 0] * q[y * stride + x - 3 + c] +
                            k[1, 2] * q[y * stride + x + 3 + c];
                        if (val < 0) val = 0;
                        if (val > 255) val = 255;
                        p[y * stride + x + c] = (byte)val;
                    }
                }
            }
        }
        bmp.UnlockBits(d);
        src.UnlockBits(s);
        src.Dispose();
    }

    static void SaveJpeg(Bitmap bmp, string dst, int quality)
    {
        long q = quality;
        ImageCodecInfo enc = null;
        foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders())
            if (i.FormatID == ImageFormat.Jpeg.Guid) enc = i;
        using (EncoderParameters ep = new EncoderParameters(1))
        {
            ep.Param[0] = new EncoderParameter(Encoder.Quality, q);
            bmp.Save(dst, enc, ep);
        }
    }

    // Removes the navy background from the logo -> gold PNG (alpha) + navy PNG (alpha).
    public static string LogoAlpha(string src, string dstGold, string dstNavy)
    {
        Bitmap bmp = new Bitmap(src);
        int W = bmp.Width, H = bmp.Height;

        // Background color sampled from the top-left corner region.
        int br = 0, bg = 0, bb = 0, bn = 0;
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
            {
                Color c = bmp.GetPixel(x, y);
                br += c.R; bg += c.G; bb += c.B; bn++;
            }
        Color bgc = Color.FromArgb(br / bn, bg / bn, bb / bn);

        // Content mask per row/col, ignoring full-width frame lines.
        int[] rowCount = new int[H];
        int[] colCount = new int[W];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                if (Dist(bmp.GetPixel(x, y), bgc) > 45) { rowCount[y]++; colCount[x]++; }

        // Frame lines are full-width/full-height bars; drop them plus a small margin.
        bool[] badRow = new bool[H];
        bool[] badCol = new bool[W];
        for (int y = 0; y < H; y++)
            if (rowCount[y] > W * 0.6)
                for (int j = Math.Max(0, y - 6); j <= Math.Min(H - 1, y + 6); j++) badRow[j] = true;
        for (int x = 0; x < W; x++)
            if (colCount[x] > H * 0.6)
                for (int j = Math.Max(0, x - 6); j <= Math.Min(W - 1, x + 6); j++) badCol[j] = true;

        int minY = -1, maxY = -1, minX = -1, maxX = -1;
        for (int y = 0; y < H; y++)
            if (!badRow[y] && rowCount[y] > 0)
            {
                if (minY < 0) minY = y;
                maxY = y;
            }
        for (int x = 0; x < W; x++)
            if (!badCol[x] && colCount[x] > 0)
            {
                if (minX < 0) minX = x;
                maxX = x;
            }
        if (minY < 0) { minY = 0; maxY = H - 1; }
        if (minX < 0) { minX = 0; maxX = W - 1; }

        // Small padding around the mark.
        int pad = 4;
        minX = Math.Max(0, minX - pad);
        minY = Math.Max(0, minY - pad);
        maxX = Math.Min(W - 1, maxX + pad);
        maxY = Math.Min(H - 1, maxY + pad);
        int cw = maxX - minX + 1, ch = maxY - minY + 1;

        Bitmap gold = new Bitmap(cw, ch);
        Bitmap navy = new Bitmap(cw, ch);
        for (int y = 0; y < ch; y++)
        {
            for (int x = 0; x < cw; x++)
            {
                Color p = bmp.GetPixel(minX + x, minY + y);
                double d = Dist(p, bgc);
                double a = (d - 22.0) / (72.0 - 22.0);
                if (a < 0) a = 0;
                if (a > 1) a = 1;
                int R = p.R, G = p.G, B = p.B;
                if (a > 0.05 && a < 0.999)
                {
                    // Un-premultiply against the navy background to recover true color.
                    R = (int)((p.R - (1 - a) * bgc.R) / a);
                    G = (int)((p.G - (1 - a) * bgc.G) / a);
                    B = (int)((p.B - (1 - a) * bgc.B) / a);
                    if (R < 0) R = 0; if (R > 255) R = 255;
                    if (G < 0) G = 0; if (G > 255) G = 255;
                    if (B < 0) B = 0; if (B > 255) B = 255;
                }
                int alpha = (int)(a * 255);
                gold.SetPixel(x, y, Color.FromArgb(alpha, R, G, B));
                navy.SetPixel(x, y, Color.FromArgb(alpha, 0x0D, 0x22, 0x30));
            }
        }
        bmp.Dispose();
        gold.Save(dstGold, ImageFormat.Png);
        navy.Save(dstNavy, ImageFormat.Png);
        gold.Dispose();
        navy.Dispose();
        return dstGold + " | " + dstNavy;
    }

    // Square favicon tile: navy background + logo fitted with padding.
    public static string Favicon(string logo, string dst, int size)
    {
        using (Bitmap bmp = new Bitmap(size, size))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(0x0D, 0x22, 0x30));
                using (Bitmap l = new Bitmap(logo))
                {
                    float pad = size * 0.12f;
                    float maxW = size - 2 * pad;
                    float maxH = size - 2 * pad;
                    float scale = Math.Min(maxW / l.Width, maxH / l.Height);
                    int w = (int)(l.Width * scale), h = (int)(l.Height * scale);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(l, new Rectangle((size - w) / 2, (size - h) / 2, w, h));
                }
            }
            bmp.Save(dst, ImageFormat.Png);
        }
        return dst;
    }

    // Open Graph image: navy canvas, gold hairlines, logo, title text.
    public static string OgImage(string logo, string dst, int w, int h)
    {
        using (Bitmap bmp = new Bitmap(w, h))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(0x0D, 0x22, 0x30));
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (Pen gold = new Pen(Color.FromArgb(0xC9, 0xA4, 0x4C), 2))
                {
                    g.DrawLine(gold, 60, 56, w - 60, 56);
                    g.DrawLine(gold, 60, h - 56, w - 60, h - 56);
                }
                using (Bitmap l = new Bitmap(logo))
                {
                    float scale = Math.Min(560f / l.Width, 220f / l.Height);
                    int lw = (int)(l.Width * scale), lh = (int)(l.Height * scale);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(l, new Rectangle((w - lw) / 2, 150, lw, lh));
                }
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Near;
                using (Font f = new Font("Georgia", 30f, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Brush b = new SolidBrush(Color.FromArgb(0xE7, 0xDC, 0xC9)))
                {
                    g.DrawString("Direito com ética, acolhimento e presença real.", f, b,
                        new RectangleF(100, 400, w - 200, 80), sf);
                }
                using (Font f = new Font("Segoe UI", 22f, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Brush b = new SolidBrush(Color.FromArgb(0xC9, 0xA4, 0x4C)))
                {
                    g.DrawString("Santiago/RS  ·  Atendimento online e presencial", f, b,
                        new RectangleF(100, 470, w - 200, 60), sf);
                }
            }
            bmp.Save(dst, ImageFormat.Png);
        }
        return dst;
    }
}
