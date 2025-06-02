var DbNetEdit = GridEditControl.extend({
	init: function(id){
		this._super(id);
		window.DbNetEditArray[id] = this;
		DbNetLink.components[id] = this;
		jQuery(this.container).addClass("dbnetedit");
		this.assignAjaxUrl("dbnetedit.ashx");
		//this.ajaxConfig.url = "dbnetedit.ashx";
		
		this.browseDialogWidth = 300;
		this.browseDialogHeight = 300;
		this.columnOrder = 0;
		this.dialog = null;
		this.height = null;
		this.htmlEditors = [];
		this.insertOnly = false;
		this.layoutColumns = 1;
		this.nicEditorsContent = {};		
		this.htmlEditorsInitialised = 0;
		this.mode = "";
		this.navigable = false;
		this.placeHolders = [];		
		this.primaryKey = null;
		this.toolbarLocation = "bottom";
		this.width = null;
		this.browseDialog = new BrowseDialog(this);
		this.browseList = null;
		this.sql = null;
		this.search = true;
		this.params = null;
		
		/* column property names */
		/* legacy properties */
		
		this.findEditFields();
	},
	
	findEditFields: function(){
		this.placeHolders = jQuery(this.container).find("*[ColumnExpression]");
		var a = [];
		for (var i=0; i < this.placeHolders.length; i++ )
			a.push(this.placeHolders[i].getAttribute("ColumnExpression"));
			
		if (a.length == 0)
			return;
		
		if (this.columns.length == 0)
		    this.setColumnsProperty(a, "columnExpression");

		for (var i=0; i < this.placeHolders.length; i++ )
		{
			var e = jQuery(this.placeHolders[i]);
			for (var j=0; j < this.columnPropertyNames.length; j++)
			{
				var a = this.columnPropertyNames[j];
				var v = e.get(0).getAttribute(a);
				if (v == null)
					continue;
				switch (a.toLowerCase())
				{
					case "columnexpression":
					case "style":
						continue;
					case "editcontrolproperties":
						try
						{
							v = JSON.parse(v);
						}
						catch(e)
						{
							continue;
						}
						break;
				}
				
				this._setColumnProperty( e.attr("ColumnExpression"), a, v );
			}
			e.attr("tabindex",-1);
			if (e.attr("style"))
				this._setColumnProperty( e.attr("ColumnExpression"), "style", e.attr("style") );
		}		
		
	},		
	
	ajaxDataProperties: function(values){
		return this._super().concat(["mode","insertOnly","layoutColumns","columnOrder"]);
	},	
	
 	initialize: function(parentControl){
 		this._super();
 		
		if (this.convertLegacyCode)
			this.convertLegacyCode();
 		
/*
		for (var i=0; i < this.columns.length; i++ )
			if (this.columns[i].editControlType == "Html")
				if (!window.tinyMCE)
				{
					this.columns[i].editControlType = "HtmlPreview";
					alert("TinyMCE libraries not available reverting to preview");
				}
*/
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
		
		if (this.insertOnly)
			this.fixedFilterSql = "1=2";
		
		this.fireEvent("onBeforeInitialized");
		
		this.currentPage = 1;
		this._initialize();
	},
	
 	_initialize: function(){
		var data = { placeHolders : this.placeHolders.length};
		if (this.primaryKey != null)
			data.primaryKey = this.primaryKey;
		if (this.primaryKeys != null)
			data.primaryKeys = this.primaryKeys;
		
		this.fireEvent("onBeforePageLoaded");
		this.callServer("initialize", this.initializeCallback, data);
	},	
 
 	initializeCallback: function(response){
 		if ( this.$("toolbarPanel").length == 0)
 		{
 			if (this.toolbarLocation.toLowerCase() == "top")
				this.addDOMElement("toolbarPanel",this.container).addClass("toolbar-top");
			this.addDOMElement("editPanel",this.container);
			if (this.audit)
				this.addDOMElement("auditPanel",this.container);
			
			if (this.toolbarLocation.toLowerCase() != "hidden")
				this.addDOMElement("toolbarPanel",this.container).addClass("toolbar-bottom");	
		}
		
		if (this.messageLine == null)
		{
			this.messageLine = this.$("messagePanel");
			if (this.messageLine.length == 0)
				this.messageLine = this.addDOMElement("messagePanel",this.container).show();	
				
			this.messageLine.html("&nbsp;").addClass("message-line alert alert-info");
 		}
 		
 		this.editPanel = this.$("editPanel");
		this.editPanel.prop("disabled",false);
		this.toolbar = this.$("toolbarPanel");
		
		this.toolbar.prop("disabled",false);
		if (this.placeHolders.length == 0)
		{
			if (this.height)
				this.editPanel.css("height",this.height).css("overflow-y","auto");
			if (this.width)
				this.editPanel.css("width",this.width).css("overflow-x","auto");
			this.editPanel.show().html(response.html);	
		}
		else 
		{
			for (var ce in response.editFields)
			{
				var html = response.editFields[ce];
				var e = jQuery(this.container).find("*[ColumnExpression='" + ce + "']");
				if (e.length == 0)
					this.editPanel.append(html);
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

			this.assignToolbarHandlers();
			
			this.$("spellCheckBtn").bind("click",this.createDelegate(this.checkSpelling));
			
			if (this.$("browsePanel").length > 0)
			{
				this.$("browseBtn").hide();
				this.configureBrowseList(false);
			}	
			else
			{		
				this.$("browseBtn").bind("click",this.createDelegate(this.browse));
			}
			
			if (this.oneToOneParent())
				this.$("toolbar .navigation").unbind().bind("click",this.createDelegate(this.navigateParent));
		}
		
		if (response.audit)	{
			this.$("auditPanel").attr("class","audit-panel").html(response.audit).show();	
			this.$("auditPanel").find(".audit-history").bind("click", this.createDelegate(this.auditHistory));
		}	
		
		if (response.searchPanel)
			this.configureSearchPanel(response);	
			
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
			e.tooltip();
			this.inputControls[col.columnName] = e;
			this.initialiseInput(e,col);
		}

		if (this.htmlEditors.length > 0)
			this.configureHtmlEditors(response);
		else
			this.start(response);
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
		
	configureHtmlEditors : function(response) {
		this._response = response;
		if (window.tinyMCE)
			this.configireTinyMCE()
		else
			this.configireNicEdit()
	},
	
	configireTinyMCE: function () {
	    var config = {};
	    if (tinyMCE.majorVersion == 4) {
	        config.init_instance_callback = this.createDelegate(this.htmlEditorsInitialized);
	        config.theme = "modern";
	        config.selector = "#" + this.htmlEditors.join(",#");
        }
	    else {
	        config.theme_advanced_buttons1 = "bold,italic,underline,strikethrough,|,undo,redo,|,cleanup,|,bullist,numlist|,formatselect,fontselect,fontsizeselect";
	        config.theme_advanced_buttons2 = "";
	        config.theme_advanced_buttons3 = "";
	        config.isAdvancedMode = false;
	        config.mode = "exact";

	        config.oninit = this.createDelegate(this.htmlEditorsInitialized);

	        config.elements = this.htmlEditors.join(",");
	        config.theme = "advanced";
	        config.theme_advanced_toolbar_location = "top";
	        config.theme_advanced_toolbar_align = "left";
	        config.theme_advanced_toolbar_location = "top";
	    }

	    this.fireEvent("onBeforeTinyMceInit", { config: config });

	    tinyMCE.init(config);
	},
	
	configireNicEdit : function() {
		var config = {};
		config.iconsPath = dbnetsuite.nicEditGif;
		config.fullPanel = true;
		
		this.fireEvent("onBeforeNicEditInit", {config : config });

		for (var i=0; i< this.htmlEditors.length; i++)
		{
			config.maxHeight = jQuery("#" + this.htmlEditors[i] ).height();
			if (config.maxHeight == 0)
				config.maxHeight = jQuery("#" + this.htmlEditors[i] )[0].style.height.replace(/[a-z]/g,"");

			var ne = new nicEditor(config).panelInstance(this.htmlEditors[i]);
		}
		this.htmlEditorsInitialized();	
	},	
	
	htmlEditorsInitialized : function() {
		this.start(this._response);
	},	
	
	start : function(response) {
		this.disable();
		this.getRecordsetCallback(response);
		this.initialized = true;
		
		for (var cn in this.inputControls)	
			this.bindSpellChecker(this.inputControls[cn]);
			
		if (this.dbNetSpell.elements.length > 0)
		{
			if (this.dbNetSpell.connectionString == "")
				this.dbNetSpell.connectionString = this.connectionString;

			this.dbNetSpell.createButton = false;
			this.dbNetSpell.initialize();
		}			
				
		this.fireEvent("onInitialized");
		
		if (this.insertOnly)
			this.initializeInsert();
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
		
		if (this.totalPages > 0)
		{
			if (response)
			{
				this.assignPrimaryKey();
				this.selectRecordCallback(response);
			}
			else
				this.selectRecord();
		}	
		else
		{
			this.disable();
			this.configureToolbar();	
		}
		this.refreshBrowseList();
		this.fireEvent("onPageLoaded");	
		this.notifyChildControlsParentPageLoaded();

		if (this.userProfile) {
		    if (this.profileOnLoad == null) {
		        this.profileOnLoad = this.getProfileProperties();
		    }
		    if (response.defaultUserProfileId) {
		        this.selectDefaultUserProfile(response.defaultUserProfileId);
		    }
		}
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
	
	hideAuditData : function() {
		for (var cn in this.inputControls) {
			var table = this.inputControls[cn].parents("table:first");
			this.hideAuditDataItem(table, "updated");
		}

		this.hideAuditDataItem(this.$("auditPanel"), "updated");
		this.hideAuditDataItem(this.$("auditPanel"), "created");
	},
		
	displayAuditData : function(auditinfo) {
		if (auditinfo._row_created)
			this.displayAuditDataItem(auditinfo._row_created, this.$("auditPanel"), "created");

		if (auditinfo._row_updated)
			this.displayAuditDataItem(auditinfo._row_updated, this.$("auditPanel"), "updated");
			
		for (var columnName in auditinfo) {
			switch(columnName){
				case "_row_updated":
				case "_row_created":
					continue;
				default:
					var e = this.getInputControl(columnName)
					if (e == null)
						continue;
					this.displayAuditDataItem(auditinfo[columnName], e.parents("table:first"), "updated");
					break;
			}
		}
	},	
	
	displayAuditDataItem : function(auditItem, container, action) {
		var suffix = ["","_by"];
		
		for (var i=0; i< suffix.length; i++) {
			var key = "updated" + suffix[i];
			if (auditItem[key] != "") {
				container.find("." + action + suffix[i].replace("_","-") + "-audit").text(auditItem[key]).show();
				container.find("." + action + "-audit-label").show();
				
				if (action == "updated")
					container.find(".audit-history").show();
			}
		}
	},		
	
	hideAuditDataItem : function( container, action) {
		container.find("." + action + "-audit").hide()
		container.find("." + action + "-by-audit").hide()
		container.find("." + action + "-audit-label").hide();
		container.find(".audit-history").hide();
	},		
	
	editButtonClick : function(event) {
		this.initializeInsert();
	},
		
	initializeInsert : function() {
		this.hasUnappliedChanges( this.createDelegate(this.initializeInsertCallback));
	},
	
	initializeInsertCallback : function(buttonPressed) {
		if (buttonPressed != "yes")
			return;
			
		this.insertRecord();
	},
	
	insertRecord : function() {
		if (this.browseDialog.isOpen())
			this.browseDialog.close();
					
		this.configureEditFields("insert");
		this.assignForeignKeyValues(false);	
		this.assignFocus();	
		this.noData = (this.totalRows == 0);
	//	this.disableNavigation();

	//	this.disableChildControls();
	
		for(var i=0; i < this.childControls.length; i++)
			this.childControls[i].ctrl.clear();

		this.fireEvent("onInsertInitialize");
	},

	applyClicked: function () {
	    this.unbind("onEditAutoSaveCompleted")
	    for (var i = 0; i < this.parentControls.length; i++) {
	        var ctrl = this.parentControls[i].ctrl;
	        if (ctrl instanceof DbNetGrid)
	            ctrl.unbind("onGridAutoSaveCompleted")
	    }

	    this.apply();
	},
	
	apply: function () {
	    if (!this.preValidationCheck((this.mode == "update")))
			return false;
	
		var params = this.getUpdateParams(this.inputControls);
	
		if (this.isEmptyObject(params))
			return true;

		var data = { params : params };
	
		if (this.mode == "update")
			data.primaryKey = this.primaryKey;

		this.callServer("validate-edit", this.createDelegate(this.commit), data);

		return true;
	},

	commit : function(response) {
		if (!response.ok)
		{
			var args = { columnName : response.columnName,message : response.message}; 
			this.fireEvent("onValidationFailed",args);
			this.highlightField(this.inputControls[response.columnName],args.message);
			return; 
		}
		
		response.parameters = this.deserializeRecord(response.parameters);
		
		for (var key in response.parameters )
			if (response.parameters[key])
				if (response.parameters[key].toUTCString)
				    response.parameters[key] = this.getLocalDateObject(response.parameters[key]);
		
		if (!this.customValidation( response.parameters, response.currentRecord ))
			return

		this.addUploadedFileName(response.parameters, this.inputControls);

		var data = {};
		data.params = response.parameters;
		data.primaryKey = this.primaryKey;
		data.foreignKeyValues = this.assignForeignKeyValues(true);	
		
		this.callServer( this.mode + "-record", this.createDelegate(this.commitCallback), data);
	},
	
	commitCallback : function(response) {
		if (response.ok)
		{
			var editMode = this.mode;
			var args = {};
			args.message = response.message;
			if(editMode == "insert")
			{	
				args.id = response.autoIncrementValue;
				args.inserted = this.deserializeRecord(response.inserted);
				this.autoIncrementValue = response.autoIncrementValue;
				this.primaryKey = response.primaryKey;
	
				this.fireEvent("onRecordInserted", args);
			}		
			else	
			{											
				args.updated = this.deserializeRecord(response.updated);
				this.refreshParentControls(editMode);	
										
				this.fireEvent("onRecordUpdated", args);
				this.fireEvent("onEditAutoSaveCompleted", args);
			}		
			
			this.showMessage(args.message);
			
			if(editMode == "insert")
			{	
				if (this.insertOnly)
					this.insertRecord();
				else if (this.navigable)
					this.getRecordSet(this.primaryKey);
				else
					this.refreshParentControls(editMode);							
			}
			else
			{
			    if (this.reloadAfterUpdate) {
			        this.cancel();
			    }
			}
		}
		else
		{
			this.handleUpdateError(response);
		}
	},	
	
	browse : function() {
		this.browseDialog.data.width = this.browseDialogWidth;
		this.openDialog(this.browseDialog,"browse-dialog");
	},
	
	refreshParentControls : function(mode) {
		for (var i=0; i < this.parentControls.length; i++)
		{
			var ctrl = this.parentControls[i].ctrl;
			if (ctrl instanceof DbNetGrid && this.parentControls[i].oneToOne)
			{
				switch(mode)
				{
					case "update":
						ctrl.refreshRow();
						break;
					case "insert":
					    ctrl.loadData({ primaryKey: this.primaryKey });
						break;
					case "delete":
						ctrl.reload();
						break;
				}
			}
		}
	},
	
	assignForeignKeyValues : function(returnValues) {
		var ctrl = this;
		var o = {};
		if (this.parentControls.length > 0)
			if (this.parentControls[0].oneToOne)
				ctrl = this.parentControls[0].ctrl;
		
		for(var i=0; i< this.columns.length; i++)
		{
			var col = this.columns[i];
			if (col.foreignKey)	{
				var editField = this.inputControls[col.columnName];
				var idx = ctrl.columnIndex(col.columnName);
				var paramName = "detailCol" + col.columnKey + "Param0";
				var paramValue = ctrl.parentFilterParams[paramName];
				if (!paramValue)
					for (p in ctrl.parentFilterParams)	{
						paramName = p;
						break;
					}
				if (returnValues) {
					o[paramName] = paramValue;
				}
				else {
					var d = this.deserialize(paramValue);
					if (d instanceof Date) {
						d = this.reAdjustTimeZone(d);
						paramValue = DbNetLink.Util.dateToString(d,null,true);
					}
					editField.val(paramValue);

					if (editField.attr("dependentColumnIndex"))
					    editField.change();
				}
			}
		}
		
		return o;
	},	
	
    configureEditFields : function(mode) {
		this.skipUnappliedChangesCheck = false;
		this.mode = mode;
		
		this.configureEditToolbar();
		
		if (this.audit)
			this.hideAuditData();

		for(var i=0; i<this.columns.length; i++)
		{
			var editField = this.inputControls[this.columns[i].columnName];
			this.configureEditField(editField);
			
			if (mode == "insert"){
				if ( editField.attr("initialValue") != "" && editField.attr("initialValue") != undefined)
					this.setInputControlValue(editField,editField.attr("initialValue"),true);
					
			}
			
			switch ( editField.attr("editFieldType") )
			{
				case "Upload":
					editField[0].disabled = true;
					break;
			}
		}
    },
    
    assignFocus : function() {
		for(var i=0; i<this.columns.length; i++) {
			var e = this.inputControls[this.columns[i].columnName];
			if (e.attr("placeholder"))
				e.trigger("blur");
		}
		    
		for(var i=0; i<this.columns.length; i++) {
			var editField = this.inputControls[this.columns[i].columnName][0];
			try
			{
				if ( !editField.disabled && !editField.readOnly)
				{
					editField.focus();
					break;
				}
			}
			catch(e){}
		}
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
                
				if (this.dbValueNotInlist(editField))
				    editField.val('');
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

    dbValueNotInlist : function (combo) {
        return (combo.children('[value="' + combo.attr("fieldValue") + '"]').length == 0);
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
		img.css({ "visibility": "hidden" });

		if (editData.displayValue) {
		    if (editData.displayValue.indexOf("data:") == 0)
		        img.css({ "visibility": "visible" });
		    img.attr("src", editData.displayValue);
		}

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
	
	getInputRow : function(columnName) {
		var e = this.getInputControl(columnName);
		return jQuery(e).parents("tr.edit-row:first");
	},		
	
	getLabel: function (columnName) {
	    var e = this.getInputControl(columnName);
	    return jQuery(e).parents(".edit-field-table:first").parent().prev();
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
				    setColumnProperty(c.columnName, "display", false);

				setColumnProperty(c.columnExpression, "format", c.format);
				setColumnProperty(c.columnExpression, "lookup", c.lookup);
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
