mergeInto(LibraryManager.library, {
  JS_FileSystem_SyncPush: function () {
    FS.syncfs(false, function (err) {
      if (err) {
        console.error('FS.syncfs push error:', err);
      }
    });
  },
  JS_FileSystem_SyncPull: function (gameObjectNamePtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);
    FS.syncfs(true, function (err) {
      if (err) {
        console.error('FS.syncfs pull error:', err);
      }
      SendMessage(gameObjectName, 'OnPullCompleted', '');
    });
  },
});
