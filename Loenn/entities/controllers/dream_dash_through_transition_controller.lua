local dreamDashThroughTransitionController = {}

dreamDashThroughTransitionController.name = "aonHelper/DreamDashThroughTransitionController"
dreamDashThroughTransitionController.texture = "objects/aonHelper/dreamDashThroughTransitionController"
dreamDashThroughTransitionController.depth = 0
dreamDashThroughTransitionController.placements = {
    {
        name = "dream_dash_through_transition_controller",
        data = {
            flag = ""
        }
    }
}

dreamDashThroughTransitionController.fieldOrder = {
    "x", "y",
    "flag"
}

return dreamDashThroughTransitionController
