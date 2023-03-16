require "Common/define"

require "3rd/pblua/login_pb"
--require "3rd/pbc/protobuf"

--local sproto = require "3rd/sproto/sproto"
--local core = require "sproto.core"
--local print_r = require "3rd/sproto/print_r"
PromptCtrl = {};
local this = PromptCtrl;

local panel;
local prompt;
local transform;
local gameObject;
local someLocalData = {
    122, 123
}

--构建函数--
function PromptCtrl.New()
    logWarn("PromptCtrl.New--->>");
    return this;
end

function PromptCtrl.Awake()
    logWarn("PromptCtrl.Awake--->>");
    panelMgr:CreatePanel('Prompt', this.OnCreate);
end

--启动事件--
function PromptCtrl.OnCreate(obj)
    gameObject = obj;
    transform = obj.transform;

    panel = transform:GetComponent('UIPanel');
    prompt = transform:GetComponent('LuaBehaviour');
    logWarn("Start lua--->>" .. gameObject.name);

    prompt:AddClick(PromptPanel.btnOpen, this.OnClick);
    prompt:AddClick(PromptPanel.btnSnapShot, this.OnClickSnapshot);
    prompt:AddClick(PromptPanel.btnDoSth, this.OnClickSnapshot2);
    resMgr:LoadPrefab('prompt', { 'PromptItem' }, this.InitPanel);
end

local preLuaSnapshot = nil
local function snapshotLuaMemory(sender, menu, value)
    -- 首先统计Lua内存占用的情况
    print("GC前, Lua内存为:", collectgarbage("count"))
    -- collectgarbage()
    -- print("GC后, Lua内存为:", collectgarbage("count"))
    local snapshot = require "snapshot"
    print("snaoshot ", snapshot)
    --print("snaoshot--- ",snapshot.snaoshot)
    --local curLuaSnapshot = snapshot.snapshot()
    --local ret = {}
    --local count = 0
    --if preLuaSnapshot ~= nil then
    --    for k,v in pairs(curLuaSnapshot) do
    --        if preLuaSnapshot[k] == nil then
    --            count = count + 1
    --            ret[k] = v
    --        end
    --    end
    --end
    --
    --for k, v in pairs(ret) do
    --    print(k)
    --    print(v)
    --end
    --
    --print ("Lua snapshot diff object count is " .. count)
    --preLuaSnapshot = curLuaSnapshot

end

function PromptCtrl.OnClickSnapshot()
    logError("snapshotClick")
    require("logic.dump")
    snapshotLuaMemory()
end

function PromptCtrl.OnClickSnapshot2()
    logError("snapshotClick2")
    collectgarbage("collect")
    local timeStr = os.date('%Y-%m-%d-%H-%M-%S')
    local logPath = string.format('luaSnapshots/luaSnapshotSimple_%s_%s.json', timeStr, tostring("keyValues"))
    local file = io.open(logPath, "w+")
    if file==nil then
        logError("路径创建失败 ",logPath)
    end

    print("GC前, Lua内存为:", collectgarbage("count"))
    -- collectgarbage()
    local snapshot = require "snapshot"
    local curLuaSnapshot = snapshot.snapshot()
    local tinsert = table.insert
    local snapshotRecord = { dataList = {} }

    for k, v in pairs(curLuaSnapshot) do
        --这里的type(v)是没有用的，都是string, 要对比是否存在还是用key 去做唯一性判断
        tinsert(snapshotRecord.dataList, { key=tostring(k), value= tostring(v)})
    end


    local cjson = require "cjson"
    file:write(cjson.encode(snapshotRecord))
    io.close(file)

    collectgarbage("collect")
    collectgarbage("collect")
    collectgarbage("collect")
    print("snapshot, Lua内存为:", collectgarbage("count"))

end

--初始化面板--
function PromptCtrl.InitPanel(objs)
    local count = 100;
    local parent = PromptPanel.gridParent;
    for i = 1, count do
        local go = newObject(objs[0]);
        go.name = 'Item' .. tostring(i);
        go.transform:SetParent(parent);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        prompt:AddClick(go, this.OnItemClick);

        local label = go.transform:Find('Text');
        label:GetComponent('Text').text = tostring(i);
    end
end


