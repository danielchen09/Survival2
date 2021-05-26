using System.Runtime.CompilerServices;
using Unity.Mathematics;

public struct NoiseGenerator {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Generate(float2 pos, NoiseSettings noiseSettings) {
        float val = 0;
        for (int octave = 0; octave < noiseSettings.octaves; octave++) {
            val += Snoise(pos * noiseSettings.frequency * math.pow(noiseSettings.lacunarity, octave)) * math.pow(noiseSettings.persistence, octave);
        }
        return math.clamp((val + 1) / 2, 0, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Generate(float3 pos, NoiseSettings noiseSettings) {
        float val = 0;
        for (int octave = 0; octave < noiseSettings.octaves; octave++) {
            val += Snoise(pos * noiseSettings.frequency * math.pow(noiseSettings.lacunarity, octave)) * math.pow(noiseSettings.persistence, octave);
        }
        return math.clamp((val + 1) / 2, 0, 1);
    }

    public static float Snoise(float pos) {
        return SimplexNoise.Generate(pos);
    }

    public static float Snoise(float2 pos) {
        return SimplexNoise.Generate(pos.x, pos.y);
    }

    public static float Snoise(float3 pos) {
        return SimplexNoise.Generate(pos.x, pos.y, pos.z);
    }
}