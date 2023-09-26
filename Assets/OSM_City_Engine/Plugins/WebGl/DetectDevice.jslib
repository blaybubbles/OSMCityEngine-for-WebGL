 var DetectDevicePlugin = {
     IsMobile: function()
     {
		console.log("isMobile "+  /iPhone|iPad|iPod|Android/i.test(navigator.userAgent));
		return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
         //return UnityLoader.SystemInfo.mobile;
     }
 };
 
 mergeInto(LibraryManager.library, DetectDevicePlugin);