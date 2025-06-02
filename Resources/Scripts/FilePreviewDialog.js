var FilePreviewDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"file-preview");
		this.url = "";
	},
	
	build : function(response) {
		this._super(response);
	},
	
	configure : function() {
		this.resizable = true;
	
		this._super();

		this.dialog.dialog("option", "width", this.parentControl.previewDialogWidth);
		this.dialog.dialog("option", "height", this.parentControl.previewDialogHeight);
		this.sizeSet = true;
			
		this.dialog.find("[buttonType='download']" ).bind("click",this.createDelegate(this.download));
		this.dialog.find("[buttonType='print']" ).bind("click",this.createDelegate(this.print));
		this.dialog.find("[buttonType='window']" ).bind("click",this.createDelegate(this.openInNewWindow));
		this.dialog.find("[buttonType='copy']" ).bind("click",this.createDelegate(this.copy));
		
		this.frame = window.frames[ this.parentControl.id + "_preview_dialog_frame"];
		
		this.bind("onBeforeOpen",this.createDelegate(this.setDialogSize));
		this.bind("onOpen",this.createDelegate(this.onResize));
		
//		jQuery("#" + this.parentControl.id + "_preview_dialog_frame").contents().find("body").css("margin","0px");
		this.open();
	},
	
	resize : function() {
		this._super();
		this.onResize();
	},
	
	onResize : function(event) {
		var h = this.dialog.parent().height() -35;
		var w = this.dialog.parent().width() -5;
		jQuery("#" + this.parentControl.id + "_preview_dialog_panel").height(h).width(w);
		jQuery("#" + this.parentControl.id + "_preview_dialog_frame").height(h-60).width(w);
	},		
	
	setDialogSize : function(response) {
		if (this.parentControl instanceof DbNetFile)
		{
			this.dialog.find(".caption" ).text(this.url);
			this.frame.location.href = this.parentControl.streamUrl(this.url);
		}
		else
		{
			this.frame.location.href = this.url;
		}
		
		var h = this.data.frameHeight;
		var w = this.data.frameWidth;
		
		this.dialog.dialog("option", "width", w + 40);
		this.dialog.dialog("option", "height", h + 130);
		this.dialog.dialog("option", "position", "center");
		
		if (this.isOpen())
			window.setTimeout(this.createDelegate(this.onResize),1);
	},
		
	download : function(event) {
		this.parentControl.downloadDocument(this.url);
	},
	
	print : function(event) {
		this.frame.focus();
		this.frame.print();
	},
	
	copy : function(event) {
		try
		{
			var textRange = this.frame.document.body.createTextRange();
			textRange.execCommand('Copy');
			this.showMessage(this.translate("DocumentCopied"));
		}
		catch(ex)
		{
			this.showMessage(this.translate("DocumentNotCopied"));
		}
	},
	
	openInNewWindow : function(event) {
		this.parentControl.displayDocument(this.url);
	}	
});
