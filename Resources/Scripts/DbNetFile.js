var DbNetFile = DbNetSuite.extend({
	init: function(id){
		DbNetLink.components[id] = this;
		window.DbNetFileArray[id] = this;
		
		this._super(id);
		jQuery(this.container).addClass("dbnetfile");
		this.assignAjaxUrl("dbnetfile.ashx");
		//this.ajaxConfig.url = "dbnetfile.ashx";
		
		this.allowFolderDeletion = false;
		this.autoSelectFolder = false;
		this.browseMode = "FileSystem";

		/* column property names */
		
		this.columns = [];
		this.confirmDelete = true;
		this.createFolder = false;
		this.currentPage = 1;
		this.currentFolder = "";
		this.customMimeTypes = {};
		this.deleteRow = false;
		this.displayStyle = "Grid";
		
		this.fileFilter = "";
		this.folderFilter = "";
		this.fileInfoList = [];
		this.fileSelectionAction = "preview";
		this.folderPathLocation = "top";
		this.headerRow = true;
		this.height = "";
		this.indexingServiceCatalog = "system";
		this.maxSearchMatches = 100;
		this.messageLine = null;
		this.navigation = true;
		this.newFolder = false;
		this.newFolderDialog = new NewFolderDialog( this );
		this.orderBy = "Name";
		this.pageSize = 20;
		this.parentControl = null;
		this.rootFolder = "";
		this.rootFolderAlias = "";
		this.search = true;
		this.searchDialog = new FileSearchDialog( this );
		this.searchMode = "FileSystem";
		this.searchResultsDialog = new FileSearchResultsDialog( this );
		this.selectableFileTypes = "";
		this.selectionMode = "FoldersAndFiles";
		this.totalPages = 0;
		this.thumbnailHeight = 0;
		this.thumbnailPercent = 0;
		this.thumbnailWidth = 0;
		this.totalRows = 0;
		this.toolbarLocation = "top";
		this.toolbarButtonStyle = "image";
		this.upload = false;
		this.uploadFileTypes = "";
		this.uploadOverwrite = false;
		this.uploadMaxFileSizeKb = 1024;	
		this.visibleFileTypes = "";
		this.width = "";
		this.windowsSearchConnectionString = "Provider=Search.CollatorDSO;Extended Properties='Application=Windows';";
	},
	
	ajaxDataProperties: function(values){
		return this._super().concat([
			"allowFolderDeletion",
			"browseMode",
			"columns",
			"createFolder",
			"columns",
			"currentFolder",
			"currentPage",
			"customMimeTypes",
			"deleteRow",
			"displayStyle",
			"folderPathLocation",
            "fileFilter",
            "folderFilter",
			"headerRow",
			"indexingServiceCatalog",
			"maxSearchMatches",
			"navigation",
			"newFolder",
			"orderBy",
			"pageSize",
			"search",
			"selectionMode",
			"rootFolder",
			"rootFolderAlias",
			"selectableFileTypes",
			"thumbnailHeight",
			"thumbnailPercent",
			"thumbnailWidth",
			"toolbarButtonStyle",
			"upload",
			"uploadOverwrite",
			"uploadFileTypes",
			"uploadMaxFileSizeKb",
			"visibleFileTypes",
			"windowsSearchConnectionString"
			]);
	},
	
	setColumnTypes: function(values){
		if (arguments.length > 1)
			values = [values].concat(Array.prototype.slice.call(arguments, 1));
			
		this.setColumnsProperty(values,"columnType");
	},
	
	newColumn: function(){
		return {label : ""};
	},	  		
	
 	initialize: function(primaryKey){
 		if (!this.parentControl)
			if ( this.uninitializedControlId() != "" )
			{
				window.setTimeout(this.createDelegateCallback(this.initialize),100);
				return;
			} 	
 	
		this.currentPage = 1;
 		if (this.toolbarLocation.toLowerCase() == "top")
			this.addDOMElement("toolbarPanel",this.container).addClass("toolbar-top");
		this.addDOMElement("filePanel",this.container);
		if (this.toolbarLocation.toLowerCase() != "hidden")
			this.addDOMElement("toolbarPanel",this.container).addClass("toolbar-bottom");	
			
		if (this.messageLine == null)
		    this.messageLine = this.addDOMElement("messagePanel", this.container).show().html("&nbsp;").addClass("message-line alert alert-info");
 	
 		this.filePanel = this.$("filePanel");		
		
		if (this.displayStyle.toLowerCase() == "tree")
		{
			if (this.height != "")
				this.filePanel.height(this.height).css("overflow-y","auto");
			if (this.width != "")
				this.filePanel.width(this.width).css("overflow-y","auto");
		}
		
		this.fireEvent("onBeforeInitialized");
		this.showWaiting(this.filePanel);
		this.callServer("initialize", this.initializeCallback);
	},		
	
 	initializeCallback: function(response){
		this.filePanel.prop("disabled",false);
		this.toolbar = this.$("toolbarPanel");
		this.toolbar.prop("disabled",false);
		
		if (response.toolbar)
		{
			this.$("toolbarPanel").html(response.toolbar);
			this.$("toolbarPanel").show();
			this.assignToolbarHandlers();
			this.initialized = true;
			this.fireEvent("onInitialized");
		}
		
		this.loadDataCallback(response);
	},
	
	loadData : function(container)	{
		this.showWaiting(this.filePanel);
		this.callServer("load-data", {method:this.loadDataCallback, params:[container]});
	},
	
	loadDataCallback : function(response, container) {
		if (!container)
			container = this.filePanel;
			
		this.fileInfoList = this.deserializeRecord(response.fileInfoList);
		this.$(container).show().empty().html(response.html);	
		this.$("waitPanel").hide();

		this.filePanel.find(".path-segment").unbind().bind("click",this.createDelegate(this.pathSegmentSelected));
		this.filePanel.find(".header-cell").unbind().bind("click",this.createDelegate(this.headerCellClick));
		this.filePanel.find("#Icon_headerCell").unbind("click");
		this.filePanel.find("#Thumbnail_headerCell").unbind("click");

		this.filePanel.find(".data-row").unbind().bind("click",this.createDelegate(this.rowClick));
		this.filePanel.find(".data-row").bind("mouseover",this.createDelegate(this.rowMouseOver));
		this.filePanel.find(".data-row").bind("mouseout",this.createDelegate(this.rowMouseOut));
		
		this.filePanel.find(".folder-link").unbind().bind("click",this.createDelegate(this.folderSelected));
		this.filePanel.find(".file-link").unbind().bind("click",this.createDelegate(this.fileSelected));
		this.filePanel.find(".thumbnail").unbind().bind("click",this.createDelegate(this.fileSelected));

		this.filePanel.find(".tree-node-open").unbind().bind("click",this.createDelegate(this.openTreeNode));
		this.filePanel.find(".tree-last-node-open").unbind().bind("click",this.createDelegate(this.openTreeNode));

 		this.table = this.filePanel.find("table:first");

		if (this.displayStyle.toLowerCase() == "grid") {
			if (this.height != "" || this.width != "") {
				var h = this.height;
				var w = this.width;
				if (h == "")
					h = this.filePanel.height() + 20;
				if (w == "")
					w = this.filePanel.width() + 20;
				this.table.fixedHeader({width: parseInt(w), height:parseInt(h)});
			}
		}

		this.configureNavigation();

		if (this.autoSelectFolder)
			this.$(container).find(".data-row:first").find(".folder-link").click();
		else
			this.$(container).find(".data-row:first").click();
		
	//	this.fireEvent("onPageLoaded",this.$(container).children(":first"));
	
		var rows = this.$(container).find(".data-row")
		
		for (var r=0; r<rows.length; r++ )
		{
			var cells = jQuery(rows[r]).find(".data-cell");
			for (var c=0; c<cells.length; c++)
			{
				var args = {cell : cells[c], fileInfo : this.fileInfoList[r]}
				this.fireEvent("onCellTransform", args);
			}
		}
		
		this.fireEvent("onPageLoaded",this.$(container).children(":first"));
	},
	
	assignToolbarHandlers : function() {
		this.$("toolbar .navigation").bind("click",this.createDelegate(this.navigate));
		this.$("pageSelect").bind("keydown", this.createDelegate(this.pageSelectKeydown));
        this.$("searchBtn").bind("click",this.createDelegate(this.openSearchDialog));
        this.$("newFolderBtn").bind("click",this.createDelegate(this.openNewFolderDialog));
        this.$("uploadBtn").bind("click",this.createDelegate(this.doUpload));
        this.$("deleteRowBtn").bind("click",this.createDelegate(this.deleteFile));
	},
	
	headerCellClick : function(event) {
		var e = (event.target.className == "header-cell") ? event.target : this.$(event.target).parents(".header-cell").get(0);
		var cn = this.$(e).attr("columnName");
		if ( this.orderBy.toLowerCase() == (cn + " desc").toLowerCase() ) 
			this.orderBy = cn + " asc"
		else
			this.orderBy = cn + " desc"
		this.loadData();
	},

	rowClick : function(event) {
		var row = this.parentElement(event.target,"tr");
		this.$(row).parent().find(".data-row").removeClass("selected");
		
		if (this.displayStyle.toLowerCase() == "tree")
			if (this.$(row).parents(".tree-leaf-parent-row:first").length == 0)
				this.currentFolder = "";
			else
				this.currentFolder = this.$(row).parents(".tree-leaf-parent-row:first").prev(":first").attr("folder");
		this.$(row).addClass("selected");
		
		this.selectedFile = this.$(row).attr("file");
 		this.selectedFilePath = this.makePath( this.currentFolder, this.selectedFile );
 		
 		var idx = parseInt(this.$(row).attr("RowIdx"))-1;
		var fileInfo = this.fileInfoList[idx];

		this.fireEvent("onRowSelected", { row : row, fileInfo : {} });	
	},
	
	openTreeNode : function(event) {
		var cell = jQuery(event.target);
		var row = cell.parents("tr:first");

		if (cell.hasClass("tree-node-open") || cell.hasClass("tree-last-node-open") )
		{
			this.loadNode(row);
		}
		else
		{
			cell[0].className = cell[0].className.replace("-close","-open");
			row.next(":first").hide()
		}
	},	
	
	loadNode : function(row, reload) {
		var lastNode = false
		var cell = row.children(":first");
		var table = row.parents("table:first");

		if ( cell.hasClass("tree-last-node-open") )
			lastNode = true;
		
		cell[0].className = cell[0].className.replace("-open","-close")
		
		if (row.next(":first").find(".tree-vertical-line").length > 0)
		{
			if (!reload)
			{
				row.next(":first").show()
				return;
			}
			else
				row.next(":first").remove();			
		}
		
		var newRow = table[0].insertRow( parseInt(row.prop("rowIndex")) + 1);
		newRow.className = "tree-leaf-parent-row"
		var cell = newRow.insertCell(-1);
		
		if (!lastNode)
			cell.className = "tree-vertical-line";
		cell = newRow.insertCell(-1);
		cell.colSpan = 2;
		
		this.currentFolder = this.$(row).attr("folder");
		this.loadData(cell);
	},

	rowMouseOver : function(event) {
		this.$(event.target).parents("tr:first").addClass("highlight");
	},
	
	rowMouseOut : function(event) {
		this.$(event.target).parents("tr:first").removeClass("highlight");
	},	
	
	fileSelected : function(event) {
		var row = this.$(event.target).parents("tr:first");
		this.selectedFile = "/" + row.attr("file");
		this.selectFile();
	},
	
	selectFile : function() {
		this.selectedFilePath = this.makePath( this.currentFolder, this.selectedFile );
		
		var fileInfo = this.getFileInformation(this.selectedFile);
 		
 		var args = {cancel:false, message : "", fileInfo : fileInfo}; 

		this.fireEvent("onBeforeFileSelected", args );

		if (args.cancel)
		{
			if (args.message == "")
				args.message = "File cannot be accessed";
			this.showMessage(args.message);
			return;
		}	
			
		switch ( this.fileSelectionAction.toLowerCase() )
		{
			case "download":
				this.downloadDocument(this.selectedFilePath);
				break;
			case "display":
				this.displayDocument(this.selectedFilePath);
				break;
			case "preview":
				this.openFilePreviewDialog(this.selectedFilePath);
				break;
		}
	},	
	
	makePath : function(folder, file) {
		var path = file;
		if (folder != "")
			path = folder + "/" + file;			
		return path.replace(/\/\//g,"/");
	},

	openSearchDialog : function(event) {
		this.openDialog(this.searchDialog,"search-dialog");
	},
	
	openNewFolderDialog : function(event) {
		this.openDialog(this.newFolderDialog,"new-folder-dialog");
	},	
	
	runSearch : function(searchCriteria) {
		searchCriteria["currentFolder"] = this.currentFolder;
		var data = {searchCriteria : searchCriteria, orderBy : "Name"}
		this.callServer("run-search", this.runSearchCallback, data);
	},
	
	runSearchCallback : function(response) {
		this.searchResultsDialog.results = response.html;
		this.openDialog(this.searchResultsDialog,"search-results-dialog");
    },	
	
	downloadDocument : function(filePath) {
		window.location.href = this.streamUrl(filePath) + "&download=true&rootFolder=" + this.rootFolder;
	},
	
	folderSelected : function(event) {
		var row = this.$(event.target).parents("tr:first");
		if (this.displayStyle.toLowerCase() == "grid")
			if (this.currentFolder == "")
				this.currentFolder = row.attr("file");
			else
				this.currentFolder += "/" + row.attr("file");
		else
			this.currentFolder = "/" + row.attr("folder");
			
		if (this.displayStyle.toLowerCase() == "grid")
		{
			event.stopPropagation();
			this.selectFolder();
		}
		this.fireEvent("onFolderSelected", this.currentFolder);
	},
	
	selectFolder : function() {
		this.currentPage = 1;
		this.loadData();
	},	
	
	pathSegmentSelected : function(event) {
		var cellIndex = event.target.parentNode.cellIndex;

		var pathrow = this.$(event.target).parents(".path_segment_row").get(0);
		
		if (cellIndex == 0)
			cellIndex = pathrow.cells.length -2;
		
		this.currentPage = 1;
		this.currentFolder = "";
		
		for ( var i = 3; i <= cellIndex; i++ )
		{
			var c = this.$(pathrow.cells[i]);
			if ( c.text() == "/" )
				this.currentFolder += c.text();
			else
				this.currentFolder += c.attr("folder");
		}	
		
		this.loadData();
	},
	
	doUpload : function(event) {
		var data = {}
		data.currentFolder = this.currentFolder;
		this.openUploadDialog(data);
	},	
	
	assignUpload : function() {
		this.reloadFolder();
	},
	
	reloadFolder : function(reloadParent) {
		if (this.displayStyle.toLowerCase() == "grid")
		{
			this.loadData();
			return;
		}
		var row = this.filePanel.find(".selected")	
 		if (row.length == 0)
 			return
			
		if (row.attr("recordType") == "FileInfo" || reloadParent)
			if (row.parents(".tree-leaf-parent-row:first").length == 0)
				this.loadData();
			else		
				this.loadNode(row.parents(".tree-leaf-parent-row:first").prev(":first"), true);
		else
			this.loadNode(row, true);
	},	
	
 	deleteFile : function() {
		var row = this.filePanel.find(".selected")	
 		if (row.length == 0)
 		{
 			this.showMessage(this.translate("NoFileSelected"));
 			return
 		}

		this.selectedFile = row.attr("file");
  			
 		var fileInfo = this.getFileInformation(this.selectedFile);

 		if (fileInfo.type == "DirectoryInfo")
 			if (!this.allowFolderDeletion)
 				if (fileInfo.files + fileInfo.directories > 0)
 				{
					this.showMessage(this.translate("FolderNotEmpty"));
					return;
				}

 		var args = {cancel:false, message : "", fileInfo : fileInfo}; 
		this.fireEvent("onBeforeFileDeleted", args);

		if( args.cancel )
		{
			if (args.message == "")
				args.message = "File cannot be deleted";
			this.showMessage(args.message);
			return;
		}	

		if (this.confirmDelete){
			var msg 
 			if ( row.attr("recordType") == "DirectoryInfo" )
				msg = this.translate("DeleteFolderConfirm");
 			else
				msg = this.translate("DeleteFileConfirm");
			this.messageBox("question", msg, this.createDelegate(this.deleteConfirmed));
		}
		else {
			this.deleteConfirmed("yes");
		}
				
			
	},
	
	deleteConfirmed : function(buttonPressed) {
		if (buttonPressed != "yes")
			return;
		
		var data = { selectedFile : this.selectedFile}
		this.callServer("delete-file", this.fileDeleted, data);
 	},

	getFileInformation : function(fileName) {
		var args = {selectedFile:fileName};
		var resp = this.callServer("file-info",null,args);
		return this.deserializeRecord(resp.fileInfo);
	},		
 	
 	fileDeleted : function(response) {
		if (response.message != "")
		{
			this.showMessage(response.message);
			return;
		}
	
		this.reloadFolder(true);
 	},
 	
 	addLinkedControl : function(ctrl) {
		ctrl.rootFolder = this.rootFolder;
		ctrl.rootFolderAlias = this.rootFolderAlias;
		ctrl.parentControl = this;
		this.childControls.push({ ctrl : ctrl });

		this.bind("onFolderSelected", ctrl.createDelegate(ctrl.parentControlFolderSelected));
		ctrl.initialize();
	},		
 	
 	parentControlFolderSelected : function(parentControl) {
		this.currentFolder = parentControl.currentFolder;
		this.loadData();
 	}	
});
