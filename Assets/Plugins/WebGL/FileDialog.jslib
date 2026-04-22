mergeInto(LibraryManager.library, {

    OpenFileDialog: function () {
        var input = document.createElement('input');
        input.type = 'file';
        input.accept = '.json';

        input.onchange = function (e) {
            var file = e.target.files[0];
            if (!file) return;

            var reader = new FileReader();
            reader.onload = function (evt) {
                var content = evt.target.result;

                // Send data back to Unity
                unityInstance.SendMessage(
                    'FileDialogReceiver',   // GameObject name
                    'OnFileLoaded',         // method name
                    content                // file text
                );
            };
            reader.readAsText(file);
        };

        input.click();
    },

    SaveFileDialog: function (filenamePtr, dataPtr) {
        var filename = UTF8ToString(filenamePtr);
        var data = UTF8ToString(dataPtr);

        var blob = new Blob([data], { type: "application/json" });

        var a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        a.click();
    }
});