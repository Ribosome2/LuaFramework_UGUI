---@class RichTextLineInfo
RichTextLineInfo = class('RichTextLineInfo')

function RichTextLineInfo:ctor()
    self.widgetList ={}
end

function RichTextLineInfo:AddWidget(widgetInfo)
    table.insert(self.widgetList,widgetInfo)
end

function RichTextLineInfo:ParseLine(line)
    local lineLength = string.len(line)
    local matchIndex =1 
    while (matchIndex < lineLength) do
        -- 例子： "some text <graphic=image  width=100 height=200 type=1 id=5/>")
        local startIndex , endIndex, contentType , width,height,itemType,itemId =line:find( "<%s*graphic=(%w+)%s+width=([%d.]+)%s+height=([%d.]+)%s+type=(%d+)%s+id=(%d+)%s*%/>",matchIndex)
        if startIndex==nil then
            local textWidget = RichTextWidgetInfo.New()
            textWidget:SetTextContent(string.sub(line,matchIndex))
            self:AddWidget(textWidget)
            break
        else
            if(startIndex>matchIndex) then --匹配到图片内容前 还有文字
                local textWidget = RichTextWidgetInfo.New()
                textWidget:SetTextContent(string.sub(line,matchIndex,startIndex-1))
                self:AddWidget(textWidget)
            end
            local graphicWidget = RichTextWidgetInfo.New()
            graphicWidget:SetWidth(tonumber(width))
            graphicWidget:SetHeight(tonumber(height))
            graphicWidget:SetItemType(tonumber(itemType))
            graphicWidget:SetItemId(tonumber(itemId))
            graphicWidget:SetGraphicContentType(contentType)
            self:AddWidget(graphicWidget)
            --print("find match ",startIndex , endIndex, contentType , width,height)
            matchIndex = endIndex+1
        end
    end
end

