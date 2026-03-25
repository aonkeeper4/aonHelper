local featherBounceScamController = {}

featherBounceScamController.name = "aonHelper/FeatherBounceScamController"
featherBounceScamController.texture = "objects/aonHelper/featherBounceScamController"
featherBounceScamController.depth = 0
featherBounceScamController.placements = {
    {
        name = "feather_bounce_scam_controller",
        data = {
            featherBounceScamThreshold = 0.2,
            flag = "",
        }
    }
}

featherBounceScamController.fieldOrder = {
    "x", "y",
    "featherBounceScamThreshold", "flag"
}

return featherBounceScamController
