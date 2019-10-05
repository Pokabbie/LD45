using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{
	/// <summary>
	/// 3D lookup table used by this noise generator (Values store between -1 and 1)
	/// </summary>
	private float[] m_NoiseTable;
	/// <summary>
	/// The size of the lookup table (In 1 dimension)
	/// </summary>
	private int m_TableSize;


	public PerlinNoise(int Seed, int LookupTableSize = 16)
	{
		m_TableSize = LookupTableSize;

		// Generate noise table
		m_NoiseTable = new float[m_TableSize * m_TableSize * m_TableSize];
		Random.InitState(Seed);

		for (int i = 0; i < m_NoiseTable.Length; ++i)
			m_NoiseTable[i] = Random.Range(-1.0f, 1.0f);
	}

	/// <summary>
	/// Get a raw unfiltered value from the lookup table using 2D
	/// </summary>
	/// <returns>In range (-1 to 1)</returns>
	public float GetRawNoise(int X, int Y)
	{
		int x = X % m_TableSize;
		int y = Y % m_TableSize;

		if (x < 0)
			x += m_TableSize;
		if (y < 0)
			y += m_TableSize;

		return m_NoiseTable[x + y * m_TableSize];
	}

	/// <summary>
	/// Get a raw unfiltered value from the lookup table using 3D
	/// </summary>
	/// <returns>In range (-1 to 1)</returns>
	public float GetRawNoise(int X, int Y, int Z)
	{
		int x = X % m_TableSize;
		int y = Y % m_TableSize;
		int z = Z % m_TableSize;

		if (x < 0)
			x += m_TableSize;
		if (y < 0)
			y += m_TableSize;
		if (z < 0)
			z += m_TableSize;

		return m_NoiseTable[x + y * m_TableSize + z * m_TableSize * m_TableSize];
	}

	/// <summary>
	/// Cosine interpolate
	/// </summary>
	private static float CosLerp(float a, float b, float t)
	{
		float mu = (1.0f - Mathf.Cos(t * Mathf.PI)) * 0.5f;
		return Mathf.Lerp(a, b, mu);
	}

	/// <summary>
	/// Get filtered noise in 2D
	/// </summary>
	/// <param name="Frequency">How often noise should change</param>
	/// <param name="Scale">How should the noise be scaled</param>
	/// <returns></returns>
	public float GetNoise(int X, int Y, float Frequency, float Scale)
	{
		float x = (float)X * Frequency;
		float y = (float)Y * Frequency;

		int wholeX = Mathf.FloorToInt(x);
		int wholeY = Mathf.FloorToInt(y);
		float remX = x - wholeX;
		float remY = y - wholeY;


		float v1 = GetRawNoise(wholeX, wholeY) * Scale;
		float v2 = GetRawNoise(wholeX + 1, wholeY) * Scale;
		float v3 = GetRawNoise(wholeX, wholeY + 1) * Scale;
		float v4 = GetRawNoise(wholeX + 1, wholeY + 1) * Scale;

		// Interpolate between values
		float r1 = CosLerp(v1, v2, remX);
		float r2 = CosLerp(v3, v4, remX);

		// Interpolate between final values
		return CosLerp(r1, r2, remY);
	}

	/// <summary>
	/// Get filtered noise in 3D
	/// </summary>
	/// <param name="Frequency">How often noise should change</param>
	/// <param name="Scale">How should the noise be scaled</param>
	/// <returns></returns>
	public float GetNoise(int X, int Y, int Z, float Frequency, float Scale)
	{
		float x = (float)X * Frequency;
		float y = (float)Y * Frequency;
		float z = (float)Z * Frequency;

		int wholeX = Mathf.FloorToInt(x);
		int wholeY = Mathf.FloorToInt(y);
		int wholeZ = Mathf.FloorToInt(z);
		float remX = x - wholeX;
		float remY = y - wholeY;
		float remZ = z - wholeZ;

		float v000 = GetRawNoise(wholeX, wholeY, wholeZ) * Scale;
		float v100 = GetRawNoise(wholeX + 1, wholeY, wholeZ) * Scale;
		float v010 = GetRawNoise(wholeX, wholeY + 1, wholeZ) * Scale;
		float v110 = GetRawNoise(wholeX + 1, wholeY + 1, wholeZ) * Scale;

		float v001 = GetRawNoise(wholeX, wholeY, wholeZ + 1) * Scale;
		float v101 = GetRawNoise(wholeX + 1, wholeY, wholeZ + 1) * Scale;
		float v011 = GetRawNoise(wholeX, wholeY + 1, wholeZ + 1) * Scale;
		float v111 = GetRawNoise(wholeX + 1, wholeY + 1, wholeZ + 1) * Scale;


		float v1 = CosLerp(v000, v001, remZ);
		float v2 = CosLerp(v100, v101, remZ);
		float v3 = CosLerp(v010, v011, remZ);
		float v4 = CosLerp(v110, v111, remZ);

		// Interpolate between values
		float r1 = CosLerp(v1, v2, remX);
		float r2 = CosLerp(v3, v4, remX);

		// Interpolate between final values
		return CosLerp(r1, r2, remY);
	}
}
