---@class RichTextWidgetInfo
RichTextWidgetInfo = class('RichTextWidgetInfo')

E_RichTextWidgetType ={
    None =0,
    Text =1,
    Image=2,
    Sprite=3,
}

local contentTypeMap={
    ["image"]=E_RichTextWidgetType.Image,
    ["sprite"]=E_RichTextWidgetType.Sprite
}


function RichTextWidgetInfo:ctor(widgetType)
    self.widgetType = widgetType
    self.width = 0
    self.height = 0
    self.textContent = nil
    self.itemType =0
    self.itemId =0
end

function RichTextWidgetInfo:SetGraphicContentType(contentTypeStr)
    local t = contentTypeMap[contentTypeStr]
    self.widgetType = t
    assert(t,string.format("未支持类型%s",contentTypeStr))
end


function RichTextWidgetInfo:GetWidgetType()
	return self.widgetType
end

function RichTextWidgetInfo:GetWidth()
	return self.width
end

function RichTextWidgetInfo:SetWidth( width )
	self.width = width
end

function RichTextWidgetInfo:SetHeight( height )
	self.height = height
end

function RichTextWidgetInfo:GetHeight()
	return self.height
end

function RichTextWidgetInfo:SetTextContent( textContent )
    self.widgetType = E_RichTextWidgetType.Text
	self.textContent = textContent
    --print("Set text ",textContent)
end

function RichTextWidgetInfo:GetTextContent()
	return self.textContent
end

function RichTextWidgetInfo:SetItemType( itemType )
	self.itemType = itemType
end

function RichTextWidgetInfo:GetItemType()
	return self.itemType
end

function RichTextWidgetInfo:SetItemId( itemId )
	self.itemId = itemId
end

function RichTextWidgetInfo:GetItemId()
	return self.itemId
end