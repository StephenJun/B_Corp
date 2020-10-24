using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 记录UI切换数据, 用于切换与恢复
/// </summary>
public class UITreeNodeP 
{
	public int ui_name;
	public UITreeNodeP pre = null;       // 浏览链表的前一个节点
	public UITreeNodeP next = null;          // 浏览链表的后一个节点
	public UITreeNodeP root_node = null;     // 当前节点对应的一级UI节点
	public UITreeNodeP second_node = null;    // 当前节点对应的二级UI节点
	public List<UITreeNodeP> third_nodes = new List<UITreeNodeP>();       // 当前节点对应的所有3级UI节点

	public UITreeNodeP(int _ui_name)
    {
		ui_name = _ui_name;
		pre = null;
		next = null;
		root_node = null;
		second_node = null;
		third_nodes = new List<UITreeNodeP>();    
	}
}


/// <summary>
/// UI管理类
/// </summary>
public class UIManager : MonoBehaviour
{
	// 保存注册的callback
	private Dictionary<string, int> _message_maps;
	// 保存当前记录在浏览链表中的ui，用于切换与恢复
	private Dictionary<int, UIBase> _ui_caches = new Dictionary<int, UIBase>();
	// 保存通过runUI来操作的状态机相关U
	private List<UIBase> _hsm_caches = new List<UIBase>();
	// 保存当前UITreeNode
	private UITreeNodeP _cur_node = null;
	// 保存当前全屏界面node
	private UITreeNodeP _cur_full_screen_node = null;
	// 保存当前二级界面node
	private UITreeNodeP _cur_second_node = null;
	// 保存当前状态下根UITreeNode, 会随着状态机的切换而变化
	private UITreeNodeP _root_node = null;
	// 保存当前三级页面的最高z_order
	private int _third_zorder = 0;

	private Transform _first_layer;
	private Transform _second_layer;
	private Transform _third_layer;
	private Transform _top_layer;


	/// <summary>
	/// 初始化UI场景
	/// </summary>
	public void Init()
	{

	}

