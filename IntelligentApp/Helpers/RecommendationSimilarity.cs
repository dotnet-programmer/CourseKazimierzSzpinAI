namespace IntelligentApp.Helpers;

public static class RecommendationSimilarity
{
	public static float CosineSimilarity(float[]? vecA, float[]? vecB)
	{
		ArgumentNullException.ThrowIfNull(vecA);
		ArgumentNullException.ThrowIfNull(vecB);

		var dot = 0f;
		var magA = 0f;
		var magB = 0f;

		for (int i = 0; i < vecA.Length; i++)
		{
			dot += vecA[i] * vecB[i];
			magA += vecA[i] * vecA[i];
			magB += vecB[i] * vecB[i];
		}

		magA = (float)Math.Sqrt(magA);
		magB = (float)Math.Sqrt(magB);

		return magA == 0 || magB == 0 ? 0f : dot / (magA * magB);
	}
}