local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local fgStylegroundBloomController = {}

fgStylegroundBloomController.name = "aonHelper/FgStylegroundBloomController"
fgStylegroundBloomController.texture = "objects/aonHelper/fgStylegroundBloomController"
fgStylegroundBloomController.placements = {
    {
        name = "fg_styleground_bloom_controller",
        data = {
            bloomTag = "",
            global = true
        }
    }
}

fgStylegroundBloomController.fieldOrder = {
    "x", "y",
    "bloomTag",
    "global"
}

return aonHelper.controllerify(fgStylegroundBloomController, aonHelper.globalByDefault)
