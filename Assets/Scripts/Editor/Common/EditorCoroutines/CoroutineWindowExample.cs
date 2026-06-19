using UnityEngine;
using System.Collections;
using UnityEditor;

#pragma warning disable CS0618 // WWW is obsolete

namespace EditorCoroutines
{
	public class CoroutineWindowExample : EditorWindow
	{
		[MenuItem("Window/Coroutine Example")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(CoroutineWindowExample));
		}

		void OnGUI()
		{
			if (GUILayout.Button("Start"))
			{
				this.StartCoroutine(Example());
			}

			if (GUILayout.Button("Start WWW"))
			{
				this.StartCoroutine(ExampleWWW());
			}

			if (GUILayout.Button("Start Nested"))
			{
				this.StartCoroutine(ExampleNested());
			}

			if (GUILayout.Button("Stop"))
			{
				this.StopCoroutine("Example");
			}
			if (GUILayout.Button("Stop all"))
			{
				this.StopAllCoroutines();
			}

			if (GUILayout.Button("Also"))
			{
				this.StopAllCoroutines();
			}
		}

		IEnumerator Example()
		{
			while (true)
			{
				yield return new WaitForSeconds(2f);
			}
		}

		IEnumerator ExampleWWW()
		{
			while (true)
			{
				var www = new WWW("https://unity3d.com/");
				yield return www;
				yield return new WaitForSeconds(2f);
			}
		}

		IEnumerator ExampleNested()
		{
			while (true)
			{
				yield return new WaitForSeconds(2f);
				yield return this.StartCoroutine(ExampleNestedOneLayer());
			}
		}

		IEnumerator ExampleNestedOneLayer()
		{
			yield return new WaitForSeconds(2f);
			yield return this.StartCoroutine(ExampleNestedTwoLayers());
		}

		IEnumerator ExampleNestedTwoLayers()
		{
			yield return new WaitForSeconds(2f);
		}


		class NonEditorClass
		{
			public void DoSomething(bool start, bool stop, bool stopAll)
			{
				if (start)
				{
					EditorCoroutines.StartCoroutine(Example(), this);
				}
				if (stop)
				{
					EditorCoroutines.StopCoroutine("Example", this);
				}
				if (stopAll)
				{
					EditorCoroutines.StopAllCoroutines(this);
				}
			}

			IEnumerator Example()
			{
				while (true)
				{
					yield return new WaitForSeconds(2f);
				}
			}
		}
	}
}