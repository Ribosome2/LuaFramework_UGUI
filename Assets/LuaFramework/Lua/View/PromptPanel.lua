local transform;
local gameObject;

PromptPanel = {};
local this = PromptPanel;
myTable={11,"dd",seee={222,444,4.34}}
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