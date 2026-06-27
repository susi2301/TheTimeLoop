#define FRAMERENDERER

using UnityEngine;

public class FpsRenderer : MonoBehaviour
{
    public bool renderFps = false;
    
    private uint _frame = 0;
    private float _fps;
    private float _average = 0f;
    
    private uint _framesBelow60 = 0;
    private float _percentBelow60 = 0;

    
    private uint _framesBelow100 = 0;
    private float _percentBelow100 = 0;
    
    
    private uint _framesAbove144 = 0;
    private float _percentAbove144 = 0;

    public void Toggle(bool toggleOn)
    {
        renderFps = toggleOn;
    }

    private void Start()
    {
        GUI.depth = 2;
    }

#if FRAMERENDERER
    private void Update()
    {
        if (renderFps)
        {
            _frame++;
            _fps = 1f / Time.unscaledDeltaTime;
            
            // average
            _average += (_fps - _average) / _frame;
            
            // below 100
            if (_fps < 100)
            {
                _framesBelow100++;
            }
            _percentBelow100 = (float)_framesBelow100 / (float)_frame * 100;

            // below 60
            if (_fps < 60)
            {
                _framesBelow60++;
            }
            _percentBelow60 = (float)_framesBelow60 / (float)_frame * 100;

            // above 144
            if (_fps > 144)
            {
                _framesAbove144++;
            }
            _percentAbove144 = (float)_framesAbove144 / (float)_frame * 100;
            
        }
    }
#endif

    private void OnGUI()
    {
#if FRAMERENDERER
        
        if (renderFps)
        {

            string txt = "FPS: " + (int)_fps +
                         "\tAverage: " + (int)_average +
                         "\nBelow 60: " + (int)_percentBelow60   + "%" +
                         "\nBelow 100: " + (int)_percentBelow100 + "%" +
                         "\nAbove 144: " + (int)_percentAbove144 + "%" ;
            

            GUI.Label(new Rect(5, 10, 250, 100), txt);
        }
#endif
    }
}
