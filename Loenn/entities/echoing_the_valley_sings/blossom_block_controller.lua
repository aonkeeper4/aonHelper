local celesteEnums = require("consts.celeste_enums")
local objectDepths = require("consts.object_depths")
local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local blossomBlockController = {}

blossomBlockController.name = "aonHelper/BlossomBlockController"
blossomBlockController.texture = "objects/aonHelper/blossomBlock/blossomBlockController"
blossomBlockController.placements = {
    name = "blossom_block_controller",
    data = {
        spritePath = "",
        surfaceIndex = 33,
        particleColor1 = "ff94af",
        particleColor2 = "e1417f",
        ambientParticleDirection = 150.0,
        minSwirlRadius = 0.0,
        maxSwirlRadius = 2.0,
        minSwirlSpeed = 60.0,
        maxSwirlSpeed = 120.0,
        affectedDepth = -9000,
        global = false
    }
}

blossomBlockController.fieldOrder = {
    "x", "y",
    "spritePath", "surfaceIndex",
    "particleColor1", "particleColor2", "ambientParticleDirection",
    "minSwirlRadius", "maxSwirlRadius", "minSwirlSpeed", "maxSwirlSpeed",
    "affectedDepth",
    "global"
}
blossomBlockController.fieldInformation = {
    surfaceIndex = {
        fieldType = "integer",
        options = celesteEnums.tileset_sound_ids,
        editable = true
    },
    particleColor1 = {
        fieldType = "color"
    },
    particleColor2 = {
        fieldType = "color"
    },
    minSwirlRadius = {
        minimumValue = 0.0
    },
    maxSwirlRadius = {
        minimumValue = 0.0
    },
    affectedDepth = {
        fieldType = "integer",
        options = objectDepths,
        editable = true
    }
}

return aonHelper.controllerify(blossomBlockController)