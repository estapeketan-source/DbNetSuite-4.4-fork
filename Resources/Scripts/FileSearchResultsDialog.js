var FileSearchResultsDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"file_search_results");
		this.orderBy = "Name";
		this.results = null;
	},
	
	configure : function() {
		this._super();
		
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".help-button").bind("click", this.createDelegate(this.showHelp));
		this.bind("onOpen", this.createDelegate(this.sd));

		this.open();
	},
	
	sd : function() {
		var p = this.dialog.find(".results-panel");
		var maxHeight = (screen.availHeight / 2);
		var maxWidth = (screen.availWidth -100);
		if ( this.$(p.children().get(0)).height() > maxHeight )
		{
			p.css("height",maxHeight);
			p.css("overflowY","auto");
		}
		else
		{
			p.css("height","");
			p.css("overflowY","visible");
		}
		if ( this.$(p.children().get(0)).width() > maxWidth )
		{
			p.css("width",maxWidth);
			p.css("overflowX","auto");
		}
		else
		{
			p.css("width","");
			p.css("overflowX","visible");
		}
			
		window.setTimeout(this.createDelegate(this.sizeDialogToContent),1);
	},
	
	open : function() {
		var p = this.dialog.find(".results-panel");
		p.html( this.results );
		
		this.dialog.find(".header-cell").bind("click",this.createDelegate(this.headerCellClick));
		this.dialog.find("#Icon_headerCell").unbind("click");
		this.dialog.find("#Thumbnail_headerCell").unbind("click");

		this.dialog.find(".data-row").bind("click",this.createDelegate(this.rowClick));
		this.dialog.find(".data-row").bind("mouseover",this.createDelegate(this.rowMouseOver));
		this.dialog.find(".data-row").bind("mouseout",this.createDelegate(this.rowMouseOut));
		
		this.dialog.find(".folder-link").bind("click",this.createDelegate(this.folderSelected));
		this.dialog.find(".parent-folder-link").bind("click",this.createDelegate(this.folderSelected));

		this.dialog.find(".file-link").bind("click",this.createDelegate(this.fileSelected));
		this.dialog.find(".thumbnail").unbind().bind("click",this.createDelegate(this.fileSelected));
		
		this.dialog.find(".data-row:first").trigger("click");
		
		this._super();
	},
	
	headerCellClick : function(event) {
		var e = (event.target.className == "header-cell") ? event.target : this.$(event.target).parents(".header-cell").get(0);
		this.orderBy = this.$(e).attr("columnName") + " " + (( this.orderBy == (this.$(e).attr("columnName") + " asc") ) ? "desc" : "asc");
		this.runSearch();
	},

	rowClick : function(event) {
		this.parentControl.rowClick(event);
	},

	rowMouseOver : function(event) {
		this.parentControl.rowMouseOver(event);
	},
	
	rowMouseOut : function(event) {
		this.parentControl.rowMouseOut(event);
	},

	fileSelected : function(event) {
		var row = this.$(event.target).parents("tr").get(0);	
		var selectedFile = this.$(row).attr("file");
		var folder = this.$(row).attr("folder");
		this.parentControl.openFilePreviewDialog( folder + "/" + selectedFile);
	},

	folderSelected : function(event) {
		var elem = this.$(event.target);
		if ( elem.is("." + this.componentId + "_folder-link") )
		{
			var row = elem.parents("tr").get(0);	
			var folder = this.$(row).find("." + this.componentId + "_parent-folder-link").get(0);

			this.parentControl.currentFolder = this.$(folder).text() + "/" + elem.text();
			this.parentControl.selectFolder();
		}	
		else
		{
			this.parentControl.currentFolder = elem.text();
		}
		this.parentControl.selectFolder();
	}
	
});
