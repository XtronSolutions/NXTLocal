mergeInto(LibraryManager.library, {
	IsMobile:function(){
	console.log("returing information");
	return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
  }
});
