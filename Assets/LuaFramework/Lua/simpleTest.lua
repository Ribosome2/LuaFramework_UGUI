local testStr = "#table\n0000000014570378 : S1 : logic/dump:36#"
print("----- ", testStr:match("0x(%w+) :"))
print("----- ", testStr:find("8(%w+) :"))
print("----- ", testStr:find("\n(%w+) :"))
print("----- ", testStr:match("\n(%w+) :"))
print("----- ", testStr:match("table\n(%w+) :"))