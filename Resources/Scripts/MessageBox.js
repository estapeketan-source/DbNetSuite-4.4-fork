var MessageBox = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"message-box");
		this.type = "";
		this.message = "";
		this.callback = null;
		this.buttonPressed = "";
	},
	
	configure : function() {
		this._super();
		this.dialog.bind("dialogclose", this.createDelegate(this.messageBoxClosed));
		this.open();
	},	
	
	open : function() {
		if (this.type == "info" || this.callback == undefined)
			this.dialog.find(".dbnetsuite-dialog-button").hide()
		else
			this.dialog.find(".dbnetsuite-dialog-button").show()
	
		this.dialog.find(".dbnetsuite-dialog-button").unbind().bind("click",this.createDelegate(this.close));
	
		var e = this.dialog.find(".dbnetsuite-message-box-text");
		e.html( this.translate(this.message) );
		
		e.css("white-space","nowrap").css("width", ( e.text().length < 80 ) ? "" : "300px");

		this.dialog.find(".dbnetsuite-message-box-icon").hide();
		this.dialog.find("[iconType='" + this.type + "']").show();	
		this.buttonPressed = "";	
		
		this.dialog.dialog("open");
		window.setTimeout(this.createDelegate(this.resizeDialog),100);
	},	
	
	resizeDialog : function(event) {
		this.sizeDialogToContent(true);
	},		
	
	close : function(event) {
		if (event)
		{
			var button = this.parentElement(event.target,"button");
			this.buttonPressed = this.$(button).attr("buttonType");
		}
		this.dialog.dialog("close");
	},	
	
	messageBoxClosed : function() {
		if (this.callback)
			this.invokeCallback(this.callback,[this.buttonPressed]);
	}			      	
	
});
