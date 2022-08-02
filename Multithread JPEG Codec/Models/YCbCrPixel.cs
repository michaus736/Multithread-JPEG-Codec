using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithread_JPEG_Codec.Models;

public class YCbCrPixel
{
    public float Y { get; set; }
    public float Cb { get; set; }
    public float Cr { get; set; }
    public override bool Equals(object? obj)
    {
        if (obj is not YCbCrPixel) return false;
        if (this is null || obj is null) return false;
        YCbCrPixel other = obj as YCbCrPixel;
        return this.Y == other.Y && this.Cb == other.Cb && this.Cr == other.Cr;
    }
    public override string ToString()
    {
        return String.Format("Y: {0}, Cb: {1}, Cr: {2}", this.Y, this.Cb, this.Cr);
    }
}
