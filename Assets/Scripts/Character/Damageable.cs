using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DamageEvent : UnityEvent<GameObject> { }

public class Damageable : MonoBehaviour
{
	[SerializeField]
	private DamageEvent m_DamageEvent;

	public virtual void ApplyDamage(GameObject source)
	{
		m_DamageEvent.Invoke(source);
	}
}
