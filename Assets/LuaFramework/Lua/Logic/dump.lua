local snapshot = require "snapshot"
local snapshot_utils = require "logic.snapshot_utils"
local print_r = require "logic.print_r"
forcePrint =function(...)
    print(...)
end
local construct_indentation = snapshot_utils.construct_indentation

forcePrint("GC前, Lua内存为:", collectgarbage("count"))
collectgarbage("collect")
-- 首先统计Lua内存占用的情况
forcePrint("GC后, Lua内存为:", collectgarbage("count"))
local S1 = snapshot.snapshot()

local testData = {kk=23,hh=13}
local testDat222a = {k11k=23,hh=13}
kkk={}
forcePrint("snapshot后, Lua内存为:", collectgarbage("count"))
local S2 = snapshot.snapshot()
print("second")

collectgarbage("collect")
local diff = {}
for k,v in pairs(S2) do
    if not S1[k] then
        diff[k] = v
    end
end

local construct_indentation = snapshot_utils.construct_indentation
local result = construct_indentation(diff)
print_r(result)
S1 =nil
S2 =nil
collectgarbage("collect")
forcePrint("释放snapshot后, Lua内存为:", collectgarbage("count"))