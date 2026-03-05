using System;
using System.Collections;
using UnityEngine;

public class RopeShineEffect : MonoBehaviour
{
	Material _mat;
	
	private void Awake()
	{
		_mat = GetComponent<Renderer>().material;
	}


	public void ShineRope()
	{
		StartCoroutine(ShineRopeRoutine());
	}
	
	private IEnumerator ShineRopeRoutine()
	{
		float newIntensity = 2f;
		Color originColor = _mat.GetColor("_EmissionColor");
		Color endColor = Color.white * newIntensity;

		float maxTime = 2;
		float current = 0;
		float percent = 0;

		while (current < maxTime)
		{
			current += Time.deltaTime;
			percent = current / maxTime;
			
			Color emissive = Color.Lerp(originColor, endColor, percent);
			_mat.SetColor("_EmissionColor", emissive);
			yield return null;
		}
		
		_mat.SetColor("_EmissionColor", endColor);
	}
}
