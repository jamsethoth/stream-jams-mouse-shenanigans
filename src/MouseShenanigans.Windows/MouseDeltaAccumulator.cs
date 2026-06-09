using MouseShenanigans.Core;

namespace MouseShenanigans.Windows;

public sealed class MouseDeltaAccumulator
{
    private double remainderX;
    private double remainderY;

    public IntegerMouseDelta Convert(RemappedMouseDelta delta)
    {
        double totalX = remainderX + delta.Dx;
        double totalY = remainderY + delta.Dy;

        int wholeX = (int)Math.Truncate(totalX);
        int wholeY = (int)Math.Truncate(totalY);

        remainderX = totalX - wholeX;
        remainderY = totalY - wholeY;

        return new IntegerMouseDelta(wholeX, wholeY);
    }

    public void Reset()
    {
        remainderX = 0;
        remainderY = 0;
    }
}
