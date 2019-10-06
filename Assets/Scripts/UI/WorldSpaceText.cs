using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceText : MonoBehaviour
{
	[SerializeField]
	private Text m_Message;
	[SerializeField]
	private Text m_BackgroundMessage;

	[SerializeField]
	private Vector3 m_AnimationStep = Vector3.up;
	
	private GameObject m_RootObject;
	private float m_LifeTime;
		
    void Update()
    {
		m_LifeTime -= Time.deltaTime;

		transform.position += m_AnimationStep * Time.deltaTime;

		if (m_LifeTime <= 0.0)
			ObjectPooler.Main.ReturnObject(m_RootObject);
    }

	public static WorldSpaceText CreatePopup(GameObject prefab, string message, Vector3 position, Color colour, float duration = 2.0f)
	{
		GameObject obj = ObjectPooler.Main.GetObject(prefab, position, Quaternion.identity);
		WorldSpaceText text = obj.GetComponent<WorldSpaceText>();
		text.m_RootObject = obj;
		text.m_Message.color = colour;
		text.m_Message.text = message;
		text.m_BackgroundMessage.text = message;
		text.m_LifeTime = duration;
		return text;
	}
}
