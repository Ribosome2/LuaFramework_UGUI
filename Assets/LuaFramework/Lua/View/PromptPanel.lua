local transform;
local gameObject;

PromptPanel = {};
local this = PromptPanel;
myTable={11,"dd",
	secondTable={
		222,444,4.34,0.0005,
		third={"some Lua ,dd",333}
	},
	other={
		33,44,55
	},
}
print('this is lua=============')
--启动事件--
function PromptPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();

end

--初始化面板--
function PromptPanel.InitPanel()
	this.btnOpen = transform:Find("Open").gameObject;
	this.gridParent = transform:Find('ScrollView/Grid');
end

--单击事件--
function PromptPanel.OnDestroy()
	logWarn("OnDestroy---->>>");
end