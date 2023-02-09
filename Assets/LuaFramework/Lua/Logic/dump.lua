local snapshot_utils = require "logic.snapshot_utils"
local construct_indentation = snapshot_utils.construct_indentation
local print_r = require "logic.print_r"

local S1 = snapshot.snapshot()

local tmp = {
    player = {
        uid = 1,
        camps = {
            {campid = 1},
            {campid = 2},
        },
    },
    player2 = {
        roleid = 2,
    },
    [3] = {
        player1 = 1,
    },
}

local a = {}
local c = {}
a.b = c
c.d = a

local msg = "bar"
local foo = function()
    print(msg)
end

local co = coroutine.create(function ()
    print("hello world")
end)

local S2 = snapshot.snapshot()

local diff = {}
for k,v in pairs(S2) do
	if not S1[k] then
        diff[k] = v
	end
end

print_r(diff)
print("===========================")

local result = construct_indentation(diff)
print_r(result)
