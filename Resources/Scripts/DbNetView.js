var DbNetView = DbNetSuite.extend({
	init: function(id){
		this._super(id);
		DbNetLink.components[id] = this;
		jQuery(this.container).addClass("dbnetview");

		this.assignAjaxUrl("dbnetgrid.ashx");
		//this.ajaxConfig.url = "dbnetgrid.ashx";
		
		this.columns = [];
		this.dialog = null;
		this.height = null;
		this.layoutColumns = 1;
		this.navigable = false;
		this.placeHolders = [];		
		this.primaryKey = null;
		this.width = null;
		
		this.findViewPlaceholders();
	},
	
	findViewPlaceholders: function(){
		this.placeHolders = jQuery(this.container).find("*[ColumnExpression]");
		var a = [];
		for (var i=0; i < this.placeHolders.length; i++ )
			a.push(this.placeHolders[i].getAttribute("ColumnExpression"));
			
		if (a.length == 0)
			return;
		
		if (this.columns.length == 0)
			this.setColumnExpressions(a);

		for (var i=0; i < this.placeHolders.length; i++ )
		{
			var e = jQuery(this.placeHolders[i]);
			for (var j=0; j < this.columnPropertyNames.length; j++)
			{
				var a = this.columnPropertyNames[j];
				if (!e.attr(a))
					continue;
				var v = e.attr(a);
				switch (a.toLowerCase())
				{
					case "columnexpression":
						continue;
				}
				
				this._setColumnProperty( e.attr("ColumnExpression"), a, v );
			}
		}		
		
	},		
	
	ajaxDataProperties: function(values){
		return this._super().concat([]);
	},	
	
 	initialize: function(parentControl){
 		this._super();
 		
		this.fireEvent("onBeforeInitialized");
		
		this.currentPage = 1;
		this._initialize();
	},
	
 	_initialize: function(){
		var data = { placeHolders : this.placeHolders.length};
		this.fireEvent("onBeforePageLoaded");
		this.callServer("initialize", this.initializeCallback, data);
	},	
 
 	initializeCallback: function(response){
		this.addDOMElement("viewPanel",this.container);
		this.addDOMElement("toolbarPanel",this.container).addClass("toolbar-bottom");	
		this.messageLine = this.addDOMElement("messagePanel",this.container).show();	
				
		this.messageLine.html("&nbsp;").addClass("message-line alert alert-info");
 		
 		this.viewPanel = this.$("viewPanel");
		this.toolbar = this.$("toolbarPanel");
		
		this.toolbar.prop("disabled",false);
		if (this.placeHolders.length == 0)
		{
			if (this.height)
				this.viewPanel.css("height",this.height).css("overflow-y","auto");
			if (this.width)
				this.viewPanel.css("width",this.width).css("overflow-x","auto");
			this.viewPanel.show().html(response.html);	
		}
		else 
		{
			for (var ce in response.editFields)
			{
				var html = response.editFields[ce];
				var e = jQuery(this.container).find("*[ColumnExpression='" + ce + "']");
				if (e.length == 0)
					this.viewPanel.append(html);
				else
					e.html(html);
			}
		}
				
		if (response.toolbar)
		{
			this.$("toolbarPanel").html(response.toolbar);
			if (this.oneToOneParent())
				this.toolbar.find(".page-information").hide();
			this.subUserButtons();
			this.$("toolbarPanel").show();

			//this.assignToolbarHandlers();
			
			this.$("toolbar .navigation").unbind().bind("click",this.createDelegate(this.navigateParent));
		}
		
		
		this.navigable = (this.$("nextBtn").length > 0) && !this.oneToOneParent();
				
		for (var i=0; i < this.columns.length; i++ )
		{
			var col = this.columns[i];
			col.columnIndex = i;
			var e = jQuery(this.container).find("[columnName='" + col.columnName + "']")
			if (e.length == 0)
			{
				alert("Cannot find edit control ==> " + this.id + ":" + col.columnName + "[" + col.columnExpression + "]")
				break;
			}
			this.inputControls[col.columnName] = e;
			this.initialiseInput(e,col);
		}

		this.start();
	},
	
	oneToOneParent : function() {
		if (this.parentControls.length > 0)
			return this.parentControls[0].oneToOne;
		else
			return false;
	},	
	
	navigateParent: function(event) {
		var id = this.parentElement(event.target,"button").getAttribute("buttonType");
		this.parentControls[0].ctrl.navigateRow(id);
	},	
		
	subUserButtons : function() {
		var buttons = this.$("toolbarPanel").find("button");
		for (var i=0; i < buttons.length; i++ )
		{
			var btn = buttons[i];
			if (jQuery("[id=" + btn.id + "]").length > 1)
				jQuery(btn).attr("remove","true");
		}
		this.$("toolbarPanel").find("[remove='true']").parent().hide();
		this.$("toolbarPanel").find("[remove='true']").remove();
	},
		
	
	start : function() {
		this.getRecordsetCallback();
		this.initialized = true;
				
		this.fireEvent("onInitialized");
	},	
	
	getRecordSet: function(addedPrimaryKey){
		if (!this.initialized)
		{
			this.initialize();
			return;
		}
		
		var data = {};
		if (addedPrimaryKey)
			if ( !this.isEmptyObject(addedPrimaryKey))
				data.addedPrimaryKey = addedPrimaryKey;
				
		this.fireEvent("onBeforePageLoaded");
		this.callServer("get-recordset", this.getRecordsetCallback, data);
	},	
	
	getRecordsetCallback: function(response){
		this.adjustDatePrimaryKeyTimeZone();
	
		this.totalPages = this.primaryKeyList.length;
		this.totalRows = this.totalPages;
		
		if (response)
		{
			this.assignPrimaryKey();
			this.selectRecordCallback(response);
		}
		else
			this.selectRecord();

		this.fireEvent("onPageLoaded");	
		this.notifyChildControlsParentPageLoaded();
	},	

	loadData: function(){
		if (!this.initialized)
			this.initialize();
		else
			this.selectRecord();
	},	
		
	selectRecord: function(pk){
		if (this.dialog)
			if (this.container.width() == 0)
				return;
				
		this.assignPrimaryKey(pk);

		var data = { primaryKey : this.primaryKey };
		this.callServer("select-record", this.selectRecordCallback, data);
	},	
	
	assignPrimaryKey: function(pk){
		if (pk)
		{
			this.primaryKeyList = [pk];
			this.currentPage = 1;
		}
		else if (this.primaryKeyList.length == 0)
		{
			return;
		}
		if (this.currentPage > this.primaryKeyList.length)
			this.currentPage = this.primaryKeyList.length;
		if (this.currentPage < 1)
			this.currentPage = 1;
			
		this.primaryKey = this.primaryKeyList[this.currentPage-1];
	},		
 
 	selectRecordCallback: function(response){
		this.configureToolbar();		
 		this.configureEditFields("update");
		var data = response.data;
		this.clearModifiedClass();
		for(var i=0; i<this.columns.length; i++)
			this.setEditFieldValue(this.inputControls[this.columns[i].columnName], data[i], true);			
		if (response.auditinfo) {
			this.displayAuditData(response.auditinfo);	
		}
		
		if (this.browseList)
			this.browseList.selectRowByIndex(this.currentPage);
		this.assignFocus();	
		
		this.notifyChildControlsParentRowSelected();
		this.fireEvent("onRecordSelected", { record : response.record });	
	},
	
	setEditFieldValue : function(editField, editData, initialAssigment) {
		if (editData.value == null)
			editData.value = "";
		if (editData.displayValue == null)
			editData.displayValue = "";
			
		var value = editData.value;
		var displayValue = editData.displayValue;
		
		var dbValue = this.getDbValue(editField);
		
		editField.attr("fieldValue", displayValue);

		if (editData.options)
			if (editData.options != "")
				this.assignOptions(editField, editData.options);
	
		switch(editField.attr("editFieldType"))
		{
			case "CheckBox":
				editField[0].checked = eval(displayValue.toString().toLowerCase());
				editField.attr("fieldValue", editField[0].checked.toString());
				break;
			case "RadioButtonList":
				this.setRadioButtonListValue(editField, editData);
				break;
			case "DropDownList":
			case "ListBox":
				var v = jQuery.trim(value.toString()); 
				editField.val(v);
				if (editField.val() != v)
				{
					editField.val(displayValue);
					editField.attr("fieldValue", displayValue);
				}
				else
					editField.attr("fieldValue", value);
				break;
			case "Upload":
				this.setDataFieldValue(editField, editData);
				break;
			case "HtmlPreview":
				editField.parents("table:first").find(".html-content").val(displayValue);
				editField.html(displayValue);
				editField.attr("modified", "false");
				break;
			case "Html":
				editField.parents("table:first").find(".html-content").val(displayValue);
				if (window.tinyMCE)
				{
					tinymce.EditorManager.get(editField[0].id).setContent(displayValue);
					tinymce.EditorManager.get(editField[0].id).isNotDirty = true;
				}
				else
				{
					nicEditors.findEditor(editField[0].id).setContent(displayValue);
					this.nicEditorsContent[editField[0].id] = nicEditors.findEditor(editField[0].id).getContent();
				}
				break;				
			case "TextBoxLookup":
			case "TextBoxSearchLookup":
			case "SuggestLookup":
			case "AutoCompleteLookup":
				editField.val(displayValue);
				editField.attr("fieldValue", value);
				editField.attr("dbValue", value);
				editField.attr("displayValue", displayValue);
				break;
			default:
				editField.val(displayValue);
				break;
		}	
		
		if (!initialAssigment)
			this.setDbValue(editField,dbValue);

		editField.parents("table:first").find("button").each( this.createDelegate(this.updateEditors) );
	},	
	
	updateEditors : function(i,e) {
		var editField = this.$(e).parents("table:first").find(".html-preview:first")
		
		if (editField.length == 0)
			editField = this.$(e).parents("table:first").find(".dbnetedit-input:first")
	
		switch (e.getAttribute("inputButtonType") )
		{
			case "htmlEdit":
				if (this.htmlEditor.configured)
					this.htmlEditor.update();
				break;
			case "textEdit":
				if (this.textEditor.configured)
					this.textEditor.update();
				break;
		}
	},	
	
	setRadioButtonListValue : function(editField, editData) {
		var items = editField.parents("table:first").find("input[type=radio]");
		
		for (var i=0; i <items.length; i++)
		{
			var e = items[i];
			e.checked = (e.value == jQuery.trim(editData.value.toString()));
		}				

		editField.attr("fieldValue", editData.value);
	},
	
	setDataFieldValue : function(editField, editData ) {
		var img = editField.parents("table:first").find(".thumbnail-image");
		img.css({"visibility":"hidden"});
		if(editData.displayValue)
			img.attr("src",editData.displayValue);
			
		editField.val(editData.value);
		editField.attr("fieldValue", editData.value);
	},
	
    disable : function() {
		this.mode = "disabled";
		this.configureEditToolbar();
		for(var i=0; i<this.columns.length; i++)
			this.configureEditField(this.inputControls[this.columns[i].columnName]);
	},
	
	cancel : function() {	
		if(this.mode == "insert" && this.totalPages == 0)
			this.getRecordSet();
		else
			this.selectRecord();
		this.fireEvent("onEditCancelled");
	},	
	
    disableButton : function(buttonID, disable) {
		this.$( buttonID + "Btn").prop("disabled", disable); 
    },  
 				
	configureEditToolbar : function(){
		this.disableButton("search", (this.mode == "insert") );
		this.disableButton("insertRow", (this.mode == "insert") );
		this.disableButton("deleteRow", (this.mode != "update") );
		this.disableButton("apply", (this.mode == "disabled") );
		this.disableButton("cancel", (this.mode == "disabled") );
 		this.$("toolbar .custom").prop("disabled", (this.mode == "disabled"));
		this.disableButton("browse", (this.mode == "insert") );
		
		if (this.browseList)
		{
			var e = this.browseList.container.children();
			var v = (this.mode == "insert") ? "hidden" : "visible";
			e.css("visibility",v);	
		}
		if (this.mode != "update")
			this.disableNavigation();
			
		this.fireEvent("onToolbarConfigured");
	},
	
	addInputControlButton : function(columnName) {
		var ef = this.getInputControl(columnName);
		var t = ef.parents("table:first")[0];
		var c = t.rows[0].insertCell(-1);
		var b = document.createElement("button");
		c.appendChild(b);
        b.style.padding = "0px"
        b.setAttribute("type","button");
        return b;
	},	
	
	columnValue : function(columnName) {
		return this.getEditFieldValue( this.getInputControl(columnName));
	},		
	
	setInputControlValue : function(ef, value, initialValue) {
		if (typeof(ef) == "string") 
			ef = this.getInputControl(ef);
			
		if (ef == null)
			return;
		var editData = { value : value, displayValue : value };
		
		switch(ef.attr("editFieldType"))
		{		
			case "TextBoxLookup":
			case "TextBoxSearchLookup":	
			case "SuggestLookup":
			case "AutoCompleteLookup":
				ef.attr("fieldValue", value);
				ef.attr("dbValue", value);
				var data = {};
				data.value = value;
				data.columnIndex = ef.attr("columnIndex");
				var response = this.callServer("get-lookup-text", null, data);
				editData.displayValue = response.text;
				break;
		}	

		this.setEditFieldValue(ef, editData, initialValue);
	},

	disableNavigation: function() {
		this.$("pageSelect").val("");
		this.$("totalPages").val("");
		this.$("totalRows").val("");
		this.$("toolbar .navigation").prop("disabled",true);
	},	
	
    configureEditField : function(editField) {
		switch (this.mode )
		{
			case "disabled":
			case "insert":
				this.clearEditFieldValue(editField);
				break;
		}
			
		var readOnly = false;
		
		switch (this.mode )
		{
			case "disabled":
			case "readonly":	
				readOnly = true;
				break;
			default:	
				var updateReadOnly = (editField.attr("updateReadOnly").toLowerCase() == "true");
				var insertReadOnly = (editField.attr("insertReadOnly").toLowerCase() == "true");
				readOnly = (this.mode == "insert") ? insertReadOnly : updateReadOnly;
				break;
		}
		
		editField.prop("disabled", readOnly );
		editField.parents("table:first").find("button").prop("disabled", readOnly );
    },

	recordHasChanged : function() {
		if (this.skipUnappliedChangesCheck)
			return false
		else
			return this.modifiedInputControls();
	},		
	
	rowPrimaryKey : function() {
		return this.primaryKey;
	},
	
	clearRadioButtonListValue : function(editField) {
		var items = editField.parents("table:first").find("input[type=radio]");
		
		for (var i=0; i <items.length; i++)
			items[i].checked = false;
	},
	
	refreshBrowseList : function() {
		if (!this.browseList)
			return;
			
		with (this.browseList)
		{
			sql = this.browseSql();
			parameters = this.params;
			load();
		}			
	},	
	
	browseSql : function() {
		var browseColumns = [];
		for (var i=0; i < this.columns.length; i++)
		{
			var c = this.columns[i];
			if (c.browse)
				browseColumns.push(c.columnExpression);
		}
		if (browseColumns.length == 0)
			browseColumns = ["'No browse column specified'"];
		return this.sql.replace(" from ", "," + browseColumns.join(",") + " from ");
	},	
	
	configureBrowseList : function(windowedList) {
		this.browseList = new DbNetList(this.componentId + "_browsePanel");
		
		with (this.browseList)
		{
			connectionString = this.connectionString;
			rowSelection = true;
			headerRow = true;
			
			for (var i=0; i < this.columns.length; i++)
			{
				var c = this.columns[i];
				if (c.browse)
					setColumnProperty(c.columnExpression,"label",c.label );
				if (c.primaryKey)
					setColumnProperty(c.columnName,"display",false );
			}
			
			autoRowSelect = !windowedList;
			bind("onBeforeRowSelected", this.createDelegate(this.checkBrowseListSelection));
			bind("onRowSelected", this.createDelegate(this.selectFromBrowseList));
			
			sql = this.browseSql();
			parameters = this.params;
		}
		
		this.fireEvent("onBrowseListConfigured");
	},

	checkBrowseListSelection : function(list, args) {
		args.cancel = this.recordHasChanged();
	},
	
	selectFromBrowseList : function(list, args) {
		var row = parseInt(args.row.rowIndex) + (list.headerRow ? 0 : 1);
		if (row == this.currentPage)
			return;
		this.currentPage = row;
		this.loadData();	
	},	
	
	/* legacy code */
	
	dummy : function() {
	}	
	 
});
