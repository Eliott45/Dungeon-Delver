using UnityEngine;

namespace __Dungeon_Editor
{
	public class EditorDestroyGameObject : MonoBehaviour {
		// Destroy this.gameObject when the scene starts
		void Start () {
			Destroy(gameObject);
		}
	}
}
