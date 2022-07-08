if globalFunctionTrapMap==nil then globalFunctionTrapMap={} end
print("before------------ ",getmetatable(UnityEngine.GameObject)['Instantiate'])
globalFunctionTrapMap['UnityEngine.GameObject.Instantiate'] =getmetatable(UnityEngine.GameObject)['Instantiate']
getmetatable(UnityEngine.GameObject)['Instantiate']=function(...)
    print('lua call--- ' ,'UnityEngine.GameObject.Instantiate',debug.traceback())
    return globalFunctionTrapMap['UnityEngine.GameObject.Instantiate'](...)
end
print("after------------ ",getmetatable(UnityEngine.GameObject)['Instantiate'])