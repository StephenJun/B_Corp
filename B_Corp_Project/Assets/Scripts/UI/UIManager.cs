using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 记录UI切换数据, 用于切换与恢复
/// </summary>
public class UITreeNode
{
    public UI_Name ui_name;
    public UITreeNode pre = null;       // 浏览链表的前一个节点
    public UITreeNode next = null;          // 浏览链表的后一个节点
    public UITreeNode root_node = null;     // 当前节点对应的一级UI节点
    public UITreeNode second_node = null;    // 当前节点对应的二级UI节点
    public List<UITreeNode> third_nodes = new List<UITreeNode>();       // 当前节点对应的所有3级UI节点

    public UITreeNode(UI_Name _ui_name)
    {
        ui_name = _ui_name;
        pre = null;
        next = null;
        root_node = null;
        second_node = null;
        third_nodes = new List<UITreeNode>();
    }
}


/// <summary>
/// UI管理类
/// </summary>
public class UIManager : MonoSingleton<UIManager>
{
    // 保存注册的callback
    private Dictionary<string, int> _message_maps;
    // 保存当前记录在浏览链表中的ui，用于切换与恢复
    private Dictionary<UI_Name, UIBase> _ui_caches = new Dictionary<UI_Name, UIBase>();
    // 保存通过runUI来操作的状态机相关U
    private List<UI_Name> _hsm_caches = new List<UI_Name>();
    // 保存当前UITreeNode
    private UITreeNode _cur_node = null;
    // 保存当前全屏界面node
    private UITreeNode _cur_full_screen_node = null;
    // 保存当前二级界面node
    private UITreeNode _cur_second_node = null;
    // 保存当前状态下根UITreeNode, 会随着状态机的切换而变化
    private UITreeNode _root_node = null;
    // 保存当前三级页面的最高z_order
    private int _third_zorder = 0;
    //保存注册的tick函数
    private List<Action> _tick_funs = new List<Action>();
    //保存缓存的uiprefab
    private Dictionary<UI_Name, UIBase> ui_prefabs = new Dictionary<UI_Name, UIBase>();

    private Transform canvasTransform;
    private RectTransform _first_layer;
    private RectTransform _second_layer;
    private RectTransform _third_layer;
    private RectTransform _top_layer;


    /// <summary>
    /// 初始化UI场景
    /// </summary>
    public void Init()
    {
        canvasTransform = GameObject.FindObjectOfType<Canvas>().transform;
        InitLayer();
        InitPool();
    }

    public UITreeNode cur_node
    {
        get { return _cur_node; }
        set { _cur_node = value; }
    }


    /// <summary>
    /// 返回当前控制的UI名称
    /// </summary>
    public UI_Name cur_uiname
    {
        get
        {
            if (_cur_node == null)
                return UI_Name.UI_ERROR;
            return _cur_node.ui_name;
        }
    }

    /// <summary>
    /// 返回当前控制的UI实例
    /// </summary>
    public UIBase cur_ui
    {
        get
        {
            if (_ui_caches.ContainsKey(cur_uiname))
            {
                return _ui_caches[cur_uiname];
            }
            return null;
        }
    }

    private void InitLayer()
    {
        // 1. 首层: 放置全屏类的UI， 该层中同时只允许一个全屏UI显示
        _first_layer = CreateLayer("FirstLayer");
        // 2. 第二层：放置二级UI，该层只允许一个二级UI显示
        _second_layer = CreateLayer("SecondLayer");
        // 3. 第三层: 放置三级界面，允许同时显示多个三级界面
        _third_layer = CreateLayer("ThirdLayer");
        // 4. 顶层：放置Debug按钮等常驻最顶层UI
        _top_layer = CreateLayer("TopLayer");
    }

    private void InitPool()
    {
        UIBase[] prefabs = Resources.LoadAll<UIBase>("UIPrefabs");
        foreach (var ui in prefabs)
        {
            ui.InitConfig();
            ui_prefabs[ui.ui_name] = ui;
        }
    }

