using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public bool onlyYaxis = true;
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            if(onlyYaxis)
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            transform.Rotate(0, 180f, 0);
        }
    }
}