--滚动项单击--
function PromptCtrl.OnItemClick(go)

    GameObject.Instantiate(go)
    log(go.name);
end

--单击事件--
function PromptCtrl.OnClick(go)
    if TestProtoType == ProtocalType.BINARY then
        this.TestSendBinary();
    end
    if TestProtoType == ProtocalType.PB_LUA then
        this.TestSendPblua();
    end
    if TestProtoType == ProtocalType.PBC then
        this.TestSendPbc();
    end
    if TestProtoType == ProtocalType.SPROTO then
        this.TestSendSproto();
    end
    logWarn("OnClick---->>>" .. go.name);

    logWarn("Awake lua--1111->>" .. gameObject.name);
    print("myRable", myTable, " ", myTable[1])
    myTable.secondTable[1] = myTable.secondTable[1] + 10

    if RemoteCodeControl.TCPTestClient.Instance:IsConnected() then
        RemoteCodeControl.TCPTestClient.Instance:SendMessageToServer("This is from lua")
    else
        RemoteCodeControl.TCPTestClient.Instance:ConnectToTcpServer("127.0.0.1", 8052)
    end

end

--测试发送SPROTO--
function PromptCtrl.TestSendSproto()
    local sp = sproto.parse [[
    .Person {
        name 0 : string
        id 1 : integer
        email 2 : string

        .PhoneNumber {
            number 0 : string
            type 1 : integer
        }

        phone 3 : *PhoneNumber
    }

    .AddressBook {
        person 0 : *Person(id)
        others 1 : *Person
    }
    ]]

    local ab = {
        person = {
            [10000] = {
                name = "Alice",
                id = 10000,
                phone = {
                    { number = "123456789", type = 1 },
                    { number = "87654321", type = 2 },
                }
            },
            [20000] = {
                name = "Bob",
                id = 20000,
                phone = {
                    { number = "01234567890", type = 3 },
                }
            }
        },
        others = {
            {
                name = "Carol",
                id = 30000,
                phone = {
                    { number = "9876543210" },
                }
            },
        }
    }
    local code = sp:encode("AddressBook", ab)
    ----------------------------------------------------------------
    local buffer = ByteBuffer.New();
    buffer:WriteShort(Protocal.Message);
    buffer:WriteByte(ProtocalType.SPROTO);
    buffer:WriteBuffer(code);
    networkMgr:SendMessage(buffer);
end

--测试发送PBC--
function PromptCtrl.TestSendPbc()
    local path = Util.DataPath .. "lua/3rd/pbc/addressbook.pb";

    local addr = io.open(path, "rb")
    local buffer = addr:read "*a"
    addr:close()
    protobuf.register(buffer)

    local addressbook = {
        name = "Alice",
        id = 12345,
        phone = {
            { number = "1301234567" },
            { number = "87654321", type = "WORK" },
        }
    }
    local code = protobuf.encode("tutorial.Person", addressbook)
    ----------------------------------------------------------------
    local buffer = ByteBuffer.New();
    buffer:WriteShort(Protocal.Message);
    buffer:WriteByte(ProtocalType.PBC);
    buffer:WriteBuffer(code);
    networkMgr:SendMessage(buffer);
end

--测试发送PBLUA--
function PromptCtrl.TestSendPblua()
    local login = login_pb.LoginRequest();
    login.id = 2000;
    login.name = 'game';
    login.email = 'jarjin@163.com';
    local msg = login:SerializeToString();
    ----------------------------------------------------------------
    local buffer = ByteBuffer.New();
    buffer:WriteShort(Protocal.Message);
    buffer:WriteByte(ProtocalType.PB_LUA);
    buffer:WriteBuffer(msg);
    networkMgr:SendMessage(buffer);
end

--测试发送二进制--
function PromptCtrl.TestSendBinary()
    local buffer = ByteBuffer.New();
    buffer:WriteShort(Protocal.Message);
    buffer:WriteByte(ProtocalType.BINARY);
    buffer:WriteString("ffff我的ffffQ靈uuu");
    buffer:WriteInt(200);
    networkMgr:SendMessage(buffer);
end

--关闭事件--
function PromptCtrl.Close()
    panelMgr:ClosePanel(CtrlNames.Prompt);
end

print("-----------------------PromptCtrl")