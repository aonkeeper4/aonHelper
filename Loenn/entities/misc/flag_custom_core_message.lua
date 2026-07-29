local flagCustomCoreMessage = {}

flagCustomCoreMessage.name = "aonHelper/FlagCustomCoreMessage"
flagCustomCoreMessage.texture = "objects/aonHelper/flagCustomCoreMessage"
flagCustomCoreMessage.placements = {
    {
        name = "core_message",
        data = {
            dialogID = "",
            lineNumber = 0,
            startFadeRadius = 96.0,
            endFadeRadius = 128.0,
            appearFlag = "",
            stayFlag = "",
            flagFadeTime = 0.4,
            useRawDeltaTime = false,
            textColor = "ffffff",
            hasOutline = true,
            outlineColor = "000000",
            outlineThickness = 2.0,
            scale = 1.25,
            parallaxX = 0.2,
            parallaxY = 0.2,
            hideOnHeartCollect = true
        }
    }
}

flagCustomCoreMessage.fieldOrder = {
    "x", "y",
    "dialogID", "lineNumber",
    "startFadeRadius", "endFadeRadius",
    "appearFlag", "stayFlag", "flagFadeTime", "useRawDeltaTime",
    "textColor", "outlineColor", "outlineThickness",
    "scale", "parallaxX", "parallaxY",
    "hideOnHeartCollect"
}
flagCustomCoreMessage.fieldInformation = {
    lineNumber = {
        fieldType = "integer",
        minimumValue = 0
    },
    startFadeRadius = {
        minimumValue = 0.0
    },
    endFadeRadius = {
        minimumValue = 0.0
    },
    flagFadeTime = {
        minimumValue = 0.0
    },
    textColor = {
        fieldType = "color"
    },
    outlineColor = {
        fieldType = "color"
    },
    outlineThickness = {
        minimumValue = 0.0
    },
    scale = {
        minimumValue = 0.0
    }
}

return flagCustomCoreMessage