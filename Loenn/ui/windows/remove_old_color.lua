local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local languageRegistry = require("language_registry")
local utils = require("utils")
local widgetUtils = require("ui.widgets.utils")
local form = require("ui.forms.form")
local github = require("utils.github")
local configs = require("configs")
local meta = require("meta")

local windowPersister = require("ui.window_position_persister")
local windowPersisterName = "about_window"

local removeOldColor = {}

local aboutWindowGroup = uiElements.group({})

local noPaddingSpacing = {
    style = {
        spacing = 8,
        padding = 8
    }
}

function removeOldColor.showAboutWindow()
    local language = languageRegistry.getLanguage()
    local windowTitle = tostring(language.VivHelper.resolveOldColor.title)
    local gotIt = tostring(language.VivHelper.resolveOldColor.button)
    local description = tostring(language.VivHelper.resolveOldColor.info)

    local gotItButton = 

    local windowContent = uiElements.column({
        logoContainer,
        uiElements.label(versionText),
        uiElements.label(description),
        uiElements.label(credits),
        uiElements.button(gotIt, )
    }):with({
        style = {
            spacing = 8,
            padding = 8
        }
    })

    local window = uiElements.window(windowTitle, windowContent)
    local windowCloseCallback = windowPersister.getWindowCloseCallback(windowPersisterName)

    windowContent:layout()

    local logoOffsetX = math.floor((windowContent.innerWidth - logoWidth) / 2)

    logoContainer:with(uiUtils.fillWidth(false))
    gotItButton:with(uiUtils.fillWidth(false))
    logoElement:with(uiUtils.at(logoOffsetX, 0), logoContainer.style.padding)

    windowPersister.trackWindow(windowPersisterName, window)
    aboutWindowGroup.parent:addChild(window)
    widgetUtils.addWindowCloseButton(window, windowCloseCallback)
    form.prepareScrollableWindow(window)

    return window
end

return removeOldColor