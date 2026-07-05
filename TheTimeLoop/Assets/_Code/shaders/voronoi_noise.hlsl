#ifndef NOISE_VORONOI_HLSL
#define NOISE_VORONOI_HLSL

// https://www.shadertoy.com/view/ldl3Dl
// The MIT License
// Copyright © 2013 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

float3 noise_voronoi_hash(float3 x ){
	x = float3( dot(x,float3(127.1f,311.7f, 74.7f)),dot(x,float3(269.5f,183.3f,246.1f)), dot(x,float3(113.5f,271.9f,124.6f)));

	return frac(sin(x)*43758.5453123f);
}

// float3 noise_voronoi_3D(float3 x) {

//     float3 p = floor(x);
//     float3 f = x - p;

// 	float id = 0.0f;
//     float2 res = float2(100.0f,100.0f);
    
//     for(int k=-1; k<=1; k++){
//         for(int j=-1; j<=1; j++){            
//             for(int i=-1; i<=1; i++){

//                 float3 b = float3(float(i), float(j), float(k) );
//                 float3 r = float3(b) - f + noise_voronoi_hash( p + b );
//                 float d = dot(r,r);

//                 if( d < res.x ) {
//         			id = dot( p+b, float3(1.0f,57.0f,113.0f));
//                     res = float2( d, res.x);			
//                 } else if( d < res.y ) {
//                     res.y = d;
//                 }
//             }
//         }
//     }

//     return float3(sqrt(res.x), sqrt(res.y), abs(id) );
// }

void noise_voronoi_3D_sg_float(float3 position, float scale, out float3 out_noise) {

    out_noise = position;

    float3 x = position * scale;

    float3 p = floor(x);
    float3 f = x - p;

    float id = 0.0f;
    float2 res = float2(100.0f,100.0f);
    
    // minkowski exponent
    const float e = 0.5f;
    
    for(int k=-1; k<=1; k++){
        for(int j=-1; j<=1; j++){            
            for(int i=-1; i<=1; i++){

                float3 b = float3(float(i), float(j), float(k) );
                float3 r = float3(b) - f + noise_voronoi_hash( p + b );
                
                // euclidian (squared) distance
                //float d = dot(r,r);

                // minkowski distance
                //float d = pow( pow(abs(r.x), e) + pow(abs(r.y), e) + pow(abs(r.z), e), 1.0f/e);
                
                // minkowski distance with exponent of 1.
                float3 rabs = abs(r);
                float d = rabs.x + rabs.y + rabs.z;

                if( d < res.x ) {
                    id = dot( p+b, float3(1.0f,57.0f,113.0f));
                    res = float2( d, res.x);            
                } else if( d < res.y ) {
                    res.y = d;
                }
            }
        }
    }

    out_noise = float3(sqrt(res.x), sqrt(res.y), abs(id) );
}


#endif // NOISE_VORONOI_HLSL