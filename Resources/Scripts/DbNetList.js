var DbNetList = DbNetSuite.extend({
	init: function(id){
		DbNetLink.components[id] = this;
		this._super(id);
		
		jQuery(this.container).addClass("dbnetlist");
		this.assignAjaxUrl("dbnetlist.ashx");
		//this.ajaxConfig.url = "dbnetlist.ashx";
		
		/* column property names */
		
		this.autoRowSelect = true;
		this.checkbox = false;
		this.columns = [];
		this.columnProperties = {};
		this.linkedControls = [];
		this.nestedChildList = false;
		this.nestedLevel = 0;
		this.width = "";
		this.height = "";
		this.headerRow = false;
		this.rowSelection = false;
		this.parentRow = null;
		this.parentList = null;
		this.treeImageUrl = "";
		this.sql = "";
		this.parameters = {};
		this.table = null;
	},
	
	ajaxDataProperties: function(values){
		return this._super().concat([
			"sql",
			"parameters",
			"headerRow",
			"columns",
			"columnProperties",
			"checkbox",
			"nestedChildList",
			"nestedLevel",
			"treeImageUrl"
			]);
	},
	
	initialize: function(){
		this.initialized = true;		
		this.fireEvent("onInitialzed");
		
		this.load();
	},
	
	load: function(){
		if (!this.initialized)
			this.initialize();

		this.showWaiting(this.container);

		this.fireEvent("onBeforeItemsLoaded");
		this.callServer("load", this.loadCallback);	
	},
	
	loadCallback: function(response){
		this.$("waitPanel").hide();
		
		this.saveColumnWidths();
		
		this.container.empty().html(response.html);
		
		this.table = this.container.find("table:first");
		
		if (jQuery.browser.msie)
			this.fixColumnWidths();
		
		if (this.height != "" && this.width != "")
			if (this.table.find(".header-row").length > 0)
				this.table.fixedHeader({width: parseInt(this.width), height:parseInt(this.height)})
			else
				this.container.css("width",this.width).css("height",this.height).css("overflow","auto");

		this.dataRows = this.table.find(".data-row");	
		this.header = this.table.find(".header-row");	
		this.dataRows.unbind();
		this.dataRows
			.bind("click",this.createDelegate(this.rowClick))
			.bind("mouseover",this.createDelegate(this.rowMouseOver))
			.bind("mouseout",this.createDelegate(this.rowMouseOut));
			
		if (this.rowSelection)
			this.dataRows.css("cursor","pointer");			
		
		this.dataRows.find(".data-cell-text")
			.bind("mouseover",this.createDelegate(this.mouseoverText))
			.bind("mouseout",this.createDelegate(this.mouseoutText));
			
		this.dataRows.find(".expand-button").bind("click",this.createDelegate(this._openNestedList));	
		this.dataRows.find(".collapse-button").bind("click",this.createDelegate(this.closeNestedList));	
		this.dataRows.find(".selectable-link").bind("click",this.createDelegate(this.linkSelected));	

		this.dataRows.find(".tree-node-open").unbind().bind("click",this.createDelegate(this.openTreeNode));
		this.dataRows.find(".tree-last-node-open").unbind().bind("click",this.createDelegate(this.openTreeNode));

		this.container.find(".select-checkbox").bind("click",this.createDelegate(this.checkBoxClick));
			
		for (var r=0; r< this.dataRows.length; r++)
		{
			var row = this.dataRows[r];
			this.fireEvent("onRowTransform", {row : row});
		}
		
		if ( this.autoRowSelect )						
			if (this.dataRows.length > 0)
				this.rowClick( {target : this.dataRows[0]} );
		
		this.fireEvent("onItemsLoaded");
	},
	
	openTreeNode : function(event) {
		var cell = jQuery(event.target);
		var row = cell.parents("tr:first");

		if (cell.hasClass("tree-node-close") || cell.hasClass("tree-last-node-close") )
		{
			cell[0].className = cell[0].className.replace("-close","-open");
			this.closeNestedList(event);
		}
		else
		{
			this.openNestedList(event);
		}
	},	
		
	addNestedList : function(handler) {
		this.bind("onNestedClick", handler);
		this.nestedChildList = true;
	},	
	
	selectedValue : function(columnName) {
		return this.columnValue(this.selectedRow(),columnName);
	},	
	
	selectedRow : function() {
		var row = this.container.find(".selected");

		if (row.length > 0)
			if (row[0].tagName.toLowerCase() == "tr")
				return row;
			else
				return row.parents("tr:first");
		else
			return null;		
	},	
	
	mouseoverText : function(event) {
		jQuery(event.target).addClass("highlight");
	},				
	
	mouseoutText : function(event) {
		jQuery(event.target).removeClass("highlight");
	},	
		
	columnValue : function(row, columnName) {
		if (typeof row == "string") {
			columnName = row;
			row = this.selectedRow();
		}
		if (row == null)
			return "";
			
		var e = jQuery(row).attr(columnName);
		if (e != undefined)
			return e;
		e = jQuery(row).find("td[columnname='" + columnName + "']");
		if (e.length > 0)
			if (e.children().length > 0)
				return e.children().text();
			else
				return e.text();	
				
		return null;		
	},	
	
	getCell : function(row, columnName) {
		var c = jQuery(row).find("td[columnname='" + columnName + "']");
		if (c.length > 0)
			return c[0];
					
		return null;		
	},			
	
	openNestedList : function(event) {
		var row = this.$(event.target).parents("tr:first");
		
		var cell = row.children(":first");
		var table = row.parents("table:first");

		var lastNode = false
		if ( cell.hasClass("tree-last-node-open") )
			lastNode = true;
		
				
		var newRow = this.table[0].insertRow(row[0].rowIndex+1);
		jQuery(newRow).hide();
		
		var cell = jQuery(document.createElement("td"))
		cell.html("&nbsp;")
		if (!lastNode)
			cell.addClass("tree-vertical-line");		
		cell.appendTo(jQuery(newRow)); 

		var cell = jQuery(document.createElement("td"))
		cell.addClass("nested-list-cell").attr("colSpan", row[0].cells.length-1 );
		cell.appendTo(jQuery(newRow)); 
		
		row.find(".expand-button").hide();
		row.find(".collapse-button").show();
		
		var handlers = this.eventHandlers["onNestedClick"]
		for (var i = 0; i < handlers.length; i++) {
			if (handlers[i]) {
				this.configureNestedList(handlers[i],row,cell)
			}
		}
	},

	configureNestedList : function(handler,row,cell) {
		var listId = "dbnetlist" + new Date().valueOf().toString();
		jQuery(document.createElement("div")).attr("id", listId).appendTo(jQuery(cell)); 
		var list = new DbNetList(listId);
		list.connectionString = this.connectionString;
		list.parentRow = row[0];
		list.parentList = this;
		list.nestedLevel = this.nestedLevel+1;
		list.headerRow = false;
		list.rowSelection = false;
				
		var args = [list, this]; 
		handler.apply(window,args);   
		
		for (var p in list.parameters )
		{
			var paramValue = list.columnValue(list.parentRow,p);
			if ( paramValue != null )
				list.parameters[p] = paramValue;
		}	
		list.bind("onItemsLoaded", this.createDelegate(this.nestedListLoaded));
		list.initialize();
	},
	
	nestedListLoaded : function(list) {	
		var cell = list.parentRow.cells[0];
		var classModifier = "";
		if (list.dataRows.length == 0) {
			jQuery(list.parentRow).next().remove();	
		}
		else {
			jQuery(list.parentRow).next().show();	
			classModifier = "-close"
		}

		cell.className = cell.className.replace("-open",classModifier);
	},
	
	closeNestedList : function(event) {
		var row = this.$(event.target).parents("tr:first");
		this.table[0].deleteRow(row[0].rowIndex+1);
		row.find(".expand-button").show();
		row.find(".collapse-button").hide();
	},		
	
	rowClick : function(event) {
		if (!this.rowSelection)
			return;

		var row = this.parentElement(event.target,"tr");
		
		var args = {cancel:false, row:row}; 
		this.fireEvent("onBeforeRowSelected", args);	
		if (args.cancel)
			return;

		var topList = this;
		while( topList.parentList )
			topList = topList.parentList;
			
		this.removeSelectedClass(topList.container.find(".data-row"));
		this.dataRows.find(".select-checkbox").prop("checked", false);

		this.addSelectedClass(jQuery(row));
		this.selectRow(row);
	},
	
	hasSelectedClass : function(row) {
		if (row.find("span.data-cell-text").length > 0)
			return (row.find("span.data-cell-text").hasClass("selected"));
		else
			return (row.hasClass("selected"));
	},		
		
	addSelectedClass : function(container) {
		if (container.find("span.data-cell-text").length > 0)
			container.find("span.data-cell-text").addClass("selected");
		else
			container.addClass("selected");
	
	},		
	
	removeSelectedClass : function(container) {
		if (container.find("span.data-cell-text").length > 0)
			container.find("span.data-cell-text").removeClass("selected");
		else
			container.removeClass("selected");
	},	
	
	selectRow : function(row) {
		if (!this.rowSelection)
			return;
	
		var row = this.parentElement(row,"tr");
		this.addSelectedClass(this.$(row));

		this.$(row).find(".select-checkbox").prop("checked", true);

		this.fireEvent("onRowSelected", { row : row });	
	},
	
	selectRowByIndex : function(idx) {
		if (!this.rowSelection || !this.dataRows)
			return;
			
		if(this.dataRows.length == 0 || idx > this.dataRows.length)
			return; 
			
		var row = this.dataRows[idx-1];

		if (this.hasSelectedClass(this.$(row)))
			return;	
			
		this.removeSelectedClass(this.dataRows);	
		row.scrollIntoView(false);
		this.addSelectedClass(this.$(row));
	},		
	
	unselectRow : function(row) {
		if (!this.rowSelection)
			return;
	
		var row = this.parentElement(row,"tr");
		this.removeSelectedClass(this.$(row));
		this.$(row).find(".select-checkbox").prop("checked", false);
	},	
	
	rowMouseOver : function(event) {
		this.$(event.target).parents(".data-row").addClass("highlight");
	},
	
	rowMouseOut : function(event) {
		this.$(event.target).parents(".data-row").removeClass("highlight");
	},	
	
	checkBoxClick : function(event) {
		var row = this.parentElement(event.target,"tr");
		
		if ( jQuery(row).hasClass("header-row") )
		{
			var o = this;
			
			var checkboxes = this.dataRows.find(".select-checkbox");
			checkboxes.prop("checked", event.target.checked);
			
			var f;
			
			if ( event.target.checked )
				f = function(){o.selectRow(this)};
			else
				f = function(){o.unselectRow(this)};
				
			checkboxes.each(f);
		}
		else
		{
			event.stopPropagation();

			if(event.target.checked)
				this.selectRow(event.target);
			else
				this.unselectRow(event.target);
		}
	},	
	
	addLinkedControl: function(combo){
		this.linkedControls.push(combo);
	},		
	
	linkSelected: function(event){
		var args = {event : event}
		args.row = jQuery(event.target).parents("tr:first")[0];
		this.fireEvent("onLinkSelected", args);
	},
	
	saveColumnWidths: function(){
		var list = this;
		
		while (list.parentList != null)
		{
			list.widths = [];
			for (var i=0; i<list.parentRow.cells.length; i++)
				list.widths.push(jQuery(list.parentRow.cells[i]).width())
				
			list = list.parentList
		}
	},
	
	
	fixColumnWidths: function(){
		var list = this;

		while (list.parentList != null)
		{
			var adj = 0
			for (var i=0; i<list.parentRow.cells.length; i++)
			{	
				var c = jQuery(list.parentRow.cells[i]);
				var adj = c.width() - list.widths[i]
				list.widths[list.widths.length-1] += adj;
				c.width(list.widths[i]);
			}
			list = list.parentList
		}	
	}	
	
});	