    private RectTransform CreateLayer(string layerName)
    {
        RectTransform layer = canvasTransform.GetChildByName(layerName) as RectTransform;
        if (!layer)
        {
            layer = new GameObject(layerName).AddComponent<RectTransform>();
            layer.SetParent(canvasTransform);
        }
        layer.sizeDelta = (canvasTransform as RectTransform).sizeDelta;
        layer.anchorMin = Vector2.zero;
        layer.anchorMax = Vector2.one;
        layer.offsetMin = Vector2.zero;
        layer.offsetMax = Vector2.zero;
        return layer;
    }


    /// <summary>
    /// 根据ui类型不同，添加到不同的layer中
    /// </summary>
    /// <param name="uiType">ui类型</param>
    /// <returns></returns>
    private Transform _seek_layer(UIType uiType)
    {
        if (uiType == UIType.FULL_SCREEN)
            return _first_layer;
        if (uiType == UIType.SECOND_LEVEL)
            return _second_layer;
        if (uiType == UIType.THIRD_LEVEL)
            return _third_layer;
        if (uiType == UIType.FLOAT_UI)
            return _top_layer;
        return null;
    }

    /// <summary>
    /// 查找指定UI是否浏览链表中
    /// </summary>
    /// <param name="ui_name"></param>
    /// <returns></returns>
    private UITreeNode _seek_treenode(UI_Name ui_name)
    {
        UITreeNode node = _root_node;
        while (node != null)
        {
            if (node.ui_name == ui_name)
                return node;
            else
                node = node.next;
        }
        return null;
    }

    /// <summary>
    /// 根据名称返回UI实例
    /// </summary>
    /// <param name="ui_name">需要查找的UI名称</param>
    /// <returns>UI实例</returns>
    public UIBase _get_ui(UI_Name ui_name)
    {
        if (_ui_caches.ContainsKey(ui_name))
            return _ui_caches[ui_name];
        return null;
    }


    /// <summary>
    /// 创建UI
    /// </summary>
    /// <param name="ui_name"></param>
    /// <returns></returns>
    private bool _create_ui(UI_Name ui_name, out UIBase ui, Dictionary<string, object> kwargs = null)
    {
        UIStruct ui_struct = UIDef.GetStruct(ui_name);
        if (_ui_caches.ContainsKey(ui_name))
        {
            ui = _ui_caches[ui_name];
        }
        else
        {
            ui = null;

        }
        if (ui != null)
        {
            return true;
        }
        else
        {
            ui = Instantiate(this.ui_prefabs[ui_struct.ui_name], _seek_layer(ui_struct.ui_type));
            _ui_caches[ui_name] = ui;
            return false;
        }
    }

    /// <summary>
    /// 销毁指定UI
    /// </summary>
    /// <param name="ui_name">需要销毁的UI名称</param>
    /// <param name="includeRootUI">是否销毁当前根UI</param>
    /// <returns>True/False</returns>
    private bool _remove_ui(UI_Name ui_name, bool includeRootUI = false)
    {
        UIBase ui = _get_ui(ui_name);
        if (!ui)
            return false;
        ui.HideUI(ui.DestroyUI);
        _ui_caches.Remove(ui_name);
        return true;
    }

    /// <summary>
    /// 销毁所有已缓存的对象
    /// </summary>
    /// <returns>True/False</returns>
    private bool RemoveAll()
    {
        foreach (var ui in _ui_caches)
        {
            _remove_ui(ui.Key, true);
        }
        _ui_caches.Clear();
        _hsm_caches.Clear();
        _cur_node = null;
        _root_node = null;
        _cur_full_screen_node = null;
        _cur_second_node = null;
        _third_zorder = 0;
        _message_maps.Clear();
        return true;
    }

