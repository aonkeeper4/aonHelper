local disableAutoCameraOffsetController = {}

disableAutoCameraOffsetController.name = "aonHelper/DisableAutoCameraOffsetController"
disableAutoCameraOffsetController.texture = "objects/aonHelper/disableAutoCameraOffsetController"
disableAutoCameraOffsetController.placements = {
    name = "disable_auto_camera_offset_controller",
    data = {
        flag = ""
    }
}

disableAutoCameraOffsetController.fieldOrder = {
    "x", "y", "flag"
}

return disableAutoCameraOffsetController