	public UITreeNodeP cur_node
	{
		get { return _cur_node; }
		set { _cur_node = value; }
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

	private Transform CreateLayer(string layerName)
	{
		Transform layer = transform.GetChildByName(layerName);
		if (!layer)
		{
			layer = new GameObject(layerName).transform;
			layer.SetParent(transform);
		}
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
	private UITreeNodeP _seek_treenode(int ui_name)
	{
		UITreeNodeP node = _root_node;
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
	public UIBase _get_ui(int ui_name)
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
	private bool _create_ui(int ui_name)
	{
		UIBase ui = null;
		UIStruct ui_struct = UIDef.GetStruct(ui_name);
		if (_ui_caches.ContainsKey(ui_name))
		{
			ui = _ui_caches[ui_name];
		}
		if (ui)
		{

			return true;
		}
		else
		{
			ui = Instantiate(Resources.Load<UIBase>(ui_struct.cls_name));
			ui.transform.SetParent(_seek_layer(ui_struct.ui_type));
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
	private bool _remove_ui(int ui_name, bool includeRootUI = false)
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
		for (int i = 0; i < _ui_caches.Count; i++)
		{
			_remove_ui(_ui_caches[i].ui_name, true);
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
}

/*
	/// <summary>
	/// 关闭指定UI
	/// </summary>
	/// <param name="ui_name">需要关闭的UI名称</param>
	/// <param name="force"></param>
	/// <param name="kwargs"></param>
	/// <returns>True/False</returns>
	private bool _close_ui(int ui_name, bool force = false, Dictionary<string, object> kwargs = null)
    {
		ui_struct = UIDef.get_struct(ui_name)
		d_rule = struct.destroy_rule
		ui = self._get_ui(ui_name)
		// 关闭即销毁
		if d_rule == UIDestroyRule.UI_DESTROY_AFTER_SELF_CLOSE:
			self._remove_ui(ui_name)
		elif ui and ui.widget.isVisible():
			ui.set_visible(False) if force else ui.hide_ui(** kwargs)
	}
}
	def _close_ui(self, ui_name, force=False, **kwargs):
		// 关闭指定UI
		@param ui_name: 需要关闭的UI名称
		@return: True/False
		//
		struct = UIDef.get_struct(ui_name)
		d_rule = struct.destroy_rule
		ui = self._get_ui(ui_name)
// 关闭即销毁
		if d_rule == UIDestroyRule.UI_DESTROY_AFTER_SELF_CLOSE:
			self._remove_ui(ui_name)
		elif ui and ui.widget.isVisible():
			ui.set_visible(False) if force else ui.hide_ui(**kwargs)

	def _add_ui(self, ui_name, **kwargs):
		// 将UI添加到当前Scene中
		@param ui_name: 需要添加的UI名称
		@param **kwargs: UI创建/显示时需要的参数
		@return: True/False
		//
// 0. 已经是当前正在显示的UI了, 直接返回
		if ui_name == self.cur_uiname:
			return self.cur_ui
		struct = UIDef.get_struct(ui_name)
		uiType = struct.ui_type
// 1. 全屏非透明类UI，同时只能显示一个
		if uiType == UIType.FULL_SCREEN:
			for existUI in self._ui_caches.keys():
				struct = UIDef.get_struct(existUI)
				existUI != ui_name and struct.ui_type != UIType.FLOAT_UI and self._close_ui(
					existUI, True)
// 2. 二级弹窗UI：和一级界面同时显示，且只能显示一个二级页面
		elif uiType == UIType.SECOND_LEVEL:
			for existUI in self._ui_caches.keys():
				struct = UIDef.get_struct(existUI)
				existUI != ui_name and struct.ui_type in [
					UIType.SECOND_LEVEL, UIType.THIRD_LEVEL] and self._close_ui(existUI, True)
		_, ui = self._create_ui(ui_name, **kwargs)
		ui.show_ui(**kwargs)
		self._update_treenode(ui_name, ui)
		ui.UI_NAME = ui_name
		return ui

	def _update_treenode(self, ui_name, ui):
		// 更新浏览链表
		//
		struct = UIDef.get_struct(ui_name)
		uiType = struct.ui_type
// FLoatUI不对当前UI进行切换，不更新浏览链表，直接返回
		if uiType == UIType.FLOAT_UI:
			return
		tree_node = self._seek_treenode(ui_name)
		pre, next, inCache, oldCurNode = None, None, False, self._cur_node
		if tree_node:
			pre, next, inCache = tree_node.pre, tree_node.next, True
			tree_node.next = None
			self._cur_node = tree_node
		else:
			tree_node = UITreeNode(ui_name)
			if self._cur_node is None:
				self._cur_node = tree_node
			else:
				tree_node.pre = self._cur_node
				self._cur_node.next = tree_node
				self._cur_node = tree_node
		if struct.ui_type == UIType.FULL_SCREEN:
			self._cur_full_screen_node = self._cur_node
			self._cur_second_node = None
		elif struct.ui_type == UIType.SECOND_LEVEL:
			if inCache and self._cur_node.root_node != self._cur_full_screen_node:
				pre and setattr(pre, 'next', next)
				next and setattr(next, 'pre', pre)
				self._cur_node.pre = oldCurNode
				oldCurNode.next = self._cur_node
			self._cur_node.root_node = self._cur_full_screen_node
			self._cur_second_node = self._cur_node
		elif struct.ui_type == UIType.THIRD_LEVEL:
			self._cur_node.root_node = self._cur_full_screen_node
			self._cur_node.second_node = self._cur_second_node
			self._third_zorder += 1
			ui.ui_node.setLocalZOrder(self._third_zorder)
		return ui

	def _back_treenode(self, struct, node, **kwargs):
		preNode = node.pre
// TODO: 临时处理，允许关闭当前状态根UI
		if preNode is None:
			self._root_node = None
			return None

// 1. 当前界面是一级界面
		if struct.ui_type == UIType.FULL_SCREEN:
			prestruct = UIDef.get_struct(preNode.ui_name)
// 1.1 上个界面是全屏非透明界面, 直接显示
			if prestruct.ui_type == UIType.FULL_SCREEN:
				if not prestruct.footprint:
					preNode = preNode.pre or preNode
				preUIInCache, preUI = self._create_ui(preNode.ui_name)
				preUI.set_visible(
					True) if preUIInCache else preUI.show_ui(**kwargs)
				self._cur_full_screen_node = preNode
				return preNode
// 1.2 上个界面是二级界面, 需要找到对应的一级界面并显示
			elif prestruct.ui_type == UIType.SECOND_LEVEL:
				root_node = preNode.root_node
				rootUIInCache, rootUI = self._create_ui(
					root_node.ui_name)
				rootUI.set_visible(
					True) if rootUIInCache else rootUI.show_ui(**kwargs)
				self._cur_full_screen_node = root_node
				if prestruct.footprint:
					preUIInCache, preUI = self._create_ui(
						preNode.ui_name)
					preUI.set_visible(
						True) if preUIInCache else preUI.show_ui(**kwargs)
					self._cur_second_node = preNode
					return preNode
				return root_node
// 1.3 上个界面是三级界面，需要找到对应的一级，二级并显示
			elif prestruct.ui_type == UIType.THIRD_LEVEL:
				root_node = preNode.root_node
				rootUIInCache, rootUI = self._create_ui(
					root_node.ui_name)
				rootUI.set_visible(
					True) if rootUIInCache else rootUI.show_ui(**kwargs)
				self._cur_full_screen_node = root_node
				second_node = preNode.second_node
				self._cur_second_node = second_node
				backNode = root_node
				if second_node:
					secondstruct = UIDef.get_struct(second_node.ui_name)
					if secondstruct.footprint:
						inCache, secondUI = self._create_ui(
							second_node.ui_name)
						secondUI.set_visible(
							True) if inCache else secondUI.show_ui(**kwargs)
						backNode = second_node
				for th_node in preNode.third_nodes:
					th_ui_st = UIDef.get_struct(th_node.ui_name)
					if th_ui_st.footprint and th_node != preNode:
						inCache, th_ui = self._create_ui(
							th_node.ui_name)
						th_ui.set_visible(
							True) if inCache else th_ui.show_ui(**kwargs)
						backNode = th_node
				if prestruct.footprint:
					preUIInCache, preUI = self._create_ui(
						preNode.ui_name)
					preUI.set_visible(
						True) if preUIInCache else preUI.show_ui(**kwargs)
					backNode = preNode
				return backNode
// 2. 当前界面是二级界面
		elif struct.ui_type == UIType.SECOND_LEVEL:
			self._cur_second_node = None
			prestruct = UIDef.get_struct(preNode.ui_name)
// 2.1 上个界面是三级界面，需要找到对应的二级/三级界面显示
			if prestruct.ui_type == UIType.THIRD_LEVEL:
				backNode = self._cur_full_screen_node
				second_node = preNode.second_node
				self._cur_second_node = second_node
				if second_node:
					secondstruct = UIDef.get_struct(second_node.ui_name)
					if secondstruct.footprint:
						inCache, secondUI = self._create_ui(
							second_node.ui_name)
						secondUI.set_visible(
							True) if inCache else secondUI.show_ui(**kwargs)
						backNode = second_node
				if prestruct.footprint:
					preUIInCache, preUI = self._create_ui(
						preNode.ui_name)
					preUI.set_visible(
						True) if preUIInCache else preUI.show_ui(**kwargs)
					backNode = preNode
				return backNode
// 2.2 上面界面是二级界面
			elif prestruct.footprint and prestruct.ui_type == UIType.SECOND_LEVEL:
				preUIInCache, preUI = self._create_ui(
					preNode.ui_name)
				preUI.set_visible(
					True) if preUIInCache else preUI.show_ui(**kwargs)
				self._cur_second_node = preNode
				return preNode
			return self._cur_full_screen_node
// 3. 当前界面是三级界面
		elif struct.ui_type == UIType.THIRD_LEVEL:
			return preNode
		return preNode

	def _back_ui(self, ui_name, **kwargs):
		// 关闭指定UI
		@param ui_name: 需要关闭的UI名称
		@param **kwargs: UI关闭时需要的参数
		@return: True/False
		//

// 1. 不可关闭当前状态根UI
//if ui_name in self._hsm_caches or ui_name not in self._ui_caches:
// return False
		if ui_name not in self._ui_caches:
			return False
		struct = UIDef.get_struct(ui_name)
// 2. 对于全屏非透明类UI，只允许关闭当前UI
		if struct.ui_type == UIType.FULL_SCREEN and ui_name != self.cur_uiname:
			return False
// 3. 关闭指定UI
		self._close_ui(ui_name, **kwargs)
// 4. 更新浏览链表
		node = self._seek_treenode(ui_name)
// 4.1 关闭的是当前UI, 直接返回pre
		if node == self._cur_node:
			self._cur_node = self._back_treenode(struct, node, **kwargs)
			if self._cur_node:
				self._cur_node.next = None
// 4.2 关闭的非当前UI， 原则上应该不允许出现这种情况，这里还是处理下吧
		elif node is None:
			return True
		else:
			node.pre and setattr(node.pre, 'next', node.next)
			node.next and setattr(node.next, 'pre', node.pre)
		return True

	def _run_ui(self, ui_name, **kwargs):
		// 该函数主要用来配合GameController切换状态时使用的函数，该函数会进行UI的现场恢复
		@param ui_name: 需要显示的UI名称
		@param **kwargs: UI初始化或show_ui时的参数
		@return: True/False
		//
		struct = UIDef.get_struct(ui_name)
		if not struct:
			return
		self._remove_all()
		_, ui = self._create_ui(ui_name, **kwargs)
		self._hsm_caches.add(ui_name)
		ui_node = UITreeNode(ui_name)
		self._root_node = ui_node
		self._cur_node = ui_node
		self._cur_full_screen_node = self._cur_node
		ui.show_ui(**kwargs)
		ui.UI_NAME = ui_name
		return ui

	@property
	def cur_uiname(self):
		// 返回当前控制的UI名称
		//
		if self._cur_node is None:
			return None
		return self._cur_node.ui_name

	@property
	def cur_ui(self):
		// 返回当前控制的UI实例
		//
		return self._ui_caches.get(self.cur_uiname, None)

	def tick(self):
		// 所有UI的tick入口:
		1. 只允许当前显示的1,2,3级界面进行tick
		2. 其中3级界面只允许cur_ui执行tick
		3. 如有其他需求：请考虑使用register_tick_func或add_repeat_timer来实现
		//
		if self._cur_full_screen_node:
			fullUI = self._get_ui(self._cur_full_screen_node.ui_name)
			fullUI and fullUI.tick()
		if self._cur_second_node:
			secondUI = self._get_ui(self._cur_second_node.ui_name)
			secondUI and secondUI.tick()
		if self.cur_ui and self._cur_node not in [self._cur_full_screen_node, self._cur_second_node]:
			self.cur_ui.tick()
		map(lambda x: x(), self._tick_funs)

	def run_ui(self, ui_name, **kwargs):
		return self._run_ui(ui_name, **kwargs)

	def add_ui(self, ui_name, **kwargs):
		return self._add_ui(ui_name, **kwargs)

	def back_ui(self, ui_name, **kwargs):
		return self._back_ui(ui_name, **kwargs)

	def register(self, msg_type, func):
		self._message_maps.setdefault(msg_type, set()).add(func)

	def cancel(self, msg_type, func=None):
		if msg_type in self._message_maps:
			if func is None:
				del self._message_maps[msg_type]
			elif func in self._message_maps[msg_type]:
				self._message_maps[msg_type].remove(func)
				if not self._message_maps[msg_type]:
					del self._message_maps[msg_type]

	def receive(self, msg_type, *args, **kwargs):
		if msg_type in self._message_maps:
			res = None
			funcs = self._message_maps[msg_type]
// 一个元素的set大小变化也会有问题，所以去除大小判断
			funcs = set.copy(funcs)
			for func in funcs:
				func_res = func(*args, **kwargs)
				res = func_res if func_res is not None else res
			return res
		return None

	def register_tick_func(self, func):
		// 注册需要tick执行的函数
		//
		if func not in self._tick_funs:
			self._tick_funs.append(func)

	def remove_tick_func(self, func):
		// 移除已注册的tick函数
		//
		if func in self._tick_funs:
			self._tick_funs.remove(func)

	def on_ad_backbutton_pressed(self):
		// Android返回键事件
		@return: TODO
		//
		self.cur_ui.close_ui()
		return True

}
*/