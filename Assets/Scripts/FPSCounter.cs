using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] float updateInterval = 0.5f;

    float accum = .0f;
    int frames = 0;
    float timeleft;
    float fps;

    GUIStyle textStyle = new GUIStyle();

    private void Start()
    {
        timeleft = updateInterval;

        textStyle.normal.textColor = Color.white;
    }

    private void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= .0f)
        {
            fps = (accum / frames);
            timeleft = updateInterval;
            accum = .0f;
            frames = 0;
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(5, 5, 100, 25), fps.ToString("F2") + " FPS", textStyle);
    }
}