    /// <summary>
    /// 关闭指定UI
    /// </summary>
    /// <param name="ui_name">需要关闭的UI名称</param>
    /// <param name="force"></param>
    /// <param name="kwargs"></param>
    /// <returns>True/False</returns>
    private void _close_ui(UI_Name ui_name, bool force = false, Dictionary<string, object> kwargs = null)
    {
        UIStruct ui_struct = UIDef.GetStruct(ui_name);
        UIDestroyRule d_rule = ui_struct.destroy_rule;
        UIBase ui = _get_ui(ui_name);
        // 关闭即销毁
        if (d_rule == UIDestroyRule.UI_DESTROY_AFTER_SELF_CLOSE)
            _remove_ui(ui_name);
        else if (ui && ui.gameObject.activeSelf)
        {
            if (force)
                ui.gameObject.SetActive(false);
            else
                ui.HideUI();
        }
    }

    /// <summary>
    /// 将UI添加到当前Scene中
    /// </summary>
    /// <param name="ui_name">需要添加的UI名称</param>
    private UIBase _add_ui(UI_Name ui_name, Dictionary<string, object> kwargs = null)
    {
        // 0. 已经是当前正在显示的UI了, 直接返回
        if (ui_name == cur_uiname)
        {
            return cur_ui;
        }
        UIStruct ui_struct = UIDef.GetStruct(ui_name);
        UIType uiType = ui_struct.ui_type;
        // 1. 全屏非透明类UI，同时只能显示一个
        if (uiType == UIType.FULL_SCREEN)
        {
            foreach (var existUI in _ui_caches.Keys)
            {
                ui_struct = UIDef.GetStruct(existUI);
                if (existUI != ui_name && ui_struct.ui_type != UIType.FLOAT_UI)
                {
                    _close_ui(existUI, true);
                }
            }
        }

        // 2. 二级弹窗UI：和一级界面同时显示，且只能显示一个二级页面
        else if (uiType == UIType.SECOND_LEVEL)
        {
            foreach (var existUI in _ui_caches.Keys)
            {
                ui_struct = UIDef.GetStruct(existUI);
                if (existUI != ui_name && (ui_struct.ui_type == UIType.SECOND_LEVEL || ui_struct.ui_type == UIType.THIRD_LEVEL))
                {
                    _close_ui(existUI, true);
                }
            }
        }
        UIBase ui;
        _create_ui(ui_name, out ui, kwargs);
        ui.ShowUI(kwargs);
        _update_treenode(ui_name, ui);
        ui.ui_name = ui_name;
        return ui;
    }

    /// <summary>
    /// 更新浏览链表
    /// </summary>
    /// <param name="ui_name"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
    private UIBase _update_treenode(UI_Name ui_name, UIBase ui)
    {
        UIStruct ui_struct = UIDef.GetStruct(ui_name);
        UIType uiType = ui_struct.ui_type;
        // FLoatUI不对当前UI进行切换，不更新浏览链表，直接返回
        if (uiType == UIType.FLOAT_UI)
            return null;
        UITreeNode tree_node = _seek_treenode(ui_name);
        UITreeNode pre = null;
        UITreeNode next = null;
        bool inCache = false;
        UITreeNode oldCurNode = _cur_node;

        if (tree_node != null)
        {
            pre = tree_node.pre;
            next = tree_node.next;
            inCache = true;
            tree_node.next = null;
            _cur_node = tree_node;
        }
        else
        {
            tree_node = new UITreeNode(ui_name);
            if (_cur_node == null)
                _cur_node = tree_node;
            else
            {
                tree_node.pre = _cur_node;
                _cur_node.next = tree_node;
                _cur_node = tree_node;
            }
        }

        if (ui_struct.ui_type == UIType.FULL_SCREEN)
        {
            _cur_full_screen_node = _cur_node;
            _cur_second_node = null;
        }
        else if (ui_struct.ui_type == UIType.SECOND_LEVEL)
        {
            if (inCache && _cur_node.root_node != _cur_full_screen_node)
            {
                if (pre != null)
                    pre.next = next;
                if (next != null)
                    next.pre = pre;
                _cur_node.pre = oldCurNode;
                oldCurNode.next = _cur_node;
            }
            _cur_node.root_node = _cur_full_screen_node;
            _cur_second_node = _cur_node;
        }
        else if (ui_struct.ui_type == UIType.THIRD_LEVEL)
        {
            _cur_node.root_node = _cur_full_screen_node;
            _cur_node.second_node = _cur_second_node;
            _third_zorder += 1;
            //ui.ui_node.setLocalZOrder(self._third_zorder);
        }
        return ui;
    }

