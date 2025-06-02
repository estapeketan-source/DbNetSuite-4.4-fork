var ChartDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"chart");
		this.url = null;
		this.img = null;
	},
	
	configure : function() {
		this._super();
	
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".copy-button").bind("click",this.createDelegate(this.copy));
		this.img = this.dialog.find(".chart-dialog-image");
		this.img.bind("load", this.createDelegate(this.sd));
		this.bind("onBeforeOpen",this.createDelegate(this.assignUrl));
		this.open();
	},
	
	sd : function() {
		window.setTimeout(this.createDelegate(this.sizeDialogToContent2),1);
	},
	
	sizeDialogToContent2 : function() {
		this.sizeDialogToContent();
	},
	
	assignUrl : function() {
		this.img.attr("src", this.url);
	}	
	
});
