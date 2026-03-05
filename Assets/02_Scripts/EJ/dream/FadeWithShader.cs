using UnityEngine;

public class FadeWithShader : MonoBehaviour
{
    private float fadeDuration = 2f;
    private Material[] mats;
    private Color[] baseColors;

    void Start()
    {
        mats = GetComponent<Renderer>().materials;
        baseColors = new Color[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            baseColors[i] = mats[i].GetColor("_BaseColor");
            Color c = baseColors[i];
            c.a = 0f;
            mats[i].SetColor("_BaseColor", c);
        }

        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);

            for (int i = 0; i < mats.Length; i++)
            {
                Color c = baseColors[i];
                c.a = alpha;
                mats[i].SetColor("_BaseColor", c);
            }

            yield return null;
        }
    }
}