    private UITreeNode _back_treenode(UIStruct ui_struct, UITreeNode node, Dictionary<string, object> kwargs = null)
    {
        UITreeNode preNode = node.pre;
        // TODO: 临时处理，允许关闭当前状态根UI
        if (preNode == null)
        {
            _root_node = null;
            return null;
        }
        // 1. 当前界面是一级界面
        if (ui_struct.ui_type == UIType.FULL_SCREEN)
        {
            UIStruct prestruct = UIDef.GetStruct(preNode.ui_name);
            // 1.1 上个界面是全屏非透明界面, 直接显示
            if (prestruct.ui_type == UIType.FULL_SCREEN)
            {
                if (!prestruct.footprint)
                    if (preNode.pre != null)
                    {
                        preNode = preNode.pre;
                    }
                UIBase preUI;
                bool preUIInCache = _create_ui(preNode.ui_name, out preUI);
                if (preUIInCache)
                {
                    preUI.gameObject.SetActive(true);
                }
                else
                {
                    preUI.ShowUI(kwargs);
                }
                _cur_full_screen_node = preNode;
                return preNode;
            }
            // 1.2 上个界面是二级界面, 需要找到对应的一级界面并显示
            else if (prestruct.ui_type == UIType.SECOND_LEVEL)
            {
                UITreeNode root_node = preNode.root_node;
                UIBase rootUI;
                bool rootUIInCache = _create_ui(root_node.ui_name, out rootUI);
                if (rootUIInCache)
                {
                    rootUI.gameObject.SetActive(true);
                }
                else
                {
                    rootUI.ShowUI(kwargs);
                }
                _cur_full_screen_node = root_node;
                if (prestruct.footprint)
                {
                    UIBase preUI;
                    bool preUIInCache = _create_ui(preNode.ui_name, out preUI);
                    if (preUIInCache)
                    {
                        preUI.gameObject.SetActive(true);
                    }
                    else
                    {
                        preUI.ShowUI(kwargs);
                    }
                    _cur_second_node = preNode;
                    return preNode;
                }
                return root_node;

            }
            // 1.3 上个界面是三级界面，需要找到对应的一级，二级并显示
            else if (prestruct.ui_type == UIType.THIRD_LEVEL)
            {
                UITreeNode root_node = preNode.root_node;
                UIBase rootUI;
                bool rootUIInCache = _create_ui(root_node.ui_name, out rootUI);
                if (rootUIInCache)
                {
                    rootUI.gameObject.SetActive(true);
                }
                else
                {
                    rootUI.ShowUI(kwargs);
                }
                _cur_full_screen_node = root_node;
                UITreeNode second_node = preNode.second_node;
                _cur_second_node = second_node;
                UITreeNode backNode = root_node;
                if (second_node != null)
                {
                    UIStruct secondstruct = UIDef.GetStruct(second_node.ui_name);
                    if (secondstruct.footprint)
                    {
                        UIBase secondUI;
                        bool inCache = _create_ui(second_node.ui_name, out secondUI);
                        if (inCache)
                        {
                            secondUI.gameObject.SetActive(true);
                        }
                        else
                        {
                            secondUI.ShowUI(kwargs);
                        }
                        backNode = second_node;
                    }
                }
                foreach (var th_node in preNode.third_nodes)
                {
                    UIStruct th_ui_st = UIDef.GetStruct(th_node.ui_name);
                    if (th_ui_st.footprint && th_node != preNode)
                    {
                        UIBase th_ui;
                        bool inCache = _create_ui(th_node.ui_name, out th_ui);
                        if (inCache)
                        {
                            th_ui.gameObject.SetActive(true);
                        }
                        else
                        {
                            th_ui.ShowUI(kwargs);
                        }
                        backNode = th_node;
                    }
                }

                if (prestruct.footprint)
                {
                    UIBase preUI;
                    bool preUIInCache = _create_ui(preNode.ui_name, out preUI);
                    if (preUIInCache)
                    {
                        preUI.gameObject.SetActive(true);
                    }
                    else
                    {
                        preUI.ShowUI(kwargs);
                    }
                    backNode = preNode;
                }
                return backNode;
            }
        }

        // 2. 当前界面是二级界面
        else if (ui_struct.ui_type == UIType.SECOND_LEVEL)
        {
            _cur_second_node = null;
            UIStruct prestruct = UIDef.GetStruct(preNode.ui_name);
            // 2.1 上个界面是三级界面，需要找到对应的二级/三级界面显示
            if (prestruct.ui_type == UIType.THIRD_LEVEL)
            {
                UITreeNode backNode = _cur_full_screen_node;
                UITreeNode second_node = preNode.second_node;
                _cur_second_node = second_node;
                if (second_node != null)
                {
                    UIStruct secondstruct = UIDef.GetStruct(second_node.ui_name);
                    if (secondstruct.footprint)
                    {
                        UIBase secondUI;
                        bool inCache = _create_ui(second_node.ui_name, out secondUI);
                        if (inCache)
                        {
                            secondUI.gameObject.SetActive(true);
                        }
                        else
                        {
                            secondUI.ShowUI(kwargs);
                        }
                        backNode = second_node;
                    }
                }

                if (prestruct.footprint)
                {
                    UIBase preUI;
                    bool preUIInCache = _create_ui(preNode.ui_name, out preUI);
                    if (preUIInCache)
                    {
                        preUI.gameObject.SetActive(true);
                    }
                    else
                    {
                        preUI.ShowUI(kwargs);
                    }
                    backNode = preNode;
                }
                return backNode;
            }
            // 2.2 上面界面是二级界面
            else if (prestruct.footprint && prestruct.ui_type == UIType.SECOND_LEVEL)
            {
                UIBase preUI;
                bool preUIInCache = _create_ui(preNode.ui_name, out preUI);
                if (preUIInCache)
                {
                    preUI.gameObject.SetActive(true);
                }
                else
                {
                    preUI.ShowUI(kwargs);
                }
                _cur_second_node = preNode;
                return preNode;
            }
            return _cur_full_screen_node;
        }
        // 3. 当前界面是三级界面
        else if (ui_struct.ui_type == UIType.THIRD_LEVEL)
            return preNode;
        return preNode;
    }

