window.robotKeyboardShortcuts = {
    register: function (dotNetHelper) {
        window.robotKeyboardHandler = function (event) {
            const active = document.activeElement;
            const isTyping =
                active &&
                (
                    active.tagName === "INPUT" ||
                    active.tagName === "TEXTAREA" ||
                    active.isContentEditable
                );

            if (isTyping) {
                return;
            }

            const key = event.key.toLowerCase();

            if (["w", "a", "s", "d"].includes(key)) {
                dotNetHelper.invokeMethodAsync("HandleGlobalKeyboardInput", key);
            }

            if (key === "r" && event.shiftKey) {
                dotNetHelper.invokeMethodAsync("HandleGlobalKeyboardInput", "shift+r");
            }
        };

        window.robotDialogFocus = {
            focusElementById: function (id) {
                setTimeout(() => {
                    const element = document.getElementById(id);

                    if (element) {
                        element.focus();
                    }
                }, 150);
            }
        };

        window.addEventListener("keydown", window.robotKeyboardHandler);
    },

    unregister: function () {
        if (window.robotKeyboardHandler) {
            window.removeEventListener("keydown", window.robotKeyboardHandler);
            window.robotKeyboardHandler = null;
        }
    }
};

window.robotDialogFocus = {
    focusElementById: function (elementId) {
        const element = document.getElementById(elementId);

        if (element) {
            element.focus();
        }
    }
};