// This function written in C# is not affiliated with the CIE (International Commission on Illumination),
// and is released into the public domain. It is provided "as is" without any warranty, express or implied.

// The functional CIE ΔE2000 implementation, which operates on two L*a*b* colors, and returns their difference.
// "l" ranges from 0 to 100, while "a" and "b" are unbounded and commonly clamped to the range of -128 to 127.
static float ciede2000(float l1, float a1, float b1, float l2, float a2, float b2, float kl = 1.0f, float kc = 1.0f, float kh = 1.0f, bool canonical = false) {
	// Working in C# with the CIEDE2000 color-difference formula.
	// kl, kc, kh are parametric factors to be adjusted according to
	// different viewing parameters such as textures, backgrounds...

	const float pi_1 = 3.14159265358979323846f;
	const float pi_3 = 1.04719755119659774615f;

	// 1. Compute chroma magnitudes ... a and b usually range from -128 to +127
	float a1_sq = a1 * a1;
	float b1_sq = b1 * b1;
	float c1_orig = MathF.Sqrt(a1_sq + b1_sq);

	float a2_sq = a2 * a2;
	float b2_sq = b2 * b2;
	float c2_orig = MathF.Sqrt(a2_sq + b2_sq);

	// 2. Compute chroma mean and apply G compensation
	float c_avg = 0.5f * (c1_orig + c2_orig);
	float c_avg_3 = c_avg * c_avg * c_avg;
	float c_avg_7 = c_avg_3 * c_avg_3 * c_avg;
	float g_denom = c_avg_7 + 6103515625.0f;
	float g_ratio = c_avg_7 / g_denom;
	float g_sqrt = MathF.Sqrt(g_ratio);
	float g_factor = 1.0f + 0.5f * (1.0f - g_sqrt);

	// 3. Apply G correction to a components, compute corrected chroma
	float a1_prime = a1 * g_factor;
	float c1_prime_sq = a1_prime * a1_prime + b1 * b1;
	float c1_prime = MathF.Sqrt(c1_prime_sq);
	float a2_prime = a2 * g_factor;
	float c2_prime_sq = a2_prime * a2_prime + b2 * b2;
	float c2_prime = MathF.Sqrt(c2_prime_sq);

	// 4. Compute hue angles in radians, adjust for negatives and wrap
	float h1_raw = MathF.Atan2(b1, a1_prime);
	float h2_raw = MathF.Atan2(b2, a2_prime);
	float h1_adj = h1_raw + ((h1_raw < 0.0f) ? 1.0f : 0.0f) * 2.0f * pi_1;
	float h2_adj = h2_raw + ((h2_raw < 0.0f) ? 1.0f : 0.0f) * 2.0f * pi_1;
	float delta_h = MathF.Abs(h1_adj - h2_adj);
	float h_mean_raw = 0.5f * (h1_adj + h2_adj);
	float h_diff_raw = 0.5f * (h2_adj - h1_adj);

	// Check if hue mean wraps around pi (180 deg)
	float hue_wrap = (pi_1 + 0.000001f < delta_h) ? 1.0f : 0.0f;

	// The part where most programmers get it wrong
	float h_mean = h_mean_raw - ((canonical && pi_1 + 0.000001f < h_mean_raw) ?
		1.0f : // canonical=true acts like Gaurav Sharma, OpenJDK, ...
		-1.0f  // canonical=false acts like Bruce Lindbloom, Netflix’s VMAF, ...
	) * hue_wrap * pi_1;

	// Michel Leonard 2026 - When mean wraps, difference wraps too
	float h_diff = h_diff_raw + hue_wrap * pi_1;

	// 5. Compute hue rotation correction factor R_T
	float c_bar = 0.5f * (c1_prime + c2_prime);
	float c_bar_3 = c_bar * c_bar * c_bar;
	float c_bar_7 = c_bar_3 * c_bar_3 * c_bar;
	float rc_denom = c_bar_7 + 6103515625.0f;
	float R_C = MathF.Sqrt(c_bar_7 / rc_denom);

	float theta = 36.0f * h_mean - 55.0f * pi_1;
	float theta_denom = -25.0f * pi_1 * pi_1;
	float exp_argument = theta * theta / theta_denom;
	float exp_term = MathF.Exp(exp_argument);
	float delta_theta = pi_3 * exp_term;
	float sin_term = MathF.Sin(delta_theta);

	// Rotation factor ... cross-effect between chroma and hue
	float R_T = -2.0f * R_C * sin_term;

	// 6. Compute lightness term ... L nominally ranges from 0 to 100
	float l_avg = 0.5f * (l1 + l2);
	float l_delta_sq = (l_avg - 50.0f) * (l_avg - 50.0f);
	float l_delta = l2 - l1;

	// Adaptation to the non-linearity of light perception ... S_L
	float sl_num = 0.015f * l_delta_sq;
	float sl_denom = MathF.Sqrt(20.0f + l_delta_sq);
	float S_L = 1.0f + sl_num / sl_denom;
	float L_term = l_delta / (kl * S_L);

	// 7. Compute chroma-related trig terms and factor T
	float trig_1 = 0.17f * MathF.Sin(h_mean + pi_3);
	float trig_2 = 0.24f * MathF.Sin(2.0f * h_mean + 0.5f * pi_1);
	float trig_3 = 0.32f * MathF.Sin(3.0f * h_mean + 1.6f * pi_3);
	float trig_4 = 0.2f * MathF.Sin(4.0f * h_mean + 0.15f * pi_1);
	float T = 1.0f - trig_1 + trig_2 + trig_3 - trig_4;

	float c_sum = c1_prime + c2_prime;
	float c_product = c1_prime * c2_prime;
	float c_geo_mean = MathF.Sqrt(c_product);

	// 8. Compute hue difference and scaling factor S_H
	float sin_h_diff = MathF.Sin(h_diff);
	float S_H = 1.0f + 0.0075f * c_sum * T;
	float H_term = 2.0f * c_geo_mean * sin_h_diff / (kh * S_H);

	// 9. Compute chroma difference and scaling factor S_C
	float c_delta = c2_prime - c1_prime;
	float S_C = 1.0f + 0.0225f * c_sum;
	float C_term = c_delta / (kc * S_C);

	// 10. Combine lightness, chroma, hue, and interaction terms
	float L_part = L_term * L_term;
	float C_part = C_term * C_term;
	float H_part = H_term * H_term;
	float interaction = C_term * H_term * R_T;
	float delta_e_squared = L_part + C_part + H_part + interaction;
	float delta_e = MathF.Sqrt(delta_e_squared);

	return delta_e; // Given a tolerance of 2.7e-4
}

// If you remove the constant 0.000001f, the code will continue to work, but CIEDE2000
// interoperability between all programming languages will no longer be guaranteed.

// Source code tested by Michel LEONARD
// Website: ciede2000.pages-perso.free.fr
