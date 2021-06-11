using System.Runtime.CompilerServices;
using Unity.Mathematics;

public struct NoiseGenerator {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Generate(float2 pos, NoiseSettings noiseSettings) {
        float val = 0;
        for (int octave = 0; octave < noiseSettings.octaves; octave++) {
            val += Snoise(pos * noiseSettings.frequency * math.pow(noiseSettings.lacunarity, octave)) * math.pow(noiseSettings.persistence, octave);
        }
        return math.clamp(val, -1, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GeneratePositive(float2 pos, NoiseSettings noiseSettings) {
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
        return math.clamp(val, -1, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GenerateRidge(float3 pos, NoiseSettings noiseSettings) {
        float val = 0;
        for (int octave = 0; octave < noiseSettings.octaves; octave++) {
            val += math.abs(Snoise(pos * noiseSettings.frequency * math.pow(noiseSettings.lacunarity, octave))) * math.pow(noiseSettings.persistence, octave);
        }
        return math.clamp(val, -1, 1);
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

    public static float SnoisePositive(float2 pos) {
        return (Snoise(pos) + 1) / 2;
    }

    public static float SnoisePositive(float3 pos) {
        return (Snoise(pos) + 1) / 2;
    }
}