    /// <summary>
    /// 关闭指定UI
    /// </summary>
    /// <param name="ui_name">需要关闭的UI名称</param>
    /// <param name="kwargs">UI关闭时需要的参数</param>
    /// <returns>True/False</returns>
    private bool _back_ui(UI_Name ui_name, Dictionary<string, object> kwargs = null)
    {
        // 1. 不可关闭当前状态根UI
        //if(ui_name in self._hsm_caches or ui_name not in self._ui_caches:
        // return False
        if (!_ui_caches.ContainsKey(ui_name))
            return false;
        UIStruct ui_struct = UIDef.GetStruct(ui_name);
        // 2. 对于全屏非透明类UI，只允许关闭当前UI
        if (ui_struct.ui_type == UIType.FULL_SCREEN && ui_name != cur_uiname)
            return false;
        // 3. 关闭指定UI
        _close_ui(ui_name, false, kwargs);
        // 4. 更新浏览链表
        UITreeNode node = _seek_treenode(ui_name);
        // 4.1 关闭的是当前UI, 直接返回pre
        if (node == _cur_node)
        {
            _cur_node = _back_treenode(ui_struct, node, kwargs);
            if (_cur_node != null)
                _cur_node.next = null;
        }
        // 4.2 关闭的非当前UI， 原则上应该不允许出现这种情况，这里还是处理下吧
        else if (node == null)
            return true;
        else
        {
            if (node.pre != null)
                node.pre.next = node.next;
            if (node.next != null)
                node.next.pre = node.pre;
        }
        return true;
    }

