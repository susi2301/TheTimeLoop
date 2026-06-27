#ifndef RAYMARCH_FIRE_HLSL
#define RAYMARCH_FIRE_HLSL

#include "perlin_noise.hlsl"


// float GetDensityNoisySphere(float3 pos)
// {

// 	float noise_scale = 10.0f;
// 	float time = 1.0f;

//     //float _no = snoise(float4(pos *_NoiseScale , _Time.y * _NoiseSpeed))

//     float noise = clamp(snoise(float4(pos * noise_scale , time * _NoiseSpeed)),0,1);
//     float sphere = 1.0f - (length(pos) / 0.5f);
//     sphere = pow(sphere,_DensityFalloff);

//     return clamp(sphere-noise,0,1) * _NoiseIntensity;
// }

float inverse_lerp(float a, float b, float v) {
  return (v-a) / (b - a);
}

float remap(float iMin,float iMax,float oMin,float oMax, float v) {
  float t = inverse_lerp(iMin,iMax,v);
  return lerp(oMin,oMax,t);
}

void GetRayMarchDensity_float(float3 ray_origin_os, float3 ray_dir_os, float step_count, float step_length, float sphere_falloff, float noise_scale, float3 noise_offset, out float out_density) {
        
    uint STEPCOUNT = (uint)step_count;
    float STEPLENGTH = step_length/float(STEPCOUNT);

    float3 ray_step = ray_dir_os * STEPLENGTH;
    
    float density = 0.0;
    
    float3 pos = ray_origin_os;

    float3 center = float3(0.0f, 0.0f, 0.0f);
    float sphere_radius = 0.5f;

    for(uint i = 0; i < STEPCOUNT; i++)
    {
    	float p_noise1 = noise_perlin_3D((pos * noise_scale) + noise_offset, 2);
    	float p_noise2 = noise_perlin_3D(float3(p_noise1,p_noise1,p_noise1), 1);
    	float n = saturate(p_noise2 * 0.5f + 0.5f);

    	// float sphere = 1.0f - (length(pos) / 0.5f);
        // sphere = pow(sphere,1.0f);

        // n = clamp(sphere-n,0,1);
        float center_dist = distance(center.xyz,pos);
        float falloff = 1.0f - saturate(inverse_lerp(0.0f, sphere_radius, center_dist));

        density += n * pow(falloff, sphere_falloff);

        pos += ray_step;
    }

    out_density = density * STEPLENGTH;
}

void RaymarchVDBFire_float(UnityTexture2DArray flipbook_tex, float3 ray_origin_os, float3 ray_dir_os, float step_count, float step_length, float3 noise_offset, float noise_scale, float2 tile_count, float time, out float4 out_density) {
    
    uint num_frames = 64;

    uint frame0 = uint(time) % num_frames;
    uint frame1 = (uint(time) + 1) % num_frames;
    float frame_frac = frac(time);

    uint STEPCOUNT = (uint)step_count;
    float STEPLENGTH = step_length/float(STEPCOUNT);

    float3 ray_step = ray_dir_os * STEPLENGTH;
    
    float3 pos = ray_origin_os;

    float2 tile_size = 1.0f / tile_count;
    
    float num_tiles = tile_count.x * tile_count.y;

    // Effectivly we are sampling a voxel 3D texture where slices are layed out as flipbook style
    // thas why we must manually bilinear between slices.
    float4 frame_0_values = 0.0;
    float4 frame_1_values = 0.0;
    
    float vel_noise = 0.0f;

    //float noise_scale = 10.0f;

    for(uint i = 0; i < STEPCOUNT; i++) {

      float u =        clamp(pos.x, -0.495f, 0.495f) + 0.5f;
      float v = 1.0f - clamp(pos.y, -0.495f, 0.495f) + 0.5f;
      float w =        clamp(pos.z, -0.495f, 0.495f) + 0.5f;
      
      // uv inside one tile.
      float2 local_tile_offset = float2(u,v) * tile_size;

      float tile_nbr_f = w *  (num_tiles-1.0f);
      uint tile_nbr = (uint)(w *  (num_tiles-1));
      float fraction = tile_nbr_f - float(tile_nbr);

      uint col_1 = (tile_nbr + 0) % (uint)tile_count.x;
      uint col_2 = (tile_nbr + 1) % (uint)tile_count.x;
      
      uint row_1 = uint(tile_count.y) - ( (tile_nbr + 0) / (uint)tile_count.x) - 1;
      uint row_2 = uint(tile_count.y) - ( (tile_nbr + 1) / (uint)tile_count.x) - 1;

      float2 uv_0 = float2(col_1, row_1) * tile_size + local_tile_offset;
      float2 uv_1 = float2(col_2, row_2) * tile_size + local_tile_offset;

      // sample 2 frames
      float4 frame_0_value_0 = SAMPLE_TEXTURE2D_ARRAY_LOD(flipbook_tex.tex, flipbook_tex.samplerstate, uv_0, float(frame0) , 0);
      float4 frame_0_value_1 = SAMPLE_TEXTURE2D_ARRAY_LOD(flipbook_tex.tex, flipbook_tex.samplerstate, uv_1, float(frame0) , 0);
      
      float4 frame_1_value_0 = SAMPLE_TEXTURE2D_ARRAY_LOD(flipbook_tex.tex, flipbook_tex.samplerstate, uv_0, float(frame1) , 0);
      float4 frame_1_value_1 = SAMPLE_TEXTURE2D_ARRAY_LOD(flipbook_tex.tex, flipbook_tex.samplerstate, uv_1, float(frame1) , 0);

      frame_0_values += lerp(frame_0_value_0, frame_0_value_1, fraction);
      frame_1_values += lerp(frame_1_value_0, frame_1_value_1, fraction);

      float n = noise_perlin_3D((pos * noise_scale) + noise_offset , 42) * 0.5f + 0.5f;
      vel_noise += saturate(n);

      pos += ray_step;
    }

    vel_noise /= STEPCOUNT;
    out_density = lerp(frame_0_values, frame_1_values, frame_frac) / STEPCOUNT;
    out_density.a = vel_noise;
    //out_density = values / STEPCOUNT;// * STEPLENGTH;
}


#endif