-- i loove stealing shit from other people
-- ty limia :revolving_hearts:
-- what even is the proper casing for field types anyways

local ui = require("ui")
local uiElements = require("ui.elements")
local contextMenu = require("ui.context_menu")

local uiUtils = require("ui.utils")
local iconUtils = require("ui.utils.icons")

local form = require("ui.forms.form")
local stringField = require("ui.forms.fields.string")

local languageRegistry = require("language_registry")

local fakeTilesHelper = require("helpers.fake_tiles")

local tiletypeListField = {}
tiletypeListField.fieldType = "aon_helper.tiletype_list"

local defaultTiletype = "1"
local tiletypeOptions = {
    fieldType = "string",
    options = function() return fakeTilesHelper.getTilesOptions("tilesFg") end,
    editable = false
}

local function splitUtf8Chars(str)
    local chars = {}
    local startIndex = 0
    local strLen = str:len()

    local function bit(b)
        return 2 ^ (b - 1)
    end
    local function hasBit(w, b)
        return w % (2 * b) >= b
    end

    local function checkMultiByte(index)
        if (startIndex ~= 0) then
            chars[#chars + 1] = str:sub(startIndex, index - 1)
            startIndex = 0
        end
    end

    for i = 1, strLen do
        local b = str:byte(i)
        
        local multiStart = hasBit(b, bit(7)) and hasBit(b, bit(8))
        local multiTrail = not hasBit(b, bit(7)) and hasBit(b, bit(8))
        if multiStart then
            checkMultiByte(i)
            startIndex = i
        elseif not multiTrail then
            checkMultiByte(i)
            chars[#chars + 1] = str:sub(i, i)
        end
    end
    checkMultiByte(strLen + 1)

    return chars
end

local function getValueParts(value)
    if not value then return {} end
    return splitUtf8Chars(value)
end

local function updateContextWindow(formField)
    local content = tiletypeListField.buildContextMenu(formField)
    local contextWindow = formField.contextWindow

    if contextWindow and contextWindow.parent then
        contextWindow.children[1]:removeSelf()
        contextWindow:addChild(content)

        ui.hovering = content
        ui.focusing = content
    end
end

local function valueDeleteRowHandler(formField, index)
    return function()
        local value = formField:getValue()
        local field = formField.field
        local parts = getValueParts(value)

        table.remove(parts, index)

        local joined = table.concat(parts)
        field:setText(joined)
        updateContextWindow(formField)
    end
end

local function valueAddRowHandler(formField)
    return function()
        local value = formField:getValue()
        local field = formField.field
        local parts = getValueParts(value)

        table.insert(parts, defaultTiletype)

        local joined = table.concat(parts)
        field:setText(joined)
        updateContextWindow(formField)
    end
end

local function getSubFormElements(formField, value)
    local language = languageRegistry.getLanguage()
    
    local elements = {}
    local parts = getValueParts(value)

    local baseFormElement = form.getFieldElement("base", defaultTiletype, tiletypeOptions)
    local valueTransformer = baseFormElement.valueTransformer or tostring

    for i, part in ipairs(parts) do
        local formElement = form.getFieldElement(tostring(i), valueTransformer(part) or part, tiletypeOptions)

        if formElement.elements[1] == formElement.label then
            formElement.width = 1
            table.remove(formElement.elements, 1)
        end

        local removeButton = uiElements.button(
            tostring(language.forms.fieldTypes.list.removeButton),
            valueDeleteRowHandler(formField, i)
        )
        local fakeElement = {
            elements = {
                removeButton
            },
            fieldValid = function() return true end
        }

        table.insert(elements, formElement)
        table.insert(elements, fakeElement)
    end

    return elements
end

local function updateTextField(formField, formData)
    local data = {}
    for k, v in pairs(formData) do
        data[tonumber(k)] = v
    end

    local joined = table.concat(data)
    formField.field:setText(joined)
end

local function getFormDataStrings(fields)
    local data = {}
    for _, field in ipairs(fields) do
        if field.getValue then
            data[#data + 1] = field:getValue()
        end
    end

    return data
end

function tiletypeListField.updateSubElements(formField)
    if not formField then
        formField._subElements = {}
        return formField._subElements
    end

    local value = formField:getValue()
    if not value then
        formField._subElements = {}
        return formField._subElements
    end

    local previousValue = formField._previousValue
    local formElements = formField._subElements
    if value ~= previousValue then
        formElements = getSubFormElements(formField, value)
    end

    formField._subElements = formElements
    formField._previousValue = value
    return formElements
end

function tiletypeListField.buildContextMenu(formField)
    local language = languageRegistry.getLanguage()
    
    local formElements = formField._subElements
    local columnElements = {}

    if #formElements > 0 then
        local columnCount = (formElements[1].width or 0) + 1
        local formOptions = {
            columns = columnCount,
            formFieldChanged = function(fields)
                local data = getFormDataStrings(fields)

                formField.subFormValid = form.formValid(fields)
                updateTextField(formField, data)
            end,
        }

        form.prepareFormFields(formElements, formOptions)
        local formGrid = form.getFormFieldsGrid(formElements, formOptions)
        table.insert(columnElements, formGrid)
    end

    local addButton = uiElements.button(
        tostring(language.forms.fieldTypes.list.addButton),
        valueAddRowHandler(formField)
    )
    if #formElements > 0 then
        addButton:with(uiUtils.fillWidth(false))
    end
    table.insert(columnElements, addButton)

    local column = uiElements.column(columnElements)
    return column
end

local function addContextSpawner(formField)
    local field = formField.field
    local contextMenuOptions = {
        mode = "focused"
    }

    if field.height == -1 then
        field:layout()
    end

    local iconMaxSize = field.height - field.style.padding
    local parentHeight = field.height
    local listIcon, iconSize = iconUtils.getIcon("list", iconMaxSize)

    if listIcon then
        local centerOffset = math.floor((parentHeight - iconSize) / 2) + 1
        local listImage = uiElements.image(listIcon)
            :with(uiUtils.rightbound(-1))
            :with(uiUtils.at(0, centerOffset))

        listImage.interactive = 1
        listImage:hook({
            onClick = function(orig, self)
                orig(self)
                formField.contextWindow = contextMenu.showContextMenu(tiletypeListField.buildContextMenu(formField), contextMenuOptions)
            end
        })

        field:addChild(listImage)
    end
end

function tiletypeListField.getElement(name, value, options)
    local formField
    
    options = table.shallowcopy(options or {})
    options.validator = function()
        if not formField then return true end
    
        local subElements = tiletypeListField.updateSubElements(formField)
        return form.formValid(subElements)
    end

    formField = stringField.getElement(name, value, options)

    tiletypeListField.updateSubElements(formField)
    addContextSpawner(formField)

    return formField
end

return tiletypeListField