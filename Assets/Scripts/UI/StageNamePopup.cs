using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageNamePopup : MonoBehaviour
{
	private static StageNamePopup s_Main;
	public static StageNamePopup Main
	{
		get { return s_Main; }
	}

	[SerializeField]
	private Text m_StageText;
	[SerializeField]
	private Text m_LevelText;

	[SerializeField]
	private float m_DisplayTime = 3.0f;

	private float m_Timer;

	void Start()
    {
		s_Main = this;

	}
	
    void Update()
    {
		m_Timer += Time.deltaTime;

		if (m_Timer >= m_DisplayTime)
			gameObject.SetActive(false);
	}

	public void DisplayText(string stageName, int level)
	{
		m_Timer = 0.0f;
		gameObject.SetActive(true);

		m_StageText.text = stageName;
		m_LevelText.text = "Level " + (level + 1);
	}
}
