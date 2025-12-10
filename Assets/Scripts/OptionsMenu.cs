using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public void setVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log("Volume set to: " + volume);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
