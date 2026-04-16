Browse : [TypeScript](https://github.com/michel-leonard/ciede2000-typescript) · [VBA](https://github.com/michel-leonard/ciede2000-vba) · [Wolfram Language](https://github.com/michel-leonard/ciede2000-wolfram-language) · [AWK](https://github.com/michel-leonard/ciede2000-awk) · [BC](https://github.com/michel-leonard/ciede2000-basic-calculator) · **C#** · [C++](https://github.com/michel-leonard/ciede2000-cpp) · [C99](https://github.com/michel-leonard/ciede2000-c) · [Dart](https://github.com/michel-leonard/ciede2000-dart) · [Go](https://github.com/michel-leonard/ciede2000-go) · [JavaScript](https://github.com/michel-leonard/ciede2000-javascript)

# CIEDE2000 color difference formula in C#

This page presents the CIEDE2000 color difference, implemented in the C# programming language.

![Logo](https://raw.githubusercontent.com/michel-leonard/ciede2000-color-matching/refs/heads/main/docs/assets/images/logo.jpg)

## About

Here you’ll find the first rigorously correct implementation of CIEDE2000 that doesn’t use any conversion between degrees and radians. Set parameter `canonical` to obtain results in line with your existing pipeline.

`canonical`|The algorithm operates...|
|:--:|-|
`false`|in accordance with the CIEDE2000 values currently used by many industry players|
`true`|in accordance with the CIEDE2000 values provided by [this](https://hajim.rochester.edu/ece/sites/gsharma/ciede2000/) academic MATLAB function|

## Our CIEDE2000 offer

This production-ready file, released in 2026, contain the CIEDE2000 algorithm.

Source File|Type|Bits|Purpose|Advantage|
|:--:|:--:|:--:|:--:|:--:|
[ciede2000.cs](./ciede2000.cs)|`double`|64|General|Interoperability|

### Software Versions

- C# 4.0 / .NET Framework 4.0
- C# 12.0 / .NET Framework 8.0 (LTS)

### Example Usage

We calculate the CIEDE2000 distance between two colors, first without and then with parametric factors.

```csharp
static void Main() {
	// Example of two L*a*b* colors
	double l1 = 95.3,  a1 = 39.2,  b1 = -1.7;
	double l2 = 94.8,  a2 = 45.0,  b2 = 2.1;

	double delta_e = ciede2000(l1, a1, b1, l2, a2, b2);
	Console.WriteLine("CIEDE2000 = " + delta_e); // ΔE2000 = 2.8916930349364693

	// Example of parametric factors used in the textile industry
	double kl = 2.0, kc = 1.0, kh = 1.0;

	// Perform a CIEDE2000 calculation compliant with that of Gaurav Sharma
	bool canonical = true;

	delta_e = ciede2000(l1, a1, b1, l2, a2, b2, kl, kc, kh, canonical);
	Console.WriteLine("CIEDE2000 = " + delta_e); // ΔE2000 = 2.880092668545492
}
```

These CIEDE2000 calculations in C# are fast, typically allowing millions of color comparisons per second.

### Test Results

LEONARD’s tests are based on well-chosen L\*a\*b\* colors, with various parametric factors `kL`, `kC` and `kH`.

```
CIEDE2000 Verification Summary :
          Compliance : [X] CANONICAL [ ] SIMPLIFIED
  First Checked Line : 60.0,96.000009,-128.0,50.0,24.0,32.0,1.0,1.0,1.0,49.09437103982911
           Precision : 12 decimal digits
           Successes : 100000000
               Error : 0
            Duration : 370.99 seconds
     Average Delta E : 67.12
   Average Deviation : 4.7e-15
   Maximum Deviation : 1.4e-13
```

```
CIEDE2000 Verification Summary :
          Compliance : [ ] CANONICAL [X] SIMPLIFIED
  First Checked Line : 60.0,96.000009,-128.0,50.0,24.0,32.0,1.0,1.0,1.0,49.09456171778273
           Precision : 12 decimal digits
           Successes : 100000000
               Error : 0
            Duration : 365.92 seconds
     Average Delta E : 67.12
   Average Deviation : 4.2e-15
   Maximum Deviation : 1.4e-13
```

## Public Domain Licence

You are free to use these files, even for commercial purposes.
