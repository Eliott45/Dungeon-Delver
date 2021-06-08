using UnityEngine;
using UnityEngine.Audio;

namespace __Scripts
{
    public class Volume : MonoBehaviour
    {
        public AudioMixer audioMixer;

        public void SetVolume(float volume)
        {
            audioMixer.SetFloat("Volume", volume);
        }
    }
}
