
namespace Flcrm {
	public static partial class Mathy {

		public static float inv_lerp(float a, float b, float v) {
			return (v-a) / (b - a);
		}

		public static float lerp(float a, float b, float t) {
			return (1.0f - t) * a + b * t;
		}

		public static float remap(float i_min, float i_max, float o_min, float o_max, float value) {
			float range_01 = inv_lerp(i_min, i_max, value);
			float v = lerp(o_min, o_max, range_01);
			return v;
		}
	}
}