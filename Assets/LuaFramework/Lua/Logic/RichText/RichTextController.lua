
RichTextController = SingletonClass('RichTextController')

function RichTextController.CheckGraphicText(text )
    local startIndex =text:find( "<%s*graphic=(%w+)%s+width=([%d.]+)%s+height=([%d.]+)%s+type=(%d+)%s+id=(%d+)%s*%/>")
    return startIndex~=nil
end