var BrowseDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"browse");
		this.loaded = false;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.panel = this.dialog.find(".browse-panel");

		if (!jQuery.browser.safari)
			this.open();
	
		this.parentControl.configureBrowseList(true);
		with(this.parentControl.browseList)
		{
			width = this.parentControl.browseDialogWidth;
			height = this.parentControl.browseDialogHeight;
			bind("onItemsLoaded", this.createDelegate(this.listLoaded));
			initialize();
		}
		
	},
	
	selectRow : function() {
		this.parentControl.browseList.selectRowByIndex(this.parentControl.currentPage);
	},		
	
	listLoaded : function() {
		if (jQuery.browser.safari)
			this.open();
		this.resize();
		this.selectRow();
	}	
});
