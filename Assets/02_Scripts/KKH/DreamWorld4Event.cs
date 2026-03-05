using System;
using UnityEngine;

public class DreamWorld4Event : MonoBehaviour
{
	public enum DreamWorld4EventType
	{
		House,
		Silkworm,
		DyePot,
		SpinningWall,
	}
	
	public DreamWorld4EventType eventType;
	private Day4Controller _day4Controller;

	private void Awake()
	{
		_day4Controller = FindFirstObjectByType<Day4Controller>();
	}

	private void OnTriggerEnter(Collider other)
	{
		switch (eventType)
		{
			case DreamWorld4EventType.House:
				_day4Controller.SetHouseOn();
				break;
			case DreamWorld4EventType.Silkworm:
				_day4Controller.SetSilkwormOn();
				break;
			case DreamWorld4EventType.DyePot:
				_day4Controller.SetDyePotOn();
				break;
			case DreamWorld4EventType.SpinningWall:
				_day4Controller.SetSpinningWheelOn();
				break;
		}
	}
}
