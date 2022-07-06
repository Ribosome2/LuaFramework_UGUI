require('Commons.RichText.RichTextWidgetInfo')
require('Commons.RichText.RichTextLineInfo')
require('Commons.RichText.RichTextController')
---@class RichTextComponent
RichTextComponent = class('RichTextComponent')



--图文混排组件，区别于UIGraphicText组件，我们不用提前指定可用的图集信息，理论上可以用任意的图片
function RichTextComponent:ctor()
    self.curText = ""
    self.lines = {}  --把文字内容按换行符分隔的字符串数组
    self.widgetLines = {}  --构建每一行富文本所需要的信息

    self.txtTemplate = nil
    self.textComponents = {}
    self.imageComponents = {}
    self.lineSpacing = 0
    self.lineTransformList = {}
    self.totalHeight = 0
    self.contentRectTrans = nil
end
function RichTextComponent:SetText(str)
    if str ~= self.curText then
        self.curText = str
        self:Parse()
        self:RefreshGraphicNodes()
    end
end

---注意设置行间距，之会在下次改变文本内容的时候刷新
function RichTextComponent:SetLineSpacing(spacing)
    self.lineSpacing = spacing
end

function RichTextComponent:Parse()
    self.lines = {}
    for s in self.curText:gmatch("[^\r\n]+") do
        table.insert(self.lines, s)
    end
    self.widgetLines = {}
    for _, line in ipairs(self.lines) do
        local lineInfo = RichTextLineInfo.New()
        lineInfo:ParseLine(line)
        table.insert(self.widgetLines, lineInfo)
    end
end

function RichTextComponent:Awake()
    self.txtTemplate = goutil.GetText(self.gameObject, '')
    self.contentRectTrans = goutil.GetRectTransform(self.gameObject, '')
    self.graphicRoot = self:AddChildNode(self.gameObject,"contentRoot")

    assert(self.txtTemplate, "这个脚本应该挂在有Text组件的节点")
    self.txtTemplate.enabled = false
end

function RichTextComponent:AddChildNode(parentGo, nodeName)
    local go = GameObject.New(nodeName)
    local rectTrans = go:AddComponent(typeof(UnityEngine.RectTransform))
    go:SetParent(parentGo, false)
    return go, rectTrans
end

function RichTextComponent:RefreshGraphicNodes()
    if self.gameObject == nil then
        return
    end
    goutil.DestroyChildren(self.graphicRoot)
    self.lineTransformList = {}
    local totalHeight = 0
    for lineIndex, widgetLine in ipairs(self.widgetLines) do
        local lineGo, lineTran = self:AddChildNode(self.graphicRoot, string.format("widgetLine%s", lineIndex))
        table.insert(self.lineTransformList, lineTran)
        local xOffset = 0
        local maxHeight = 0
        for widgetIndex, widgetInfo in ipairs(widgetLine.widgetList) do
            local widgetTrans, widgetWidth, widgetHeight = self:AddWidget(lineGo, widgetInfo, lineIndex, widgetIndex)
            widgetTrans:SetAnchoredPos(xOffset + widgetWidth * 0.5, 0)
            xOffset = xOffset + widgetWidth
            maxHeight = math.max(maxHeight, widgetHeight)
            widgetTrans.sizeDelta = Vector2(widgetWidth, widgetHeight)
        end
        local lineRect = goutil.GetRectTransform(lineGo, "")
        lineRect.sizeDelta = Vector2(xOffset, maxHeight)
        lineRect:SetAnchoredPos(-xOffset * 0.5, -totalHeight - maxHeight * 0.5)
        totalHeight = totalHeight + maxHeight + self.lineSpacing
    end
    self.totalHeight = totalHeight
    self:AdjustAlignment()
end

