using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderObjectCallback : MonoBehaviour 
{
	public delegate void RenderCallback(MeshRenderer mr); 
	public RenderCallback onWillRenderCallback = null;

	private MeshRenderer _meshRenderer;

	void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
	}

	void OnWillRenderObject()
	{
		if (onWillRenderCallback != null)
			onWillRenderCallback(_meshRenderer);
	}
}
