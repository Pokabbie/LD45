using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InheritanceContainer : MonoBehaviour
{
    void Start()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			Transform child = transform.GetChild(0);
			child.gameObject.tag = gameObject.tag;
			child.parent = null;
		}

		Destroy(gameObject);
    }
}
