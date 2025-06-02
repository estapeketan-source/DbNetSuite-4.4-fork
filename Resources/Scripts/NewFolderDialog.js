var NewFolderDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"new-folder");
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".standard-search-link").bind("click",this.createDelegate(this.standardSearch));

		this.open();
	},
	
	standardSearch : function(event) {
		this.close();
		this.parentControl.openStandardSearchDialog();
	},		
	
	apply : function(event) {
		if (this.folderName() == "")
			return;
		
        var data = {folderName : this.folderName() };
		data.currentFolder = this.parentControl.currentFolder;
		this.parentControl.callServer("create-folder", { method : this.applyCallback, context : this}, data);
	},	
	
	applyCallback : function(response) {
		if (response.message != "")
		{
			this.showMessage(response.message);
			return;
		}

		this.close();
		this.parentControl.reloadFolder(true);
	},
	
	folderName : function() {
		return this.dialog.find(".new-folder-name").val();
	}
});
