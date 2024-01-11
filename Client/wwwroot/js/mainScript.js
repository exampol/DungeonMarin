window.JsFunctions = {
    addKeyboardListenerEvent: function (foo) {
        let serializeEvent = function (e) {
            if (e) {
                return {
                    key: e.key,
                    code: e.keyCode.toString(),
                    location: e.location,
                    repeat: e.repeat,
                    ctrlKey: e.ctrlKey,
                    shiftKey: e.shiftKey,
                    altKey: e.altKey,
                    metaKey: e.metaKey,
                    type: e.type
                };
            }
        };

        window.document.addEventListener('keydown', function (e) {
            console.log('Chiamo il c#');
            DotNet.invokeMethodAsync('BlazorD.Client', 'KeyboardEventAsync', serializeEvent(e))
        });
    }
};