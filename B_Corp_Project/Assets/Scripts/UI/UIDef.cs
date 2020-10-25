using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI类型定义
/// </summary>
public enum UIType
{ 
	FULL_SCREEN = 1,		// 全屏类型UI, 同时只允许显示一个一级页面
	SECOND_LEVEL = 2,  // 二级页面，同时只允许显示一个二级页面, 不允许叠加
	THIRD_LEVEL = 3,		// 三级页面，可以任意叠加, 不记录浏览链表
	FLOAT_UI = 4,		// 不切换CurUI，直接添加到TopLayer上
}


/// <summary>
/// UI的销毁规则定义
/// </summary>
public enum UIDestroyRule
{
	UI_DESTROY_AFTER_STATE_CHANGED = 1,      // 状态切换后销毁
	UI_DESTROY_AFTER_SELF_CLOSE = 2         // 关闭即毁
}


/// <summary>
/// UI结构，保存UI的信息，如是否为全屏UI，可以根据需要动态扩展
/// </summary>
class UIStruct
{
	public UI_Name ui_name;
	public UIType ui_type;
	public UIDestroyRule destroy_rule;
	public bool footprint;

	public UIStruct(UI_Name _ui_name, UIType _ui_type, UIDestroyRule _destroy_rule, bool _footprint)
    {
		ui_name = _ui_name;
		ui_type = _ui_type;
		footprint = _footprint;
		destroy_rule = _destroy_rule;
	}
}


static class UIDef{

	private static int _id = 100000;
	private static Dictionary<UI_Name, UIStruct> UI_DEF_MAP = new Dictionary<UI_Name, UIStruct>();


	public static int UI_CONST_MAIN_MENU = generateid();
	public static int UI_CONST_GAME = generateid();

	private static int generateid()
    {
		_id += 1;
		return _id;
	}

	public static void AddStruct(UI_Name ui_name, UIStruct ui_struct)
    {
		UI_DEF_MAP[ui_name] = ui_struct;

	}

	public static UIStruct GetStruct(UI_Name ui_name)
	{
        if (UI_DEF_MAP.ContainsKey(ui_name))
        {
			return UI_DEF_MAP[ui_name];
		}
		return null;
	}
}

public enum UI_Name
{
	UI_ERROR = -1,
	UI_CONST_MAIN_MENU = 100010,
	UI_CONST_GAME = 100010,
}