function RichTextComponent:AdjustAlignment()
    --初始生成的位置都是按照水平居中，一行行向下长的规则，这里根据选择的对齐方式进行一些修正
    local alignment = self.txtTemplate.alignment
    if alignment == UnityEngine.TextAnchor.UpperLeft then
        self:AdjustHorizontal(-1, 1)
    elseif alignment == UnityEngine.TextAnchor.UpperCenter then
        self:AdjustHorizontal(0, 1)
    elseif alignment == UnityEngine.TextAnchor.UpperRight then
        self:AdjustHorizontal(1, 1)
    elseif alignment == UnityEngine.TextAnchor.MiddleLeft then
        self:AdjustHorizontal(-1, 0)
    elseif alignment == UnityEngine.TextAnchor.MiddleCenter then
        self:AdjustHorizontal(0, 0)
    elseif alignment == UnityEngine.TextAnchor.MiddleRight then
        self:AdjustHorizontal(1, 0)
    elseif alignment == UnityEngine.TextAnchor.LowerLeft then
        self:AdjustHorizontal(-1, -1)
    elseif alignment == UnityEngine.TextAnchor.LowerCenter then
        self:AdjustHorizontal(0, -1)
    elseif alignment == UnityEngine.TextAnchor.LowerRight then
        self:AdjustHorizontal(1, -1)
    else
        printError("unsupported alignment ", alignment)
    end
end

--xFactor 只能是-1 0 1  用来控制向左还是向右偏 yFactor 只能是-1 0 1  用来控制向上还是向下偏
function RichTextComponent:AdjustHorizontal(xFactor, yFactor)
    if #self.lineTransformList == 0 then
        return
    end

    local offsetToCenter = self.totalHeight * 0.5

    --每一行都需要根据行宽和文本Rect大小做
    local contentWidth = self.contentRectTrans.sizeDelta.x
    local contentHeight = self.contentRectTrans.sizeDelta.y
    local yOffset = (contentHeight - self.totalHeight) * 0.5
    for _, lineTran in ipairs(self.lineTransformList) do
        local oldX, oldY = lineTran:GetAnchoredPos(0, 0)
        local xOffset = (contentWidth - lineTran.sizeDelta.x) * 0.5
        lineTran:SetAnchoredPos(oldX + xOffset * xFactor, oldY + offsetToCenter + yFactor * yOffset)
    end
end

function RichTextComponent:AddWidget(lineGo, widgetInfo, lineIndex, widgetIndex)
    local widgetTrans
    local widgetWidth
    local widgetHeight
    local widgetType = widgetInfo:GetWidgetType()
    if widgetType == E_RichTextWidgetType.Text then
        local text, textRectTrans = self:AddTextNode(lineGo, widgetInfo, lineIndex, widgetIndex)
        widgetTrans = textRectTrans
        widgetWidth = text.preferredWidth
        widgetHeight = text.preferredHeight
    elseif widgetType == E_RichTextWidgetType.Image then
        local imageGo, imageRectTrans = self:AddChildNode(lineGo, string.format("image%s_%s", lineIndex, widgetIndex))
        local image = imageGo:AddComponent(typeof(UnityEngine.UI.Image))
        widgetTrans = imageRectTrans
        widgetWidth = widgetInfo:GetWidth()
        widgetHeight = widgetInfo:GetHeight()
        self:LoadIcon(widgetInfo, image)
    else
        printError("unsupported widget type", widgetType)
    end
    return widgetTrans, widgetWidth, widgetHeight
end

function RichTextComponent:AddTextNode(lineGo, widgetInfo, lineIndex, widgetIndex)
    local textGo, rectTrans = self:AddChildNode(lineGo, string.format("text%s_%s", lineIndex, widgetIndex))
    textGo:SetParent(lineGo, false)
    text = textGo:AddComponent(typeof(UnityEngine.UI.Text))
    text.font = self.txtTemplate.font
    text.fontSize = self.txtTemplate.fontSize
    text.text = widgetInfo:GetTextContent()
    text.alignment = UnityEngine.TextAnchor.MiddleCenter
    table.insert(self.textComponents, text)
    return text, rectTrans
end

function RichTextComponent:LoadIcon(widgetInfo, imageComp)
    MaterialService.GetMaterialIconByParams(widgetInfo:GetItemType(), widgetInfo:GetItemId(), function(icon, _)
        if goutil.IsNil(imageComp) == false and goutil.IsNil(icon) == false then
            imageComp.overrideSprite = icon
        end
    end)
end

function RichTextComponent:OnEnable()
end
function RichTextComponent:OnDisable()
    if self.delayLayoutCo ~= nil then
        self.delayLayoutCo = nil
    end
end


