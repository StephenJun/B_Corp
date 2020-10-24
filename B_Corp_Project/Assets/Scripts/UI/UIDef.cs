using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//_id = 100000


//def generateid():
//	global _id
//	_id += 1
//	return _id


/// <summary>
/// UI类型定义
/// </summary>
enum UIType
{ 
	FULL_SCREEN = 1,		// 全屏类型UI, 同时只允许显示一个一级页面
	SECOND_LEVEL = 2,  // 二级页面，同时只允许显示一个二级页面, 不允许叠加
	THIRD_LEVEL = 3,		// 三级页面，可以任意叠加, 不记录浏览链表
	FLOAT_UI = 4,		// 不切换CurUI，直接添加到TopLayer上
}


/// <summary>
/// UI的销毁规则定义
/// </summary>
enum UIDestroyRule
{
	UI_DESTROY_AFTER_STATE_CHANGED = 1,      // 状态切换后销毁
	UI_DESTROY_AFTER_SELF_CLOSE = 2         // 关闭即毁
}


/// <summary>
/// UI结构，保存UI的信息，如是否为全屏UI，可以根据需要动态扩展
/// </summary>
struct UIStruct
{
	public string cls_name;
	public UIType ui_type;
	public UIDestroyRule destroy_rule;
	public bool footprint;

	public UIStruct(string _class_name, UIType _ui_type, UIDestroyRule _destroy_rule, bool _footprint)
    {
		cls_name = _class_name;
		ui_type = _ui_type;
		footprint = _footprint;
		destroy_rule = _destroy_rule;
	}
}


class UIDef{

	private static Dictionary<int, UIStruct> UI_DEF_MAP = new Dictionary<int, UIStruct>();

	public static void AddStruct(int ui_name, UIStruct ui_struct)
    {
		UI_DEF_MAP[ui_name] = ui_struct;
	}

	public static UIStruct GetStruct(int ui_name)
	{
        if (UI_DEF_MAP.ContainsKey(ui_name))
        {
			return UI_DEF_MAP[ui_name];
		}
		//TODO:
		return default;
	}
}