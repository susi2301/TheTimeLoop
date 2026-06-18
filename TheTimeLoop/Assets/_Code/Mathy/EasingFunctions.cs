using System;

namespace Mathy {
    public enum EasingFunction {
        Linear = 0,
        InQuadratic,
        OutQuadratic,
        InOutQuadratic,
        InCubic,
        OutCubic,
        InOutCubic,
    }
    
    public static class EasingFunctions {
    
        public static float ease_float(float x, EasingFunction function) {
            switch (function) {
                case EasingFunction.Linear:         return x;
                case EasingFunction.InQuadratic:    return ease_in_quadratic_float(x);
                case EasingFunction.OutQuadratic:   return ease_out_quadratic_float(x);
                case EasingFunction.InOutQuadratic: return ease_in_out_quadratic_float(x);
                case EasingFunction.InCubic:        return ease_in_cubic_float(x);
                case EasingFunction.OutCubic:       return ease_out_cubic_float(x);
                case EasingFunction.InOutCubic:     return ease_in_out_cubic_float(x);
            }
    
            return x;
        }
        
        public static float ease_in_quadratic_float(float x) {
            return x * x;
        }
        public static float ease_out_quadratic_float(float x) {
            return 1.0f - (1.0f - x) * (1.0f - x);
        }
    
        public static float ease_in_out_quadratic_float(float x) {
            if (x < 0.0f) {
                return 2.0f * x * x;
            }
            
            return 1.0f - MathF.Pow(-2.0f * x + 2.0f, 2.0f) / 2.0f;
        }
        public static float ease_in_cubic_float(float x) {
            return x * x * x;
        }
        public static float ease_out_cubic_float(float x) {
            float one_minus_x = 1.0f - x;
            return 1.0f - (one_minus_x * one_minus_x * one_minus_x);
        }
        public static float ease_in_out_cubic_float(float x) {
            if (x < 0.5) {
                return 4.0f * x * x * x;
            }
            
            return 1.0f - MathF.Pow(-2.0f * x + 2.0f, 3.0f) / 2.0f;
        }
    }
}
