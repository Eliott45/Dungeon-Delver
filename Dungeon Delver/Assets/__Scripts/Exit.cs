using UnityEngine;
using UnityEngine.SceneManagement;

namespace __Scripts
{
    public class Exit : MonoBehaviour
    {
        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Dray"))
            {
                SceneManager.LoadScene("Credits");
            }
        }
    }
}
