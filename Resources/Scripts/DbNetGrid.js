var DbNetGrid = GridEditControl.extend({
	init: function(id){
		this._super(id);
		window.DbNetGridArray[id] = this;
		DbNetLink.components[id] = this;
		this.autoRowSelect = true;
		this.caption = "";
		this.chart = false;
		this.chartSerialize = false;
		this.chartConfig = {"autoLoad":false,"chartPanel":"","series":{},"legend":{},"legends":[],"title":{},"titles":[],"area3DStyle":{},"borderSkin":{},"chartArea":{}};
		this.chartConfigDialog = new ChartConfigDialog( this );
		this.chartDialog = new ChartDialog( this );
		this.chartPanel = null;
		this.collapseNestedChildren = false;
		this.columnFilterSql = [];
		this.columnFilterParams = {};
		this.columnPicker = false;
		this.columnPickerDialog = new ColumnPickerDialog( this );
		this.config = false;
		this.configDialog = new ConfigDialog( this );
		this.copy = true;
		this.dragAndDrop = true;
		this.dropTarget = null;
		this.editDialog = new EditDialog( this );
		this.editControl = null;
		this.editDialogHeight = "";
		this.editLayoutColumns = 1;
		this.exportFileName = "";
        this.exportFolder = "";
		this.frozenColumns = 0;
		this.frozenColumnsRows = [];
        this.slidingColumnsRows = [];
		this.inlineEditToolbarLocation = "bottom";
		this.inlineEditToolbarButtonLocation = "right";
		this.outputHeaderRow = null;
		this.cellIndexCache = {};
		this.columnIndexCache = {};
		this.customSave = false;
		this.fixedOrderBy = "";
		this.filterColumnMode = "Simple";
		this.groupBy = false;
		this.groupByHiddenColumns = false;
		this.having = null;
		this.headerRow = null;
		this.height = "";
		this.mailMerge = false;
		this.mailMergeDocuments = {};
		this.multiRowSelect = false;
		this.multiRowSelectLocation = "right";
		this.nestedGrid = false;
		this.nestedGridConfig = {};
		this.noSort = false;
		this.outputPageSelect = false;
		this.outputCurrentPage = false;
		this.outputPageSize = 0;
		this.optimizeForLargeDataSet = false;
		this.optimizeExportForLargeDataSet = false;
		this.pageSize = 20;
		this.pageUpdate = false;
		this.parentGrid = null;
		this.pdfSettingsDialog = new PdfSettingsDialog( this );
		this.print = true;
		this.procedureName = "";
		this.procedureParameters = {};
		this.rowIndex = null;
		this.save = true;
		this.saveOptions = "HTML,Word,Excel,XML,CSV,PDF"; 				
		this.saveType = "";
		this.selectModifier = "";		
		this.table = null;		
		this.updateMode = "Row";
		this.view = false;
		this.viewPrint = true;
		this.viewDialog = new ViewDialog( this );
		this.viewDialogHeight = "";
		this.viewDialogWidth = "";
		this.viewLayoutColumns = 1;
		this.viewTemplatePath = "";
		this.width = "";

		/* column property names */
		/* legacy properties */
	
		jQuery(this.container).addClass("dbnetgrid");
		this.assignAjaxUrl("dbnetgrid.ashx");
		//this.ajaxConfig.url = "dbnetgrid.ashx";
	},
	
	ajaxDataProperties: function(values){
		return this._super().concat([
			"caption",
			"chart",
			"columnFilterSql",
			"columnFilterParams",
			"columnPicker",
			"copy",
			"config",
			"editDialogHeight",
            "exportFileName",
            "exportFolder",
			"filterColumnMode",
			"fixedOrderBy",
            "frozenColumns",
			"groupBy",
			"groupByHiddenColumns",
			"having",
            "inlineEditToolbarLocation",
            "inlineEditToolbarButtonLocation",
			"mailMerge",
			"mailMergeDocuments",
			"multiRowSelect",
			"multiRowSelectLocation",
			"nestedGrid",
			"noSort",
			"outputCurrentPage",
			"outputPageSelect",
			"outputPageSize",
			"optimizeForLargeDataSet",
			"optimizeExportForLargeDataSet",
			"pageSize",
			"pageUpdate",
			"print",
			"procedureName",
			"procedureParameters",
			"save",
			"saveOptions",
			"saveType",
			"selectModifier",
			"updateMode",
			"view",
			"viewDialogHeight",
			"viewDialogWidth",
			"viewLayoutColumns",
			"viewPrint"
			]);
	},
				
	profileProperties: function(values){
		return this._super().concat(["columns","pageSize","outputPageSize", "outputCurrentPage","chartConfig"]);
	},			
 
 	initialize: function(parentControl){
 		this._super();
		
 		if (parentControl)
			this.bindToParent(parentControl);
		else	
		{
			/* 4.4
			if ( this.uninitializedControlId() != "" )
			{
				window.setTimeout(this.createDelegateCallback(this.initialize),100);
				return;
			}
			*/
		}

		if ( typeof(this.mailMergeDocuments) == "string")
              this.mailMergeDocuments = { " ": this.mailMergeDocuments };

        if (this.frozenColumns > 0) {
            if (this.multiRowSelect) {
                this.frozenColumns++;
                this.multiRowSelectLocation = 'Left';
            }
        }

		this.loadData();
	},
	
	reconfigureColumns: function(cols){
		this.columns = cols;
		this.orderBy = "";
		this.loadData();
	},		
	
	loadData: function (args) {
        if (this.search === null) {
            this.search = (this.procedureName === "");
        }
	
		if ( this.$("gridPanel").length == 0)
		{
			this.fireEvent("onBeforeInitialized");

			if (this.convertLegacyCode)
				this.convertLegacyCode();
			this.currentPage = -1;
		}	
		
		this.beforeGetRecordSet();
		
		if (this.$("gridPanel").length == 0)
		{
			if (this.toolbarLocation.toLowerCase() == "top")
				this.addDOMElement("toolbarPanel",this.container);
			this.addDOMElement("gridPanel",this.container);
			if (this.toolbarLocation.toLowerCase() != "hidden")
				this.addDOMElement("toolbarPanel",this.container);
		}
		
		this.gridPanel = this.$("gridPanel");
		
		this.showWaiting(this.gridPanel);
		
		this.$("gridPanel").prop("disabled",true);
		this.$("toolbarPanel").prop("disabled",true);
		
		this.fireEvent("onBeforePageLoaded");

		args =  args || {};
		
		this.callServer("load-data", { method: this.loadDataCallback, params: [args] }, args);
	},
	
	getRecordSet: function(){
		this.currentPage = 0;
		this.loadData();
	},
	
	loadDataCallback: function (response, args) {
	    if (response.errorMessage) {
	        this.fireEvent("onPageLoadError", response);
            this.messageBox("error", response.errorMessage);
	        this.$("waitPanel").hide();
	        return;
	    }

	    this.adjustDatePrimaryKeyTimeZone();

		var initialLoad = false;
		if (response.toolbar)
		{
			this.$("toolbarPanel").show().html(response.toolbar);
			this.$("toolbarPanel").droppable().bind("drop",this.createDelegate(this.dragDropped));

			jQuery("body").append(response["dragIcon"]);
			this.assignToolbarHandlers();
			initialLoad = true;
		}
		
		if (response.searchPanel)
			this.configureSearchPanel(response);	
			
		this.$("waitPanel").hide();
		this.gridPanel.prop("disabled",false);
		this.toolbar = this.$("toolbarPanel");
		this.toolbar.prop("disabled",false);
			
		this.gridPanel.empty().html(response.grid);

		this.table = this.gridPanel.find("table:first");
		
		this.gridPanel.find(".header-cell").bind("click",this.createDelegate(this.headerCellClick));
		this.bindRowEvents();
		
		if ( this.dataRows.length == 0 )
		{
			if ( this.currentPage > 1 )
			{
				this.currentPage--;
				this.loadData();
				return;
			}
			if (this.editDialog.isOpen())
				this.editDialog.close();
		}			
		
		this.headerRow = this.gridPanel.find(".header-row");	
		
		if (this.dragAndDrop)
		{
			this.headerRow.find(".header-cell-table")
				.draggable({helper:"clone"})
				.bind("dragstart",this.createDelegate(this.columnDragStarted))
				.bind("dragstop",this.createDelegate(this.columnDragStopped))
				.bind("drag",this.createDelegate(this.columnDrag));
		
			this.headerRow.find(".header-cell")
				.droppable()
				.bind("dropover",this.createDelegate(this.dragDropOver))
				.bind("dropout",this.createDelegate(this.dragDropOut))
				.bind("drop",this.createDelegate(this.dragDropped));
		}
		
		this.gridPanel.find(".drop-down").bind("change",this.createDelegate(this.filterColumnChange));	
		this.gridPanel.find(".input").bind("keyup",this.createDelegate(this.filterColumnInput));	
		this.gridPanel.find("#selectAllCheckBox").bind("click",this.createDelegate(this.selectAllRows));
		this.gridPanel.find(".multi-select-checkbox").bind("click",this.createDelegate(this.multiRowClick));
			
		this.configureToolbar();
		this.configureInlineEdit();	
		
		this.table = this.gridPanel.find("#" + this.id + "_datatable");
		this.transformGrid(this.table, "display");
		
		if (this.frozenColumns > 0) {
		    this.freezeColumns();
		}
		else {
		    if (this.height != "" || this.width != "") {
		        var h = this.height;
		        var w = this.width;
		        if (h == "")
		            h = this.gridPanel.height() + 20;
		        if (w == "")
		            w = this.gridPanel.width() + 20;

		        this.table.fixedHeader({ width: parseInt(w), height: parseInt(h) });
		    }
		}

        this.frozenColumnsRows = this.gridPanel.find(".fixed-columns").find(".data-row");
        this.slidingColumnsRows = this.gridPanel.find(".sliding-columns").find(".data-row");

        if (this.frozenColumns > 0) {
            if (this.multiRowSelect) {
                var $fixedColumns = this.gridPanel.find(".fixed-columns");
                var $slidingColumns = this.gridPanel.find(".sliding-columns");
                var $frozenColumnsRows = $fixedColumns.find("tr.data-row");
                var $slidingColumnsRows = $slidingColumns.find("tr.data-row");

                $slidingColumns.find("tr.header-row").css("height", $fixedColumns.find("tr.header-row").css("height"));

                for (var i = 0; i < $slidingColumnsRows.length; i++) {
                    var height = $($frozenColumnsRows[i]).css("height");
                    $($slidingColumnsRows[i]).css("height", height);
                }
            }
        }
		
		this.pageUpdate = false;
		if (response.toolbar)
		{
			this.initialized = true;
			this.fireEvent("onInitialized");
		}
		
		this.fireEvent("onPageLoaded", { table: this.table[0], initialLoad: initialLoad });
        		
		this.notifyChildControlsParentPageLoaded();

		if (this.chartConfig.autoLoad)
			this.buildChart();
		
		this.positionOnRow();

		if (this.userProfile) {
		    if (this.profileOnLoad == null) {
		        this.profileOnLoad = this.getProfileProperties();
		    }
		    if (response.defaultUserProfileId) {
		        this.selectDefaultUserProfile(response.defaultUserProfileId);
		    }
		}

		if (args.batchUpdated) {
		    this.fireEvent("onGridAutoSaveCompleted");
		}
	},

	freezeColumns : function() {
	    this.table.parent().css("white-space", "nowrap");
        this.table.wrap("<div class='sliding-columns' style='width:" + this.width + ";overflow-x:auto;vertical-align:top;'/>");
        this.table.parent().parent().find(".sliding-columns").css("display", "inline-block").before("<div class='fixed-columns' style='display:inline-block;vertical-align:top;'><table class='" + this.table.attr("class") + "'></table></div>");
        
        this.createFrozenColumn("header");
        this.createFrozenColumn("filter");
        this.createFrozenColumn("data");
        this.createFrozenColumn("aggregate");
        this.createFrozenColumn("inline-edit-toolbar");

        this.gridPanel.find(".fixed-columns").find(".data-row")
            .bind("dblclick", this.createDelegate(this.rowDoubleClick))
            .bind("click", this.createDelegate(this.frozenColumnEventPassThru))
            .bind("mouseover", this.createDelegate(this.frozenColumnEventPassThru))
            .bind("mouseout", this.createDelegate(this.frozenColumnEventPassThru));
    },

	createFrozenColumn: function (cl) {
	    var fc = this.frozenColumns;
	    this.table.find("tr." + cl + "-row").each(function (index) {
	        var newRow = jQuery("<tr/>");
	        var tag = (cl == "header" || cl == "filter") ? "th" : "td"
	        newRow.addClass(jQuery(this).attr("class"));
	        var h = jQuery(this).height();
	        if (!jQuery.support.boxmodel)
	            h--;
            newRow.height(h);
	        if (cl == "data")
	            newRow.attr("dataRowIndex", jQuery(this).attr("dataRowIndex"));
	        jQuery(this).find(tag + ":lt(" + fc.toString() + ")").each(function () {
	            var cs = jQuery(this).prop("colSpan");
	            if (cs == 1) {
	                newRow.append(jQuery(this).clone(true));
	                jQuery(this).hide();
	            }
	            else {
	                newRow.append("<td>&nbsp;</td>");
	                jQuery(this).prop("colSpan", cs-fc)
	            }
	        })

	        jQuery(this).parents(".sliding-columns:first").prev().find(".dbnetgrid").append(newRow);
	    });
	},

	frozenColumnEventPassThru: function (event) {
        var row = this.getDataRow(event.target);
	    this.dataRows.eq(parseInt(jQuery(row).attr("dataRowIndex"))).trigger(event.type);
	},

	columnDragStarted : function(event,ui) {
		var p = jQuery(event.target).parent();
		p.css("opacity",0.5);
		jQuery(ui.helper).attr("dbnetgrid_id",this.componentId);
		jQuery(ui.helper).width(p.width()+2).height(p.height()+2);
	},
	
	columnDragStopped : function(event,ui,context) {
		this.headerRow.find("tr").css("opacity",1.0);
	},
		
	columnDrag : function(event,ui) {
		if (!this.dropTarget || jQuery(ui.helper).attr("dbnetgrid_id") != this.componentId )
			return;
		
		var width = this.dropTarget.width();    
		var pos = this.dropTarget.offset();    
		
		var left = (pos.left-8);
		var top = (pos.top-16);
		 
		if ((ui.position.left + (jQuery(ui.helper).width()/2)) > (left + (width/2)))
			left += width;	
		
		this.dropTarget.attr("dropside", (left > (pos.left-8)) ? "right" : "left" );
		this.$("dragInsertionImg:visible").css({ "left": left + "px", "top":top + "px"});		
	},	

	dragDropOver : function(event,ui, context) {
		if (jQuery(ui.helper).attr("dbnetgrid_id") != this.componentId )
			return;
			
		var e = jQuery(context);

		if ( e.find(".ui-draggable-dragging").length > 0 )
			return;

		this.dropTarget = e;
		this.dropTarget.attr("dropside","left");
		this.$("dragInsertionImg").show();		
	},
	
	dragDropOut : function(event,ui) {
		if (!this.dropTarget)
			return;
	
		this.$("dragInsertionImg").hide();		
		this.dropTarget = null;
	},
	
	dragDropped : function(event,ui,context) {
		if (jQuery(ui.helper).attr("dbnetgrid_id") != this.componentId )
			return;

		var t = jQuery(context);
		var p = jQuery(ui.draggable).parent();
		p.css("opacity",1.0);
		var si = parseInt(p.attr("columnIndex"));

		if ( typeof t.attr("columnIndex") == "undefined")
		{
			if (this.columnPicker)
				if (this.headerRow.children().length > 1)
				{
					this.columns[si].display = false;
					this.loadData();
				}
			return;
		}

		var ti = parseInt(t.attr("columnIndex"));
	
		this.$("dragInsertionImg").hide();		

		if ( ti == si )
			return;
		
		var cols = [];
		
		for (var i=0; i<this.columns.length; i++)
		{
			if ( i == si )
				continue;

			if ( i == ti && t.attr("dropside") == "left")
				cols.push(this.columns[si]);

			cols.push(this.columns[i]);
			
			if ( i == ti && t.attr("dropside") == "right")
				cols.push(this.columns[si]);
		}
		
		this.reconfigureColumns(cols);
	},	
		
	transformGrid: function (table, context, dataRows) {
	    this.cellIndexCache = {};
	    this.columnIndexCache = {};

		var headerCells = table.find("th[columnIndex]");
		if (!dataRows)
			dataRows = table.find(".data-row");	

		var args = {};
		for (var c=0; c<headerCells.length; c++)
		{
			var idx = headerCells[c].getAttribute("columnIndex");
						
			var col = this.columns[idx];
	
			args.cell = headerCells[c];
			args.columnName = col.columnName;
			args.context = context;
				
			this.fireEvent("onHeaderCellTransform", args);
							
			for (var r=0; r<dataRows.length; r++)
			{
				var row = dataRows[r];
				args.cell = row.cells[headerCells[c].cellIndex];
				args.row = row;
				args.columnName = col.columnName;
				
				this.fireEvent("onCellTransform", args);
			}
		}
		
		for (var r=0; r<dataRows.length; r++)
			this.fireEvent("onRowTransform", {row : dataRows[r], context : context});
	},
	
	configureInlineEdit : function() {
		if (this.audit != "none")
			this.dataRows.find(".row-audit-info").find(".audit-history").click(this.createDelegate(this.auditHistory))
		
		this.inputControls = [];
		var ic = this.gridPanel.find(".dbnetgrid-input");
		if (ic.length == 0)
			return;
			
	//	this.$("insertRowBtn").prop("disabled",true);
			
		for (var r=0; r<this.dataRows.length; r++)			
			this.storeInputControls( jQuery(this.dataRows[r]) );

		this.messageLine = this.gridPanel.find(".dbnetgrid-message-box");

		this.gridPanel.find("[buttonType='spellcheck']").bind("click",this.createDelegate(this.checkSpelling));
        this.gridPanel.find(".apply-button").bind("click",this.createDelegate(this.applyClicked));
        this.gridPanel.find(".cancel-button").bind("click",this.createDelegate(this.cancel));
	},	
	
	storeInputControls : function(row) {
		var inputControls = {};
		var ic = row.find(".dbnetgrid-input");
		if (ic.length == 0)
			return;
		for (var i=0; i<ic.length; i++)	
		{		
			var e = jQuery(ic[i])
			var cn = e.attr("columnName");
			inputControls[cn] = e;
		}
		this.inputControls[ row.attr("dataRowIndex") ] = inputControls;
		for (var cn in inputControls)	
		{
			var col = this.columns[this.columnIndex(cn)];
			this.initialiseInput( inputControls[cn], col, row );
			this.bindSpellChecker(inputControls[cn]);
		}
	},	
	
	positionOnRow : function(editControl) {
		var r = 0;
		if ( this.rowIndex != null )
			r = this.rowIndex;
		this.rowIndex = null;
		
		if ( r >= this.dataRows.length )
			r = this.dataRows.length -1;		
		
		var row = this.$(".data-row").filter("tr[insertedRecord]");
		
		if (row.length > 0)
			row = row[0];
		
		if ( row.length == 0)
			row = this.dataRows[r];
		
		if (this.dataRows.length > 0 && this.autoRowSelect)
			this.rowClick( {target : row}, true );
	},
	
	bindRowEvents : function(editControl) {
        this.dataRows = this.gridPanel.find(".data-row");	
		this.dataRows.unbind();
		this.dataRows
			.bind("dblclick",this.createDelegate(this.rowDoubleClick))
			.bind("click",this.createDelegate(this.rowClick))
			.bind("mouseover",this.createDelegate(this.rowMouseOver))
			.bind("mouseout",this.createDelegate(this.rowMouseOut));
			
		this.gridPanel.find(".expand-button").unbind().bind("click",this.createDelegate(this.openNestedGrid));	
		this.gridPanel.find(".collapse-button").unbind().bind("click",this.createDelegate(this.closeNestedGrid));	
			
		this.dataRows.find(".thumbnail").click(this.createDelegate(this.openThumbnail));
	},
	
	refreshRow : function(row) {
		if (!row)
		{
			if (this.selectedRows().length > 0)
				row = jQuery(this.selectedRows()[0]);
			else if (this.currentRow)
				row = jQuery(this.currentRow)
			else
				return;
		}
			
		var data = {};
		data.primaryKey = this.rowPrimaryKey(row);
		data.rowIndex = row.attr("dataRowIndex")
		var delegate = this.createDelegate(this.refreshRowCallback);
		this.callServer("get-single-row", delegate ,data );
	},
	
	rowPrimaryKey : function(row) {
		if (!row)
			row = this.selectedRows()[0];
		return this.primaryKeyList[ jQuery(row).attr("dataRowIndex") ];
	},
		
	refreshRowCallback : function(response) {
		var div = document.createElement("div");
			
		var newRow = this.updateGridRow(div, response.html, false );
		this.bindRowEvents();
		this.storeInputControls(newRow);	
		newRow.addClass("selected");
		
		if ( response.totalsHtml )
			this.updateGridRow(div, response.totalsHtml, true );

		this.transformGrid(this.gridPanel.find("table:first"), "display", newRow);
		this.fireEvent("onRowRefreshed", { row: newRow });
		this.fireEvent("onGridAutoSaveCompleted");
	},
	
	updateGridRow : function(div, html, total) {
		jQuery(div).html("<table>" + html + "</table>");
		
		var newRow = jQuery(div).find("tr");
		var oldRow;
		var selector = "";

		if (!total)
		    selector = ".data-row[id=\"" + newRow.attr("id") + "\"]";
		else
		    selector = ".aggregate-row:last";

		if (this.frozenColumns > 0) {
		    var frozenRow = this.gridPanel.find(".fixed-columns").find(".data-row[datarowindex=" + newRow.attr("datarowindex") + "]");
		    newRow.find("td:lt(" + this.frozenColumns + ")").each(
                function (index) { frozenRow.children(index).html(jQuery(this).html()) }
            )
		    newRow.find("td:lt(" + this.frozenColumns + ")").remove();
		}
		
		oldRow = this.$("gridPanel:first").find(selector);
		
		if (oldRow.length > 0) {
			oldRow.html(newRow.html())

			newRow.each(function() {
			    jQuery.each(this.attributes, function (i, attrib) {
				 if (attrib.name.indexOf("hc_") > -1) {
					 oldRow.attr(attrib.name,attrib.value);
				 }
			  });
			});		
		}
		
		return this.$("gridPanel:first").find(selector);
	},	

	recordDeleted : function(sender) {
		this.rowIndex = parseInt(this.selectedRows()[0].getAttribute("dataRowIndex"));
		this.loadData();
	},	
	
	setOutputPage : function(event) {
		this.outputCurrentPage = event.target.checked;
	},		
	
	
	selectAllRows : function(event) {
		this.hasUnappliedChanges( {method:this.createDelegate(this.selectAllRowsCallback), params:[event]});
	},
	
	selectAllRowsCallback : function(buttonPressed, event) {
		if (buttonPressed != "yes")
		{
			event.target.checked = !event.target.checked;
			return;
		}

		if (this.editDialog.isOpen())
			this.editDialog.close();
		
		var o = this;
		
		this.dataRows.find(".multi-select-checkbox").prop("checked", event.target.checked );
		
		if ( event.target.checked )
			this.dataRows.find(".multi-select-checkbox").each(function(){o.selectRow(this)});
		else
			this.dataRows.find(".multi-select-checkbox").each(function(){o.unselectRow(this)});
	},	
	
	multiRowClick : function(event) {
		this.hasUnappliedChanges( {method:this.createDelegate(this.multiRowClickCallback),params:[event]});
		event.stopPropagation();
	},
	
	multiRowClickCallback : function(buttonPressed, event) {
		if (buttonPressed != "yes")
		{
			event.target.checked = !event.target.checked;
			return;
		}
		
		if(event.target.checked)
			this.selectRow(event.target);
		else
			this.unselectRow(event.target);
		
	},	
	
	filterColumnChange : function(e) {
		var filterDropDowns = this.gridPanel.find(".filter-column-select");
		
		if (!e)
			e = {"target" : filterDropDowns[0] }
		
		if (this.filterColumnMode == "Simple")
		{
			for(var index=0; index<filterDropDowns.length; index++)
			{
				if(filterDropDowns[index] != e.target)
					filterDropDowns[index].value = "";
			}
		}
		
		this.columnFilterSql = new Array();
		this.columnFilterParams = new Object();
		
		for(var index=0; index<filterDropDowns.length; index++)
		{
			if(filterDropDowns[index].value == "")
				continue;
				
			var headerRow = jQuery(filterDropDowns[index]).parents("tr.filter-row:first").prev();
			
			var ck = headerRow[0].cells[ filterDropDowns[index].parentNode.cellIndex ].getAttribute("columnKey");
				
			this.columnFilterSql.push("{col" + ck + "} {op} colfilterCol" + ck + "Param0");
			this.columnFilterParams[ck] = filterDropDowns[index].value;
		}
		
		var config = { "data" : {} };
		config.data = this.columnFilterParams;
		
		this.callServer("validate-filter-params", { method : this.filterColumnChangeCallback, params : [e]});		
	},
	
	filterColumnChangeCallback : function(response, event) {		
		if(response.result)
		{
			var args = { filters : this.$(".filter-row").find(".filter-column-select") };
			args.target = event.target;
			this.fireEvent("onFilterColumnChange",args);
			this.currentPage = 0;
			this.loadData();	
		}
		else
		{
			this.$(".filter-row").find("input[columnIndex=" + response.columnIndex + "]").addClass("field-highlight").focus();
			window.setTimeout(this.createDelegate(this.clearFilterHighlight),3000);
						
			this.showMessage(response.message);
		}
	},	
	
	getColumnFilter : function(cn)	{
		return this.$(".filter-row").find(".filter-column-select[columnName=" + cn.toLowerCase() + "]");
	},		
	
	setColumnFilter : function(cn,v)	{
		var e = this.getColumnFilter(cn);
		if (e.length == 0)
			return;
			
		e.val(v);
		this.filterColumnChange( {target : e[0] } );
	},			
	
	clearFilterHighlight : function()	{
		this.$(".filter-row").find("input").removeClass("field-highlight");
	},		
	
	filterColumnInput: function(e) {
		window.clearTimeout(this.quickSearchTimerId);
		
		switch(e.keyCode)
		{
			case 13:
			case 9:
				this.filterColumnChange(e);
				return;
		}
				
		var minChars = 0;

		switch(this.$(e.target).attr("dataType"))
		{
			case "String":
				minChars = this.quickSearchMinChars;
				break;
			case "DateTime":
				minChars = 8;
				break;
		}
			
		this.quickSearchTimerId = window.setTimeout( this.createDelegateCallback(this.filterColumnChange,e), this.quickSearchDelay);
	},		
	
	headerCellClick : function(event) {
		if (this.noSort)
			return;
			
		var hc = this.$(this.parentElement(event.target, "th"));

		if ( hc.attr("columnIndex") == null )
			return;
	
		if ( hc.attr("totalBreak") != null )
		    return;

		if (hc.attr("sortable") == "false")
		    return;
			
		var columnIndex = parseInt(hc.attr("columnIndex")) + 1;
		var sequence = ((this.orderBy.toLowerCase() == columnIndex + " asc") ? "desc" : "asc");
			
		this.orderBy = columnIndex + " " + sequence;
		this.fireEvent("onColumnSort", { columnName : hc.attr("columnName"), sequence : sequence });	
	
		this.loadData();
	},
	
	rowDoubleClick : function(event) {
		if ( this.gridPanel.find(".dbnetgrid-input").length > 0)
			return;
			
		var b = this.$("updateRowBtn:visible");
		
		if (b.length == 0)
			b = this.$("viewBtn:visible");
			
		if (b.length)
			b.click();
	},	
	
    rowClick: function (event, skipChangeCheck) {
		if (skipChangeCheck === true || !this.isEmptyObject(this.inputControls) )
			this.navigateToRow("yes",event);
		else
			this.hasUnappliedChanges( { method:this.createDelegate(this.navigateToRow), params:[event]});
	},
	
	rowModified : function() {
		for(var i=0; i < this.childControls.length; i++) {
			var c = this.childControls[i].ctrl;
			if (c instanceof DbNetEdit){
				if (c.container.is(":visible")) {
					if (c.modifiedInputControls())
						return true;
				}
			}
		}
	
//		if ( this.editDialog.isOpen() )
//			return this.editDialog.editControl.modifiedInputControls();
//		else
			return false;
	},	
		
	recordHasChanged : function() {
		if (this.rowModified())
			return true;
			
		if (this.skipUnappliedChangesCheck)
			return false;
			
		return this.modifiedInputControls();
	},	
	
	navigateToRow : function(buttonPressed,event) {
	    if (buttonPressed != "yes")
			return;

	    this.dataRows.removeClass("selected");
		this.slidingColumnsRows.removeClass("selected");
        this.frozenColumnsRows.removeClass("selected");

		this.dataRows.find(".multi-select-checkbox").prop("checked", false);
        this.frozenColumnsRows.find(".multi-select-checkbox").prop("checked", false);

		this.$("selectAllCheckBox:first").prop("checked", false);
		
		this.selectRow(event.target);
	},	
	
	selectedRows : function() {
		return this.$(".data-row:first").parent().children(".selected");
	},	
	
	selectRow : function(row) {
        row = this.getDataRow(row);

        this.$(row).addClass("selected");

        if (this.frozenColumns > 0) {
            this.getFrozenColumnDataRow(row).addClass("selected").find(".multi-select-checkbox").prop("checked", true);
            this.getSlidingColumnDataRow(row).addClass("selected");
        }

		this.currentRow = row;
		this.$(row).find(".multi-select-checkbox").prop("checked", true);
		
//		this.$(row).find(".thumbnail").each(this.createDelegate(this.refreshPreviewDialog));	
		this.notifyChildControlsParentRowSelected();
		this.fireEvent("onRowSelected", { row : row });	
	},
	
	unselectRow : function(row) {
        row = this.getDataRow(row);
        this.$(row).removeClass("selected");

        if (this.frozenColumns > 0) {
            this.getFrozenColumnDataRow(row).removeClass("selected").find(".multi-select-checkbox").prop("checked", false);
            this.getSlidingColumnDataRow(row).removeClass("selected");
        }

		this.$(row).find(".multi-select-checkbox").prop("checked", false);
	},	

	getDataRow : function(row) {
	    if ( !this.$(row).hasClass("data-row") )
	        row = this.$(row).parents(".data-row").get(0);
	    return row;
	},

    getFrozenColumnDataRow: function (row) {
        var dataRowIndex = parseInt(jQuery(row).attr("dataRowIndex"));
        return $(this.frozenColumnsRows.eq(dataRowIndex));
    },

    getSlidingColumnDataRow: function (row) {
        var dataRowIndex = parseInt(jQuery(row).attr("dataRowIndex"));
        return $(this.slidingColumnsRows.eq(dataRowIndex));
    },
		
	rowMouseOver : function(event) {
	    var row = jQuery(this.getDataRow(event.target));
	    row.addClass("highlight");
	    this.getFrozenColumnDataRow(row).addClass("highlight");
	},
	
	rowMouseOut: function (event) {
	    var row = jQuery(this.getDataRow(event.target));
	    row.removeClass("highlight");
	    this.getFrozenColumnDataRow(row).removeClass("highlight");
	},
	
	printCopyButtonClick : function(event) {
		var id = this.parentElement(event.target,"button").getAttribute("buttonType");
		this.generateOutput(id.replace('xcopy','copy'));
	},

	xCopyClick: function (event) {
	    clipboard.copy({
	        "text/html": this.gridPanel.find("table:first").html()
	    });
	    this.showMessage(this.translate("GridCopied"));
    },

    saveExport: function () {
        this.saveType = this.$("saveType").val();
        this.fireEvent("onBeforeOutput", {});
        this.callServer("save-grid", { method: this.saveExportCallback });
    },

    saveExportCallback: function (response) {
        this.showMessage(response.message);
    },

	generateOutput : function(action) {
		if (action == "save")
			this.saveType = this.$("saveType").val();
		else
			this.saveType = action;
		
		var data = this.fireOnBeforeOutput(action.toTitleCase());
		var method = this.outputCurrentPage ? "grid-css" : "build-html-export-grid";
		
		var callback;
		
		if (action == "datasource")
			callback = this.getMailMergeDataCallback;
		else
			callback = this.generateOutputCallback;
				
		this.callServer(method, { method : callback, params : [action]}, data);
	},
	
	
	getDataArray : function(columns) {
		var data = {}
		if (arguments.length > 1)
			columns = [columns].concat(Array.prototype.slice.call(arguments, 1));		
		if (typeof(columns) == "string")
			columns = columns.split(",");
		data.dataColumns = columns;
			
		var response = this.callServer("get-data-array", null, data);
		return response.data;
	},	
	
	fireOnBeforeOutput : function(mode){
		var args = { context : mode.toLowerCase(), outputTemplate : "", outputParameters : {} };
		this.fireEvent("onBeforeOutput", args);
		return args;
	},
			
	generateOutputCallback : function(resp, action) {
		if ( this.$("outputFrame").length == 0 )
			this.addDOMElement("outputFrame", null, "iframe");

		var iframe = this.$("outputFrame")[0];

		var doc = (iframe.contentWindow || iframe.contentDocument);
		if (doc.document) doc = doc.document;

		var body = jQuery(iframe).contents().find("body");
		
		if (resp["html"])
		{
		    //body.html(resp["html"]);
		    //body[0].innerHTML = resp["html"]
		    doc.open();
		    doc.write(resp["html"]);
		    doc.close();
            body = jQuery(iframe).contents().find("body");

            if (this.outputPageSize > 0) {
                var _this = this;
                body.find("table.dbnetgrid").each(function (index) { _this.transformGrid($(this), action) });
            }
            else {
                var table = body.find(".dbnetgrid:first");
                this.outputHeaderRow = table.find(".header-row");
                this.transformGrid(table, action);
                table.find('tr.data-row').removeClass('data-row');
            }
		}
		else
		{
			if (this.$("saveType").val() == "pdf")
				body.html(this.gridPanel.html());
			else
				body.html("<style>" + resp["css"] + "</style>" + this.gridPanel.html());
			this.outputHeaderRow = body.find(".dbnetgrid:first").find(".header-row");	
		}

		body.find(".expand-button").remove();
		body.find(".collapse-button").remove();
		body.find("table.toolbar").remove();
		body.find(".asc-sort-sequence-image").remove();
		body.find(".desc-sort-sequence-image").remove();
		
		this.fireEvent("onOutput", { context : action, window : iframe.contentWindow, document : doc, body : body, table : body.find(".dbnetgrid:first") });
	
		switch (action )
		{
			case "print":
				var w = iframe.contentWindow;
				w.focus();
				w.print();
				break;
		    case "copy":
		        try {
		            var textRange = doc.body.createTextRange();
		            textRange.execCommand('Copy');
		        }
		        catch (e) {
		            var range = document.createRange();
		            range.selectNode(this.gridPanel.find("table:first")[0]);
		            window.getSelection().addRange(range);
		            document.execCommand('copy');
		            window.getSelection().removeAllRanges();
		        }
				this.showMessage( this.translate("GridCopied") );
				break;
			case "datasource":
				break;
			default:
				this.exportGrid(body.html())
				break;
		}
	},
	
    saveButtonClick: function (e) {
        if (this.exportFolder) {
            this.saveExport();
        }
        else if ((this.outputCurrentPage || this.customSave) && this.gridBasedExportFormat()) {
            this.generateOutput("save");
        }
        else {
            this.exportGrid();
        }
	},

	gridBasedExportFormat: function () {
	    switch (this.$("saveType").val()) {
	        case 'xml':
	        case 'csv':
	            return false;
	    }

        return true
	},
	
	chartButtonClick : function(e) {
		this.openDialog(this.chartConfigDialog,"chart-config-dialog");
	},
	
	buildChart : function(chartConfig) {
		if (!chartConfig)
			chartConfig = this.chartConfig;
	
		if (!chartConfig.chartPanel)
			chartConfig.chartPanel = "chartPanel";
			
		if (this.$(chartConfig.chartPanel,true).length > 0)
			this.showWaiting(this.$(chartConfig.chartPanel,true));
		else if (this.chartDialog.img)
			this.showWaiting(this.chartDialog.img);
			
		var args = {chartConfig : chartConfig };
			
		this.fireEvent("onBeforeChartLoad", args);
		this.callServer("build-chart", { method : this.buildChartCallback, params : [chartConfig]}, args);	
	},			
	
	buildChartCallback : function(response,chartConfig) {
		this.$("waitPanel").hide();
		var url = this.assignHandler("dbnetgrid.ashx?method=stream-chart&chart_id=") + this.id + "_chart_" + chartConfig.chartPanel + "&id=" + new Date().valueOf();
		if (this.$(chartConfig.chartPanel,true).length > 0)
		{
			this.$(chartConfig.chartPanel,true).html("<img/>").children().attr("src",url);
		}
		else
		{
			chartConfig.chartPanel = "";
			this.chartDialog.url = url;
			this.openDialog(this.chartDialog,"chart-dialog");
		}
	},	
		
	exportGrid : function(html) {
		if (this.$("saveType").val() == "pdf")
			this.getPdfSettings(html)
		else
			this.exportGrid2(html);
	},
	
	exportGrid2 : function(html, method) {
		if ( this.$("outputForm").length === 0 )
		{
            this.addDOMElement("outputForm", null, "form");
            this.$("outputForm").attr("action", this.assignHandler("dbnetgrid.ashx"));
			this.$("outputForm").attr("target", "_blank");
			this.$("outputForm").attr("method","post");
			this.$("outputForm").hide();
			
			var input = document.createElement("input");
			input.setAttribute("name","data");
			input.setAttribute("type","hidden");
			this.$("outputForm").append(input);
		}
		
		this.saveType = this.$("saveType").val();	

		var data = {};
		
		data = this.fireOnBeforeOutput("Save");
		
		if (html)
			data.html = html;		
		
		if (this.$("saveType").val() === "pdf")
			this.pdfSettingsDialog.assignSettings(data);
		
		for (var i=0; i <this.ajaxDataProperties().length; i++)
		{
			var p = this.ajaxDataProperties()[i];
			data[p] = this[p];	
		}

        if (data.html) {
            data.html = this.htmlEncode(data.html);
        }
		
		if (method)
			data.method = method; 
		else
			data.method = "export-grid"; 
		
		var form = this.$("outputForm")[0];
		form.childNodes[0].value = DbNetLink.Util.stringify(data);		
		window.setTimeout(this.createDelegate(this.clearSaveType), 50);
		
		form.submit();
		
		if (!html)
			this.fireEvent("onOutput");		
	},

	htmlEncode : function (value){
      return $('<div/>').text(value).html();
    },

	cellText : function(idx,row) {
		if (typeof(idx) == "string")
			idx = this.columnIndex(idx);
		
		if (idx == -1)
			return;
		
		if (!row)
			row = this.selectedRows()[0];
			
		var col = this.columns[idx];
		
		var v = "";
		
		if ( this.columns[idx].display ) {
			return this.cellFromIndex(idx,row).text();
		}	
		
		return "";
	},	
	
	cellFromIndex: function (idx, row) {
	    var cellIdx;

	    if (typeof this.cellIndexCache[idx] == "undefined") {
	        var t = jQuery(row).parents("table:first");
	        var hr;
	        if (t.attr("buildMode") == "display")
	            hr = this.headerRow;
	        else
	            hr = this.outputHeaderRow;
	        var hc = hr.find("th[columnIndex=" + idx + "]");
	        cellIdx = parseInt(hc.prop("cellIndex")) + 1;
	        this.cellIndexCache[idx] = cellIdx;
	    }
	    else {
	        cellIdx = this.cellIndexCache[idx];
	    }

	    return jQuery(jQuery(row)[0].cells[cellIdx-1]);

//	    return jQuery(row).children(":nth-child(" + cellIdx.toString() + ")" );
//	    return jQuery(row).find("td:eq(" + cellIdx.toString() + ")")
	},

	columnValue : function(idx,row) {
		if (typeof(idx) == "string")
			idx = this.columnIndex(idx);
		
		if (idx == -1)
			return;
		
		if (!row)
			row = this.selectedRows()[0];
			
		var col = this.columns[idx];
		
		var v = "";
		
		if ( !this.columns[idx].display )
			v = jQuery(row).attr("hc_" + col.columnName.toLowerCase());
		else
		{
			var cell = this.cellFromIndex(idx,row)
			
			if (typeof cell.attr("value") == "string")
				v = cell.attr("value");
			else
				return cell.text();
		}	
		
		if (v == "")
			if ( col.dataType == "String" )
				return "";
			else
				return null;
				
		switch (col.dataType)
		{
			case "String":
			case "Guid":
				return v;
			case "Boolean":
				return eval(v.toLowerCase());
			case "DateTime":
				return this.deserialize(v);
			default:
				try
				{				
					return eval(v);
				}
				catch(e)
				{
					return v;
				}
		}
	},	
	
	addNestedGrid : function(handler) {
		this.bind("onNestedClick", handler);
		this.nestedGrid = true;
	},
	
	addLinkedGrid : function(grid) {
		this.addLinkedControl(grid);
	},	
	
	openNestedGrid : function(event) {
		var table = this.$(event.target).parents("table:first");
		var row = this.$(event.target).parents("tr:first");
		
		row.find(".expand-button").hide();
		row.find(".collapse-button").show();
				
		if (row.next().hasClass("nested-grid-row")) {
			row.next().show();
			return;
		}
		
		var newRow = table[0].insertRow(row[0].rowIndex+1);
		newRow.className = "nested-grid-row";
		var cell = newRow.insertCell(-1);
		cell.className = "nested-grid-cell";
		cell.colSpan = row[0].cells.length;
		
		var handlers = this.eventHandlers["onNestedClick"]
		for (var i = 0; i < handlers.length; i++) {
			if (handlers[i]) {
				this.configureNestedGrid(handlers[i],row,cell)
			}
		}
	},

	configureNestedGrid : function(handler,row,cell) {
		var gridId = "dbnetgrid" + new Date().valueOf().toString();
		jQuery(document.createElement("div")).attr("id", gridId).appendTo(jQuery(cell)); 
		var grid = new DbNetGrid(gridId);
		grid.connectionString = this.connectionString;
		grid.commandTimeout = this.commandTimeout;
		grid.parentRow = row[0];
		grid.search = false;		
		grid.parentGrid = this;
				
		var args = [grid, this]; 
		handler.apply(window,args);   
		
		var oneToOne = (this.fromPart == grid.fromPart);
		
		if (oneToOne)
		    if (grid.columnNameByProperty("foreignKey"))
		        oneToOne = (grid.columnNameByProperty("foreignKey") == this.columnNameByProperty("primaryKey"));

		if ( grid.parentFilterSql.length == 0 )
			grid.assignParentFilter(this, this.rowPrimaryKey(row), oneToOne);
		
		if (typeof(grid.parentFilterSql) == "string")
			grid.parentFilterSql = [grid.parentFilterSql];
					
		grid.loadData();
	},
	
	closeNestedGrid : function(event) {
		var table = this.$(event.target).parents("table:first");
		var row = this.$(event.target).parents("tr:first");
	
		row.find(".expand-button").show();
		row.find(".collapse-button").hide();

		if (this.collapseNestedChildren) {
            row.next().find(".collapse-button:visible").click();
		}

		row.next().hide();
	},	
	
	/*
	saveNestedConfiguration : function (config) {
		var btns = jQuery(this.gridPanel.find("img.collapse-button:visible"))
		for (var i = 0; i < btns.length; i++) {
			btn = btns[i]
			var level = (jQuery(btn).parents("table.dbnetgrid").length - 1).toString()
			if (!config[level])
				config[level] = {}
			var rowId = (parseInt(jQuery(btn).parents("tr:first").attr("rowIndex"))-1).toString()

			config[level][rowId] = true;
			
			return config;
		}

		return config;
	},
	
	restoredNestedConfiguration : function () {
		if (!this.parentGrid)
			return;
			
		var config = this.parentGrid.nestedGridConfig;
		var level = this.gridPanel.parents("table.dbnetgrid").length.toString()
		
		for (rowId in config[level]) {
			var r = config[level][rowId]
			if (r === true) {
				this.gridPanel.find("tr.data-row:eq(" + rowId + ")").find("img.expand-button").click()
				r = false;
			}
		}
	},
	*/
			
	
	editButtonClick : function(event) {
		this.editMode = this.parentElement(event.target,"button").getAttribute("buttonType").toLowerCase().replace("row","");

		var args = {cancel:false, message : ""}; 
		this.fireEvent("onBeforeRecordUpdated", args);

		if( args.cancel )
		{
			if (args.message == "")
				args.message = "Record cannot be updated";
			this.showMessage(args.message);
			return;
		}	
		
		switch (this.editMode)
		{
			case "update":
				if (this.updateMode.toLowerCase() != "page")
					if (!this.isRowSelected())
						return;	
				break;
			case "insert":
				for (var i=0; i < this.columns.length; i++)
					if (this.columns[i].bulkInsert)
					{
						this.openLookupDialog(this.columns[i].columnKey, this.createDelegate(this.bulkInsertLookupCallback));
						return;
					}
				break;
		}
			
		this.updateGrid();
	},
	
	bulkInsertLookupCallback : function(ids) {
		var data = { ids : ids};
		data.params = this.clone(this.parentFilterParams);
		this.fireEvent("onBeforeBulkInsert", data);
		this.callServer("bulk-insert", this.bulkInsertCallback, data);
	},	

	bulkInsertCallback : function(response) {
		this.loadData();
	},		
	
	updateGrid : function() {
		if (this.editMode == "update")
			if (this.updateMode == "Page")
				if ( this.dataRows.find(".dbnetgrid-input").length == 0)
				{
					this.pageUpdate = true;
					this.loadData();
					return;
				}
	
		this.editDialog.mode = this.editMode;
		this.openDialog(this.editDialog,"edit-dialog");
	},
	
	openViewDialog : function() {
		this.viewDialog.data.templatePath = this.viewTemplatePath;
		this.openDialog(this.viewDialog,"view-dialog");
	},	
	
	navigateRow : function(buttonID) {
		var currentRow = this.$(this.selectedRows()[0]);
		var newRow = [];
		
		switch(buttonID)
		{
			case "first":
				newRow = currentRow.siblings(".data-row:first");
				break;
			case "prev":
				newRow = currentRow.prevAll(".data-row:first");
				break;
			case "next":
				newRow = currentRow.nextAll(".data-row:first");
				break;
			case "last":
				newRow = currentRow.siblings(".data-row:last");
				break;
		}
		
		if(newRow.length > 0)
			this.rowClick( {target : newRow[0] } );
	},

	applyClicked: function () {
	    this.unbind("onGridAutoSaveCompleted");
	    this.apply();
	},

	apply: function () {
		if (!this.preValidationCheck(true))
			return;
			
		this.mode == "update" 

		var data = this.getBatchUpdateInfo();
	
		if (data.parameters.length == 0)
			return;
			
		this.callServer("validate-batch-update", this.commit, data);
	},
	
	commit : function(response) {
		if (!response.ok)
		{
			var r =  this.$(".data-row[dataRowIndex=" + response.rowIndex + "]");
			var field = r.find(".dbnetgrid-input[columnName='" + response.columnName + "']");
			var args = { columnName : response.columnName,message : response.message}; 
			this.fireEvent("onValidationFailed",args);
			this.highlightField(field,response.message);
			return;
		}
		
		for (var i=0; i<response.parameters.length; i++)
		{
			var r =  this.$(".data-row[dataRowIndex=" + response.rowIndex[i] + "]").get(0);
			if (!this.customValidation( response.parameters[i], response.currentRecord[i], r ))
			    return;
		}
		
		var data = this.getBatchUpdateInfo();
		this.fireEvent("onBeforeRecordBatchUpdated", data);

		for (var i = 0; i < data.parameters.length; i++) {
		    this.addUploadedFileName(data.parameters[i], this.inputControls[parseInt(data.rowIndex[i])]);
		}
		
		this.callServer("batch-update", this.commitCallback, data);
	},
	
	commitCallback : function(response) {
		if (response.ok)
		{
			this.fireEvent("onRecordBatchUpdated");
			this.showMessage(response.message);
			this.loadData({batchUpdated : true});
		}
		else
		{
			this.handleUpdateError(response);
		}
	},	
			
	cancel : function() {
		this.loadData();
	},	
	
	clearSaveType : function() {
		this.saveType = "";
	},
	
	parentDataRow : function(e) {
		var e = jQuery(e);
		return e.hasClass("data-row") ? e : e.parents(".data-row:first");	
	},	
		
	getParentInputControl : function(e) {
		var colName = this.$(e).attr("parentColumn")
	
		if (colName == null)
			return null;
		
		var cols = this.siblingInputControls(e);
		
		for(var i=0; i<cols.length; i++)
		{
			if (cols[i].getAttribute("columnName").toLowerCase() == colName.toLowerCase())	
				return cols[i];		
		}
		return null;
	},	
	
	siblingInputControls : function(e,columnIndex) {
		var pattern = ".dbnetgrid-input";
		if (columnIndex)
			pattern += "[columnIndex=" + columnIndex + "]";
		return this.parentDataRow(e).find(pattern);
	},		
	
	storeMailMergeData : function() {
		var data = { mailMergeDocument : ""};
		if ( this.$("mailMergeDocument").length > 0 )
			data.mailMergeDocument = this.$("mailMergeDocument").children(":selected").val();
			
		this.fireEvent("onBeforeMailMerge", data);	
		this.callServer("store-config", this.storeMailMergeDataCallback, data);
	},

	storeMailMergeDataCallback : function() {
	    window.document.location.href = dbnetsuite.mailMergeHta;
	},		
	
	getMailMergeData : function() {
		this.generateOutput("datasource");
	},
	
	getMailMergeDataCallback : function(resp, action) {
		this.fireEvent("onDataSourceLoaded", { response : resp });	
	},			
	
	getBatchUpdateInfo : function() {
		var data = {};
		data.parameters = [];
		data.primaryKeys = [];
		data.rowIndex = [];
		
		for ( var i = 0; i < this.dataRows.length; i++ )
		{
			var dataRow = this.dataRows[i];
			var idx = this.$(dataRow).attr("dataRowIndex");
	
			var params = this.getUpdateParams( this.inputControls[idx] );
			if (this.isEmptyObject(params))
				continue;
				
			data.parameters.push(params);	
			data.primaryKeys.push(this.primaryKeyList[ idx ]);
			data.rowIndex.push(idx);	
		}	
		
		return data;	
	},
	
	openColumnPickerDialog : function() {
		this.openDialog(this.columnPickerDialog,"column-picker-dialog");
	},
	
	openConfigDialog : function() {
		this.openDialog(this.configDialog,"config-dialog");
	},	
	
	columnFilter : function() {
		var filter = this.callServer("column-filter");
		for (var p in filter.params)
			filter.params[p] = this.deserialize(filter.params[p]);
			
		return filter;
	},		
	
	getPdfSettings : function(html) {
		this.pdfSettingsDialog.html = html;
		this.openDialog(this.pdfSettingsDialog,"pdf-settings-dialog");
	},	
	
	/* legacy code */
	
	dummy : function() {
	}
});
