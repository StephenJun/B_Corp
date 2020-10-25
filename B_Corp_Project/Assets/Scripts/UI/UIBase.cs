using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase: MonoBehaviour
{
    public UI_Name ui_name;
    [Tooltip("UI类型")]
    public UIType ui_type = UIType.FULL_SCREEN;
    [Tooltip("销毁规则")]
    public UIDestroyRule uiDestroyRule = UIDestroyRule.UI_DESTROY_AFTER_SELF_CLOSE;
    [Tooltip("是否记录浏览链表")]
    public bool footPrint;

    private bool ui_show;
    private UIStruct ui_struct;

    /// <summary>
    /// 初始化ui节点，创建ui数据结构
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public void InitConfig()
    {
        ui_struct = new UIStruct(ui_name, ui_type, uiDestroyRule, footPrint);
        UIDef.AddStruct(ui_name, ui_struct);
    }


    /// <summary>
    /// 设置widget是否可见
    /// </summary>
    /// <param name="visible"></param>
    /// <returns>TODO</returns>
    public bool SetVisible(bool visible)
    {
        print("SetVisible");
        ui_show = visible;
        transform.gameObject.SetActive(visible);
        //visible and self.register_msg()
        //not visible and self.cancel_msg()
        return true;
    }


    /// <summary>
    /// 设置ui显示
    /// </summary>
    /// <param name="kwargs">True: 显示，False, 不显示</param>
    public bool ShowUI(Dictionary<string, object> kwargs = null)
    {
        print("ShowUI");
        if (ui_show)
			return true;
        ui_show = true;
        SetVisible(true);

        Action callback = null;
        if (kwargs!=null && kwargs.ContainsKey("callback"))
        {
            callback = kwargs["callback"] as Action;
        }

        if (false) //ani_in:
            MonoBehaviour.print("UI_SHOW" + ui_name);
        //self.set_animation_play(ani_in, False, callback = callback)
        else
            callback?.Invoke();
        return true;
    }

    public void Update()
    {
        
    }

    /// <summary>
    /// 设置ui隐藏
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="kwargs">可根据需要传入参数, 如ani_out</param>
    public bool HideUI(Action callback=null, Dictionary<string, object> kwargs = null)
    {
        print("HideUI");
        if (!ui_show)
            return true;
        ui_show = false;

   //     def ani_end():
			//self.set_visible(False)

   //         callback and callback()

   //     if ani_out:
			//self.set_animation_play(ani_out, False, callback = ani_end)

   //     else:
			//ani_end()
        callback?.Invoke();
        return true;
    }

    /// <summary>
    /// 销毁UI, 需要清空相关timer及其他数据
    /// </summary>
    public void DestroyUI()
    {
        print("DestroyUI");
        //# 4. 清空ticks和messages
        //self.cancel_all_tick_func()
        //self.cancel_all_timer()
        //self.cancel_msg()
        GameObject.Destroy(transform.gameObject);
    }

}
