var ErrorDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"error");
		this.text = null;
		this.status = null;
	},
	
	build : function() {
		var o = {};
		var w = screen.availWidth * 0.8
		o.html = "<div id='errorDialog' class='dbnetsuite error-dialog' title='Error Dialog (" + this.parentControl.id + ") Status (" + this.status + ")'>" +
				"<div style='width:" + w.toString() + "px;height:500px;overflow-y:auto;' class='error-content'>" + this.text + "</div>" +
				"<div style='text-align:center'><button type='button'>OK</button></div>" +
				"</div>";

		this._super(o);
	},
	
	addContainer : function() {
		this.dialogContainer = jQuery("<div/>").appendTo("body");
		this.dialogContainer.hide();
		this.dialogContainer.addClass("dbnetsuite-" + this.dialogId + "-dialog");
	},	
	
	configure : function() {
		this._super();
		
		this.dialog.dialog("option", "width", (screen.availWidth * 0.8) + 10);
		
		this.dialog.find("button").bind("click",this.createDelegate(this.close));
		this.open();
	}		
});
