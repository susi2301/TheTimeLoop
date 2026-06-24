#ifndef NOISE_PERLIN_GLSL
#define NOISE_PERLIN_GLSL

// Perlin Noise by:
// https://www.shadertoy.com/view/slB3z3

// fast high-quality hash https://www.shadertoy.com/view/wfVczm
uint noise_perlin_hash(uint3 key, uint seed) { 
    uint3 k = key;
    k *= 0x27d4eb2fu; 
    k ^= k >> 16;
    k *= 0x85ebca77u; 
    uint h = seed;
    h ^= k.x;
    h ^= h >> 16;
    h *= 0x9e3779b1u;
    h ^= k.y;
    h ^= h >> 16;
    h *= 0x9e3779b1u;
    h ^= k.z;
    h ^= h >> 16;
    h *= 0x9e3779b1u;
    h ^= h >> 16;
    h *= 0xed5ad4bbu;
    h ^= h >> 16;
    return h;
}

// generates a distinct seed for each octave
// that will behave like a 4th coordinate  
// when mixed into the final hash
uint noise_perlin_hash(uint key, uint seed) {
    uint k = key;
    k *= 0x27d4eb2fu; 
    k ^= k >> 16;
    k *= 0x85ebca77u; 
    uint h = seed;
    h ^= k;
    h ^= h >> 16;
    h *= 0x9e3779b1u;
    return h;
}

float3 noise_perlin_gradient(uint h)
{
    const float3 gradients[12] = {
        float3(1, 1, 0), float3(-1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0),
        float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
        float3(0, 1, 1), float3(0, -1, 1), float3(0, 1, -1), float3(0, -1, -1)
    }; 
    return gradients[int(h % 12u)];
}

float3 noise_perlin_fade(float3 t) {
    // 6t^5 - 15t^4 + 10t^3
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

float noise_perlin_interpolate_triliniear(float value1, float value2, float value3, float value4, float value5, float value6, float value7, float value8, float3 t) {
    return lerp(
        lerp(lerp(value1, value2, t.x), lerp(value3, value4, t.x), t.y),
        lerp(lerp(value5, value6, t.x), lerp(value7, value8, t.x), t.y),
        t.z
    );
}


float noise_perlin_3D(float3 position, uint seed) {
    float3 floorPosition = floor(position);
    float3 fractPosition = position - floorPosition;
    uint3 cellCoordinates = uint3(int3(floorPosition));
    float value1 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates, seed)), fractPosition);
    float value2 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 0, 0), seed)), fractPosition - float3(1, 0, 0));
    float value3 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(0, 1, 0), seed)), fractPosition - float3(0, 1, 0));
    float value4 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 1, 0), seed)), fractPosition - float3(1, 1, 0));
    float value5 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(0, 0, 1), seed)), fractPosition - float3(0, 0, 1));
    float value6 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 0, 1), seed)), fractPosition - float3(1, 0, 1));
    float value7 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(0, 1, 1), seed)), fractPosition - float3(0, 1, 1));
    float value8 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 1, 1), seed)), fractPosition - float3(1, 1, 1));
    return noise_perlin_interpolate_triliniear(value1, value2, value3, value4, value5, value6, value7, value8, noise_perlin_fade(fractPosition));
}

void noise_perlin_3D_simp_float(float3 position, float scale, float seedf, out float noise) {
    
    uint seed = (uint)seedf;
    float3 floorPosition = floor(position * scale);
    float3 fractPosition = (position * scale) - floorPosition;
    uint3 cellCoordinates = uint3(int3(floorPosition));
    float value1 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates, seed)), fractPosition);
    float value2 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 0, 0), seed)), fractPosition - float3(1, 0, 0));
    float value3 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(0, 1, 0), seed)), fractPosition - float3(0, 1, 0));
    float value4 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 1, 0), seed)), fractPosition - float3(1, 1, 0));
    float value5 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(0, 0, 1), seed)), fractPosition - float3(0, 0, 1));
    float value6 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 0, 1), seed)), fractPosition - float3(1, 0, 1));
    float value7 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(0, 1, 1), seed)), fractPosition - float3(0, 1, 1));
    float value8 = dot(noise_perlin_gradient(noise_perlin_hash(cellCoordinates + uint3(1, 1, 1), seed)), fractPosition - float3(1, 1, 1));
    
    noise = noise_perlin_interpolate_triliniear(value1, value2, value3, value4, value5, value6, value7, value8, noise_perlin_fade(fractPosition));
}

void noise_perlin_3D_float(float3 position, float octaveCountf, float persistence, float lacunarity, float seedf, out float noise) {

    int octaveCount = (int)octaveCountf;
    uint seed = (uint)seedf;
    
    float value = 0.0;
    float amplitude = 1.0;
    for (int i = 0; i < octaveCount; i++) {
        uint s = noise_perlin_hash(uint(i), seed); 
        value += noise_perlin_3D(position, s) * amplitude;
        amplitude *= persistence;
        position *= lacunarity;
    }

    noise = value;    
}

#endif // NOISE_PERLIN_GLSL