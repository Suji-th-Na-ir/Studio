mergeInto(LibraryManager.library, 
{
    // JavaScript function to open IndexedDB
    OpenIndexedDB: function () 
    {
        var dbName = "StudioEditorDB";
        var storeName = "StudioStore";
        var request = window.indexedDB.open(dbName, 1);
        request.onerror = function (event) 
        {
            console.error("Error opening IndexedDB:", event.target.error);
        };
        request.onupgradeneeded = function (event) 
        {
            var db = event.target.result;
            db.createObjectStore(storeName);
        };
    },

    // JavaScript function to save data to IndexedDB
    SaveData: function (key, value, functionPtr) 
    {
        var dbName = "StudioEditorDB";
        var storeName = "StudioStore";
        var request = window.indexedDB.open(dbName, 1);
        var newKey = UTF8ToString(key)
        var newValue = UTF8ToString(value)
        request.onsuccess = function (event) 
        {
            var db = event.target.result;
            var transaction = db.transaction([storeName], "readwrite");
            var objectStore = transaction.objectStore(storeName);
            var saveRequest = objectStore.put(newValue, newKey);
            saveRequest.onsuccess = function (event) 
            {
                Module['dynCall_vi'](functionPtr, 1);
            };
            saveRequest.onerror = function (event) 
            {
                Module['dynCall_vi'](functionPtr, 0);
            };
        };
        request.onerror = function (event) 
        {
            Module['dynCall_vi'](functionPtr, 0);
        };
    },

    // JavaScript function to get data from IndexedDB
    GetData: function (key, functionPtr) 
    {
        var dbName = "StudioEditorDB";
        var storeName = "StudioStore";
        var newKey = UTF8ToString(key)
        var request = window.indexedDB.open(dbName, 1);
        request.onsuccess = function (event) 
        {
            var db = event.target.result;
            var transaction = db.transaction([storeName], "readonly");
            var objectStore = transaction.objectStore(storeName);
            var getRequest = objectStore.get(newKey);
            var emptyBufferSize = lengthBytesUTF8("") + 1;
            var emptyBuffer = _malloc(emptyBufferSize);
            stringToUTF8("", emptyBuffer, emptyBufferSize);
            getRequest.onsuccess = function (event) 
            {
                var result = event.target.result;
                if (result) 
                {
                    var bufferSize = lengthBytesUTF8(result) + 1;
                    var buffer = _malloc(bufferSize);
                    stringToUTF8(result, buffer, bufferSize);
                    Module['dynCall_vi'](functionPtr, [buffer]);
                }
                else
                {
                    Module['dynCall_vi'](functionPtr, [emptyBuffer]);
                }
            };

            getRequest.onerror = function (event) 
            {
                Module['dynCall_vi'](functionPtr, [emptyBuffer]);
            };
        };

        request.onerror = function (event) 
        {
            Module['dynCall_vi'](functionPtr, [emptyBuffer]);
        };
    },

    RemoveData: function (key, functionPtr) {
        var dbName = "StudioEditorDB";
        var storeName = "StudioStore";
        var newKey = UTF8ToString(key);
        var request = window.indexedDB.open(dbName, 1);
        request.onsuccess = function (event) {
            var db = event.target.result;
            var transaction = db.transaction([storeName], "readwrite");
            var objectStore = transaction.objectStore(storeName);
            var deleteRequest = objectStore.delete(newKey);
            deleteRequest.onsuccess = function (event) {
                Module['dynCall_vi'](functionPtr, 1);
            };
            deleteRequest.onerror = function (event) {
                Module['dynCall_vi'](functionPtr, 0);
            };
        };
        request.onerror = function (event) {
            Module['dynCall_vi'](functionPtr, 0);
        };
    },

    RenameKey: function (oldKey, newKey, functionPtr) {
        var dbName = "StudioEditorDB";
        var storeName = "StudioStore";
        var oldKeyName = UTF8ToString(oldKey);
        var newKeyName = UTF8ToString(newKey);

        var request = window.indexedDB.open(dbName, 1);
        request.onsuccess = function (event) {
            var db = event.target.result;
            var transaction = db.transaction([storeName], "readwrite");
            var objectStore = transaction.objectStore(storeName);
            var getRequest = objectStore.get(oldKeyName);
            getRequest.onsuccess = function (event) {
                var data = event.target.result;
                var deleteRequest = objectStore.delete(oldKeyName);
                deleteRequest.onsuccess = function () {
                    var addRequest = objectStore.add(data, newKeyName);
                    addRequest.onsuccess = function () {
                        Module['dynCall_vi'](functionPtr, 1);
                    };
                    addRequest.onerror = function () {
                        Module['dynCall_vi'](functionPtr, 0);
                    };
                };
                deleteRequest.onerror = function () {
                    Module['dynCall_vi'](functionPtr, 0);
                };
            };

            getRequest.onerror = function () {
                Module['dynCall_vi'](functionPtr, 0);
            };
        };

        request.onerror = function () {
            Module['dynCall_vi'](functionPtr, 0);
        };
    }
});