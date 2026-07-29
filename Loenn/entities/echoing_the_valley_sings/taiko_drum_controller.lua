local objectDepths = require("consts.object_depths")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local taikoDrumController = {}

taikoDrumController.name = "aonHelper/TaikoDrumController"
taikoDrumController.texture = "objects/aonHelper/taikoDrum/taikoDrumController"
taikoDrumController.placements = {
    name = "taiko_drum_controller",
    data = {
        soundWaveSpeed = 200.0,
        soundWaveDepth = -10501,
        soundWaveColor = "f3dbc5",
        affectedEntities = "",
        global = false
    }
}

taikoDrumController.fieldOrder = {
    "x", "y",
    "soundWaveSpeed", "soundWaveDepth", "soundWaveColor",
    "affectedEntities",
    "global"
}
function taikoDrumController.fieldInformation()
    return {
        soundWaveSpeed = {
         minimumValue = 0.0
        },
        soundWaveDepth = {
            fieldType = "integer",
            options = objectDepths,
            editable = true
        },
        soundWaveColor = {
            fieldType = "color"
        },
        affectedEntities = {
            fieldType = "list",
            elementSeparator = ",",
            elementDefault = "",
            elementOptions = {
                options = aonHelper.getAllSIDs(),
                searchable = true
            }
        }
    }
end

return aonHelper.controllerify(taikoDrumController)
