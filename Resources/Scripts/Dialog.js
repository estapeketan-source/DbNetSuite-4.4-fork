var Dialog = DbNetSuite.extend({
	init: function(parentControl, dialogId){
		this.parentControl = parentControl;
		this.dialogId = dialogId; 
		var id = parentControl.id + "_" + dialogId + "Dialog";
		this.id = id;
//		if (jQuery("#" + id).length > 0)
//			jQuery("#" + id).remove();

		this._super(id);
		this.data = {};
		this.dialog = null;

		this.assignAjaxUrl("dbnetgrid.ashx");
		//this.ajaxConfig.url = "dbnetgrid.ashx";

		this.position = null;
		this.widthAdj = (jQuery.browser ? ((jQuery.browser.msie) ? 4 : 9) : 9);
		this.yOffset = 0;
		this.resizable = false;
		this.modal = false;
		this.configured = false;
		this.options = {};
		this.component = null;
		this.sizeSet = false;
		this.closeOnApply = false;
		this.width = null;
		this.height = null;
		this.title = "";
		this.label = "";
	},
	
	open: function () {
		this.fireDialogEvent("onBeforeOpen");
		this.dialog.dialog("open");
		this.yOffset = parseInt(jQuery(this.dialog).offset().top);
	},	
		
	isOpen : function() {
		if (this.dialog)
			return this.dialog.dialog("isOpen");
		else
			return false;
	},	
	
	build : function(response) {
		this.addContainer();
			
		this.configured = true;
		this.dialogContainer.html(response.html);

		window.setTimeout(this.createDelegate(this.configure),1);
	},
	
	addContainer : function() {
		if (this.parentControl instanceof DbNetGrid || this.parentControl instanceof DbNetEdit || this.parentControl instanceof DbNetFile  || this.parentControl instanceof DbNetSpell)
			this.component = this.parentControl;		
		else
			this.component = this.parentControl.parentControl;
		if (this.component)
		{
			jQuery(document.createElement("div")).attr("id", this.id).attr("dependencyId", this.component.id).appendTo(this.component.container); 
			this.dialogContainer = jQuery("#" + this.id); 
		}
		else
			this.dialogContainer = jQuery(document.createElement("div")).appendTo("body");
		this.dialogContainer.hide();
		this.dialogContainer.addClass("dbnetsuite-" + this.dialogId + "-dialog");
	},

	configure : function() {
		this.dialog = this.dialogContainer.children();

		var child = this.dialogContainer.children(":first");
		
		if ( typeof child.attr("title") == "string" )
		{
			child.attr("title",child.attr("title")/*.replace(/ /g,"&nbsp;")*/);
			this.title = child.attr("title");
		}
		
		if (this.dialog.find(".message-line").length > 0)		
			this.messageLine = this.dialog.find(".message-line")[0];

		this.options.autoOpen = false;
		this.options.modal = this.modal;
		this.options.resizable = this.resizable;
		this.options.dragStop = this.createDelegate(this.savePosition);		
		this.options.resize = this.createDelegate(this.onResize);
		this.options.bgiframe = true;
	
		this.fireDialogEvent("onConfigure");

		this.dialog.dialog(this.options);
		this.dialog.bind("dialogopen", this.createDelegate(this.dialogOpened));
		this.dialog.bind("dialogclose", this.createDelegate(this.dialogClosed));

		if (this.component)
		{
			this.dialog.attr("dependencyId",this.component.id);
			this.dialog.parent().attr("dependencyId",this.component.id);
		}
		
		jQuery(window).scroll(this.createDelegate(this.scrollIntoView));   
		
	},
	
	scrollIntoView : function () {    
//		var offset = this.yOffset+jQuery(document).scrollTop()+"px";   
//		var e = jQuery(".ui-dialog-content").filter(".file-preview:last")[0].parentNode
//		jQuery(e).animate({top:offset},{duration:500,queue:false});   
	},	
		
	onResize : function(event) {
	},		
	
	dialogOpened : function() {
		if (this.width == null || this.resizable)
			this.width = this.dialogWidth();
		
		window.setTimeout(this.createDelegate(this.dialogOpenedCallback),1);
	},
	
	dialogOpenedCallback: function (event) {
	    this.dialog.parent().find('.ui-dialog-titlebar-close').attr('title', DbNetSuiteText['Close']);
		this.fireDialogEvent("onOpen");
		if (!this.position) 
			this.sizeDialog(true);
		else
			this.dialog.dialog("option", "position", this.position);
	},	
	
	centre : function() {
		this.dialog.dialog("option", "position", "center");
	},	
		
	size : function() {
		this.dialogOpened();
	},
	
	sizeDialogToContent : function(centre) {
		this.sizeDialog(centre);

//		var w = (jQuery.browser.msie) ? this.dialogWidth() : "auto";
		
//		this.dialog.dialog("option", "width", w );
//		if (centre)
//			this.centre();
	},
	
	dialogWidth : function() {
		return (this.dialog.children().width() + this.widthAdj);
	},	
	
	setTitle : function(title) {
		this.dialog.dialog("option","title",title/*.replace(/ /g,"&nbsp;")*/);
	},	
	
	assignLabel : function(title) {
		if (this.label != "")
			this.setTitle( this.title + " (" + this.label + ")" );
		else
			this.setTitle( this.title );	
	},	

	sizeDialog : function(centre) {
//		var w = (jQuery.browser.msie) ? this.dialogWidth() : "auto";
	
		if (!this.sizeSet)
			this.dialog.dialog("option", "width", "auto");
		
        if (jQuery.browser.msie)
            window.setTimeout(this.createDelegate(this.setTitleWidth),1);
		
		if (centre)
			this.centre(); 
	},
	
	setTitleWidth : function() {
        if (jQuery.browser.msie)
            jQuery(this.dialog).parent().find('.ui-dialog-titlebar').width(this.dialog.children().width()-24);
	},		
	
	resize : function(centre) {
		this.sizeDialogToContent(centre);
	},	
	
	savePosition : function(event) {
		var offset = this.dialog.offset();
		this.position = [offset.left,offset.top];
	},		
	
	close : function(event) {
		if (this.dialog)
			this.dialog.dialog("close");
	},
	
	dialogClosed : function() {
		this.fireDialogEvent("onClose");
	},
	
	fireDialogEvent : function(eventName) {
		var args = { dialog : this };
		this.fireEvent(eventName, args );
	},
	
	setPanelHeight : function(e, ratio) {
		e.height('').css("overflow-y","");
		
		var max = screen.availHeight * ((ratio) ? ratio : 0.6);
		if (e.height() > max)
			e.height(max).css("overflow-y","auto");
	},	
	
	adjustScrollPanel : function(className, dialogHeight) {
		var maxHeight = window.screen.availHeight * 0.5;
		var p = this.dialog.find("." + className);
		if (dialogHeight == "")
			if (p.height() > maxHeight)
				p.height(maxHeight).css("overflow-y","scroll");
		
		if (p.css("overflow-y") == "scroll"){
			p.css("padding-right",this.getScrollbarWidth() + "px");
			//this.width += this.getScrollbarWidth();
		}
	}			
});
