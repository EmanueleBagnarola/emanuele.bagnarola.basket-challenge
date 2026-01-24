using UnityEngine;

public class ButtonInjectionTest : MonoBehaviour
{
	[Button]
	public void Method1()
	{
		Debug.Log("Method1");
	}

	[Button]
	public void Method2()
	{
		Debug.Log("Method2");
	}

	[Button(label = "Custom Label")]
	public void Method3()
	{
		Debug.Log("Method3");
	}

	[Button(label = "Custom Label. Order 1", order = 1)]
	public void Method4()
	{
		Debug.Log("Method4");
	}
}