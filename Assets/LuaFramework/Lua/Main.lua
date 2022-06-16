--主入口函数。从这里开始lua逻辑
myTable={11,"dd",seee={222,444}}
function Main()					
	print("logic start")	 		
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
end