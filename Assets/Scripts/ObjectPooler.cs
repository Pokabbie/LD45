using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
	private static ObjectPooler s_Main;
	public static ObjectPooler Main
	{
		get { return s_Main; }
	}

	private Dictionary<GameObject, Queue<GameObject>> m_Pools = new Dictionary<GameObject, Queue<GameObject>>();

	void Start()
	{
		if (s_Main == null)
			s_Main = this;
		else
			Debug.LogWarning("Multiple object poolers found");
	}
	
	public GameObject GetObject(GameObject sourceType, Transform parent = null)
	{
		return GetObject(sourceType, Vector3.zero, Quaternion.identity, parent);
	}

	public GameObject GetObject(GameObject sourceType, Vector3 position, Quaternion rotation, Transform parent = null)
	{
#if UNITY_EDITOR
		GameObject sourceObj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(sourceType);
		if (sourceObj != sourceType)
		{
			Debug.LogErrorFormat("Trying to spawn object from instance for {0}", sourceType.name);
		}
#endif
		Queue<GameObject> queue = GetQueue(sourceType);
		GameObject obj;

		if (queue.Count == 0)
			obj = CreateNew(sourceType);
		else
			obj = queue.Dequeue();
		
		obj.SetActive(true);
		obj.transform.parent = parent;
		obj.transform.position = position;
		obj.transform.rotation = rotation;
		return obj;
	}

	public void ReturnObject(GameObject instance)
	{
		ObjectPoolInstance instInfo = instance.GetComponent<ObjectPoolInstance>();
		if (instInfo == null)
		{
			Debug.LogErrorFormat("Attempting to add object which was not spawned from pool '{0}'", instance.name);
		}
		else
		{
			instance.transform.SetParent(transform);
			instance.SetActive(false);
			GetQueue(instInfo.m_SourceObject).Enqueue(instance);
		}
	}

	private Queue<GameObject> GetQueue(GameObject sourceType)
	{
		if (!m_Pools.ContainsKey(sourceType))
			m_Pools.Add(sourceType, new Queue<GameObject>());

		return m_Pools[sourceType];
	}

	private GameObject CreateNew(GameObject sourceType)
	{
		GameObject obj = Instantiate(sourceType);

		// Work around for inheritance container
		if (obj.GetComponent<InheritanceContainer>())
		{
			Debug.AssertFormat(obj.transform.childCount == 1, "Need 1 and only 1 child for inheritance container when object pooling");
			obj = obj.transform.GetChild(0).gameObject;
		}

		ObjectPoolInstance inst = obj.AddComponent<ObjectPoolInstance>();
		inst.m_SourceObject = sourceType;
		return obj;
	}
}
