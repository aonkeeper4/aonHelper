local nonPersistentFlagTrigger = {}

nonPersistentFlagTrigger.name = "aonHelper/NonPersistentFlagTrigger"
nonPersistentFlagTrigger.placements = {
    name = "non_persistent_flag",
    data = {
        flag = "",
        value = true,
        oneUse = false
    }
}

nonPersistentFlagTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "flag",
    "value", "oneUse"
}

return nonPersistentFlagTrigger
