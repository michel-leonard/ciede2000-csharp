// This function written in C# is not affiliated with the CIE (International Commission on Illumination),
// and is released into the public domain. It is provided "as is" without any warranty, express or implied.

// The classic CIE ΔE2000 implementation, which operates on two L*a*b* colors, and returns their difference.
// "l" ranges from 0 to 100, while "a" and "b" are unbounded and commonly clamped to the range of -128 to 127.
static double ciede2000(double l1, double a1, double b1, double l2, double a2, double b2, double kl = 1.0, double kc = 1.0, double kh = 1.0, bool canonical = false) {
	// Working in C# (.NET Core) with the CIEDE2000 color-difference formula.
	// kl, kc, kh are parametric factors to be adjusted according to
	// different viewing parameters such as textures, backgrounds...
	double n = (Math.Sqrt(a1 * a1 + b1 * b1) + Math.Sqrt(a2 * a2 + b2 * b2)) * 0.5;
	n = n * n * n * n * n * n * n;
	// A factor involving chroma raised to the power of 7 designed to make
	// the influence of chroma on the total color difference more accurate.
	n = 1.0 + 0.5 * (1.0 - Math.Sqrt(n / (n + 6103515625.0)));
	// Application of the chroma correction factor.
	double c1 = Math.Sqrt(a1 * a1 * n * n + b1 * b1);
	double c2 = Math.Sqrt(a2 * a2 * n * n + b2 * b2);
	// atan2 is preferred over atan because it accurately computes the angle of
	// a point (x, y) in all quadrants, handling the signs of both coordinates.
	double h1 = Math.Atan2(b1, a1 * n), h2 = Math.Atan2(b2, a2 * n);
	if (h1 < 0.0) h1 += 2.0 * Math.PI;
	if (h2 < 0.0) h2 += 2.0 * Math.PI;
	// When the hue angles lie in different quadrants, the straightforward
	// average can produce a mean that incorrectly suggests a hue angle in
	// the wrong quadrant, the next lines handle this issue.
	double h_mean = (h1 + h2) * 0.5, h_delta = (h2 - h1) * 0.5;
	// The part where most programmers get it wrong.
	if (Math.PI + 1E-14 < Math.Abs(h2 - h1)) {
		h_delta += Math.PI;
		if (canonical && Math.PI + 1E-14 < h_mean)
			// Sharma’s implementation, OpenJDK, ...
			h_mean -= Math.PI;
		else
			// Lindbloom’s implementation, Netflix’s VMAF, ...
			h_mean += Math.PI;
	}
	double p = 36.0 * h_mean - 55.0 * Math.PI;
	n = (c1 + c2) * 0.5;
	n = n * n * n * n * n * n * n;
	// The hue rotation correction term is designed to account for the
	// non-linear behavior of hue differences in the blue region.
	double r_t = -2.0 * Math.Sqrt(n / (n + 6103515625.0))
			* Math.Sin(Math.PI / 3.0 * Math.Exp(p * p / (-25.0 * Math.PI * Math.PI)));
	n = (l1 + l2) * 0.5;
	n = (n - 50.0) * (n - 50.0);
	// Lightness.
	double l = (l2 - l1) / (kl * (1.0 + 0.015 * n / Math.Sqrt(20.0 + n)));
	// These coefficients adjust the impact of different harmonic
	// components on the hue difference calculation.
	double t = 1.0	- 0.17 * Math.Sin(h_mean + Math.PI / 3.0)
			+ 0.24 * Math.Sin(2.0 * h_mean + Math.PI * 0.5)
			+ 0.32 * Math.Sin(3.0 * h_mean + 8.0 * Math.PI / 15.0)
			- 0.20 * Math.Sin(4.0 * h_mean + 3.0 * Math.PI / 20.0);
	n = c1 + c2;
	// Hue.
	double h = 2.0 * Math.Sqrt(c1 * c2) * Math.Sin(h_delta) / (kh * (1.0 + 0.0075 * n * t));
	// Chroma.
	double c = (c2 - c1) / (kc * (1.0 + 0.0225 * n));
	// The result reflects the actual geometric distance in the color space, given a tolerance of 3.6e-13.
	return Math.Sqrt(l * l + h * h + c * c + c * h * r_t);
}

// If you remove the constant 1E-14, the code will continue to work, but CIEDE2000
// interoperability between all programming languages will no longer be guaranteed.

// Source code tested by Michel LEONARD
// Website: ciede2000.pages-perso.free.fr
