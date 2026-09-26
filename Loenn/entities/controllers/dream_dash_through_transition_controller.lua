local aonHelper = require("mods").requireFromPlugin("libraries.aon_helper")

local dreamDashThroughTransitionController = {}

dreamDashThroughTransitionController.name = "aonHelper/DreamDashThroughTransitionController"
dreamDashThroughTransitionController.texture = "objects/aonHelper/dreamDashThroughTransitionController"
dreamDashThroughTransitionController.placements = {
    {
        name = "dream_dash_through_transition_controller",
        data = {
            flag = "",
            global = true
        }
    }
}

dreamDashThroughTransitionController.fieldOrder = {
    "x", "y",
    "flag", "global"
}

return aonHelper.controllerify(dreamDashThroughTransitionController, aonHelper.globalByDefault)
