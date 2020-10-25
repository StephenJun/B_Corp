using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public float moveSensitivity;
    public float zoomSensitivity;
    public float smooth;

    // Start is called before the first frame update
    void Start()
    {
        //print(UIDef.UI_CONST_MAIN_MENU);
        UIManager.Instance.Init();
        UIManager.Instance.add_ui(0);
        //UIManager.Instance.back_ui(0);
    }

    // Update is called once per frame
    void Update()
    {
        Move(Vector3.right * Input.GetAxisRaw("Horizontal") * moveSensitivity);
        Move(Vector3.forward * Input.GetAxisRaw("Vertical") * moveSensitivity);

        if (Input.GetKey(KeyCode.Q))
            Move(Vector3.up * zoomSensitivity);
        if (Input.GetKey(KeyCode.Q))
            Move(Vector3.down * zoomSensitivity);
    }

    public void Move(Vector3 dir)
    {
        Vector3 targetPos = transform.position + dir;
        transform.position = Vector3.Lerp(transform.position, targetPos, smooth);
    }

    /// <summary>
    /// 相机平滑移动到targetObj上方距离height处
    /// </summary>
    /// <param name="targetObj">目标物体</param>
    /// <param name="height">高度</param>
    /// <param name="force">true：硬切，不需要过度</param>
    public void Move(GameObject targetObj, float height, bool force)
    {
        //TODO:@Lucas
        //相机平滑移动到targetObj上方距离height处
    }
}