using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MgrBase 
{
	public virtual void Init() 
	{

	}
    public virtual void Update()
    {

    }
    public virtual void FixedUpdate()
    {

    }
    public abstract void Release();

    public virtual IEnumerator PreloadResource()
    {
        yield return null;
    }
}