    /// <summary>
    /// 该函数主要用来配合GameController切换状态时使用的函数，该函数会进行UI的现场恢复
    /// </summary>
    /// <param name="ui_name">需要显示的UI名称</param>
    /// <param name="kwargs">UI初始化或show_ui时的参数</param>
    /// <returns>UI实例</returns>
    private UIBase _run_ui(UI_Name ui_name, Dictionary<string, object> kwargs = null)
    {
        UIStruct ui_struct = UIDef.GetStruct(ui_name);
		if(ui_struct == null)
			return null;
        RemoveAll();

        UIBase ui;
        _create_ui(ui_name, out ui, kwargs);
        _hsm_caches.Add(ui_name);
        UITreeNode ui_node = new UITreeNode(ui_name);
        _root_node = ui_node;
        _cur_node = ui_node;
        _cur_full_screen_node = _cur_node;

        ui.ShowUI(kwargs);
        ui.ui_name = ui_name;
        return ui;
    }


    /// <summary>
    /// 所有UI的tick入口
    /// </summary>
    private void Update()
    {
        //1.只允许当前显示的1,2,3级界面进行tick
        //2.其中3级界面只允许cur_ui执行tick
        //3.如有其他需求：请考虑使用register_tick_func或add_repeat_timer来实现
        if (_cur_full_screen_node != null)
        {
            UIBase fullUI = _get_ui(_cur_full_screen_node.ui_name);
            if (fullUI)
            {
                fullUI.Update();
            }
        }

        if (_cur_second_node != null)
        {
            UIBase secondUI = _get_ui(_cur_second_node.ui_name);
            if (secondUI)
            {
                secondUI.Update();
            }
        }

        if (cur_ui != null && _cur_node != _cur_full_screen_node && _cur_node != _cur_second_node)
            cur_ui.Update();

        for (int i = 0; i < _tick_funs.Count; i++)
        {
            _tick_funs[i]();
        }
    }

    public UIBase run_ui(UI_Name ui_name, Dictionary<string, object> kwargs = null)
    {
        return _run_ui(ui_name, kwargs);
    }

    public UIBase add_ui(UI_Name ui_name, Dictionary<string, object> kwargs = null)
    {
        return _add_ui(ui_name, kwargs);
    }

    public bool back_ui(UI_Name ui_name, Dictionary<string, object> kwargs = null)
    {
        return _back_ui(ui_name, kwargs);
    }

    public void register(int msg_type, int func)
    {
        //self._message_maps.setdefault(msg_type, set()).add(func)
    }

    public void cancel(int msg_type, int func)
    {
   //     if (msg_type in self._message_maps:
			//if (func is None:
			//	del self._message_maps[msg_type]
   //         else if (func in self._message_maps[msg_type]:
   //             self._message_maps[msg_type].remove(func)
   //             if (not self._message_maps[msg_type]:
   //                 del self._message_maps[msg_type]
    }


    public void receive(int msg_type)
    {
        //if (msg_type in self._message_maps:
        //    res = None
        //    funcs = self._message_maps[msg_type]
        //// 一个元素的set大小变化也会有问题，所以去除大小判断
        //    funcs = set.copy(funcs)
        //    for func in funcs:
        //        func_res = func(*args, **kwargs)
        //        res = func_res if (func_res is not None else res
        //    return res
        //return None
    }

    /// <summary>
    /// 注册需要tick执行的函数
    /// </summary>
    /// <param name="func"></param>
    public void register_tick_func(Action func)
    {
        // 注册需要tick执行的函数
        if (!_tick_funs.Contains(func))
        {
            _tick_funs.Add(func);
        }
    }

    /// <summary>
    /// 移除已注册的tick函数
    /// </summary>
    /// <param name=""></param>
    public void remove_tick_func(Action func)
    {
        // 移除已注册的tick函数
        if (_tick_funs.Contains(func))
        {
            _tick_funs.Remove(func);
        }
    }
}
