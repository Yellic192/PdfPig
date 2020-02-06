using System;
using UglyToad.PdfPig.Graphics.Colors;

namespace UglyToad.PdfPig.DocumentLayoutAnalysis
{
    internal static class ColorExtension
    {
        public static (double, double, double) ToXyzValues(this IColor color)
        {
            Func<double, double> adjust = (c) =>
            {
                if (c <= 0.04045)
                {
                    return c / 12.92;
                }
                else
                {
                    return Math.Pow((c + 0.055) / 1.055, 2.4);
                }
            };

            var rgb = color.ToRGBValues();
            double r = adjust((double)rgb.r);
            double g = adjust((double)rgb.g);
            double b = adjust((double)rgb.b);

            double x = 0.4124 * r + 0.3576 * g + 0.1805 * b;
            double y = 0.2126 * r + 0.7152 * g + 0.0722 * b;
            double z = 0.0193 * r + 0.1192 * g + 0.9505 * b;

            return (x * 100.0, y * 100.0, z * 100.0);
        }

        public static (double, double, double) ToLabValues(this IColor color)
        {
            Func<double, double> f = (t) =>
            {
                if (t > 0.008856)
                {
                    return Math.Pow(t, 1.0 / 3.0);
                }
                else
                {
                    return (7.787 * t) + (16.0 / 116.0);
                }
            };

            var xyz = color.ToXyzValues();
            double fY = f(xyz.Item2 / 100.0);
            double Lstar = 116.0 * fY - 16.0;
            double astar = 500.0 * (f(xyz.Item1 / 95.047) - fY);
            double bstar = 200.0 * (fY - f(xyz.Item3 / 108.883));
            return (Lstar, astar, bstar);
        }

        public static (double, double, double) ToLChValues(this IColor color)
        {
            var lab = color.ToLabValues();
            var C = Math.Sqrt(lab.Item2 * lab.Item2 + lab.Item3 * lab.Item3);
            var h = Math.Atan2(lab.Item3, lab.Item2) * 180.0 / Math.PI;
            if (h < 0) h += 360.0;
            return (lab.Item1, C, h);
        }

        public static double Ciede2000Distance(double L1, double a1, double b1, double L2, double a2, double b2)
        {
            if (L1.Equals(L2) && a1.Equals(a2) && b1.Equals(b2)) return 0.0;

            // http://www.brucelindbloom.com/index.html?Eqn_DeltaE_CIE2000.html
            double tau = 6.28318530717959;
            var TwentyFivePower7 = 6_103_515_625;

            var C1 = Math.Sqrt(a1 * a1 + b1 * b1);
            var C2 = Math.Sqrt(a2 * a2 + b2 * b2);

            var LbarPrime = (L1 + L2) / 2.0;
            var Cbar = (C1 + C2) / 2.0;
            var CbarPower7 = Cbar * Cbar * Cbar * Cbar * Cbar * Cbar * Cbar;
            var GPlus1 = 0.5 * (1.0 - Math.Sqrt(CbarPower7 / (CbarPower7 + TwentyFivePower7))) + 1.0;

            var aPrime1 = a1 * GPlus1;
            var aPrime2 = a2 * GPlus1;

            var CPrime1 = Math.Sqrt(aPrime1 * aPrime1 + b1 * b1);
            var CPrime2 = Math.Sqrt(aPrime2 * aPrime2 + b2 * b2);
            var CBarPrime = (CPrime1 + CPrime2) / 2.0;

            var hPrime1 = Math.Atan2(b1, aPrime1);
            if (hPrime1 < 0) hPrime1 += tau;

            var hPrime2 = Math.Atan2(b2, aPrime2);
            if (hPrime2 < 0) hPrime2 += tau;

            var hPrime2MinushPrime1 = hPrime2 - hPrime1;
            var hPrime2MinushPrime1Abs = Math.Abs(hPrime2MinushPrime1);

            double HHatPrime;
            if (hPrime2MinushPrime1Abs > Math.PI)
            {
                HHatPrime = (hPrime1 + hPrime2 + tau) / 2.0;
            }
            else
            {
                HHatPrime = (hPrime1 + hPrime2) / 2.0;
            }

            var T = 1.0 - 0.17 * Math.Cos(HHatPrime - 0.5235988)
                        + 0.24 * Math.Cos(2.0 * HHatPrime)
                        + 0.32 * Math.Cos(3.0 * HHatPrime + 0.1047198)
                        - 0.20 * Math.Cos(4.0 * HHatPrime - 1.099557);

            double deltahPrime;
            if (hPrime2MinushPrime1Abs <= Math.PI)
            {
                deltahPrime = hPrime2MinushPrime1;
            }
            else if (hPrime2MinushPrime1Abs > Math.PI && hPrime2 < hPrime1)
            {
                deltahPrime = hPrime2MinushPrime1 + tau;
            }
            else
            {
                deltahPrime = hPrime2MinushPrime1 - tau;
            }

            var deltaLPrime = L2 - L1;
            var deltaCPrime = CPrime2 - CPrime1;

            var deltaHPrime = 2.0 * Math.Sqrt(CPrime1 * CPrime2) * Math.Sin(deltahPrime / 2.0);

            var LbarPrimeMinus50 = LbarPrime - 50.0;
            var LbarPrimeMinus50Squared = LbarPrimeMinus50 * LbarPrimeMinus50;

            var SL = 1.0 + 0.015 * LbarPrimeMinus50Squared / Math.Sqrt(20.0 + LbarPrimeMinus50Squared);
            var SC = 1.0 + 0.045 * CBarPrime;
            var SH = 1.0 + 0.015 * CBarPrime * T;
            double HHatPrimeSquared = (HHatPrime - 4.799655) / 0.4363323;
            HHatPrimeSquared *= HHatPrimeSquared;

            var deltaTheta = 1.0471976 * Math.Exp(-HHatPrimeSquared);
            var CBarPrimePower7 = CBarPrime * CBarPrime * CBarPrime * CBarPrime * CBarPrime * CBarPrime * CBarPrime;
            var RC = 2.0 * Math.Sqrt(CBarPrimePower7 / (CBarPrimePower7 + TwentyFivePower7));
            var RT = -RC * Math.Sin(deltaTheta);

            var deltaLPrimeOverKlSl = deltaLPrime / SL;
            var deltaCPrimeOverKcSc = deltaCPrime / SC;
            var deltaHPrimeOverKhSh = deltaHPrime / SH;

            var deltaE = Math.Sqrt(deltaLPrimeOverKlSl * deltaLPrimeOverKlSl
                                 + deltaCPrimeOverKcSc * deltaCPrimeOverKcSc
                                 + deltaHPrimeOverKhSh * deltaHPrimeOverKhSh
                                 + RT * deltaCPrimeOverKcSc * deltaHPrimeOverKhSh);

            return deltaE;
        }
    }
}
