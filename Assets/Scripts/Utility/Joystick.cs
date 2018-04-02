using UnityEngine;

public class Joystick
{
    public static Vector2 GetSmoothInput(
        float deadzone,
        string horizontalAxisName,
        string verticalAxisName
    )
    {
        Vector2 axisInput       = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));
        float inputMagnitude    = axisInput.magnitude;

        return inputMagnitude < deadzone ? Vector2.zero : axisInput.normalized * ((inputMagnitude - deadzone) / (1.0f - deadzone));
    }
};