	///////////////////////////////////////////////////////////
	// Legacy compatability code
	///////////////////////////////////////////////////////////

	convertLegacyCode : function() {
		if ( this.tableName )
			this.fromPart = this.tableName;

		if (this.primaryKeyName)
		{
			var a = this.primaryKeyName.split(",");
			for (var i=0; i < a.length; i++)
				this._setColumnProperty( jQuery.trim(a[i]), "primaryKey", true);
		}	
		
		if (this.filterPart)
			this.searchFilter = this.filterPart;
		if (this.fixedFilterPart)
			this.fixedFilterSql = this.fixedFilterPart;

		if (this.searchFields)
			this._configColumns(this.searchFields,this.searchLabels, "search");		
		if (this.browseColumns)
			this._configColumns(this.browseColumns,this.browseTitles, "browse");	
			
		if (this.browseContainer)
			jQuery("#" + this.browseContainer ).attr("id", this.componentId + "_browsePanel").css("overflow-y","auto");
		
		if (this.searchDialogCloseOnApply)
			 this.searchDialog.closeOnApply = this.searchDialogCloseOnApply;
			
		if (this.rowInitialisation)
			this.bind("onRecordSelected", eval(this.rowInitialisation));	
		if (this.onPageLoaded)
			this.bind("onPageLoaded", eval(this.onPageLoaded));	
		if (this.rowValidation)
			this.bind("onRecordValidate", eval(this.rowValidation));	
		if (this.onEditApply)
		{
			this.bind("onRecordInserted", eval(this.onEditApply));	
			this.bind("onRecordUpdated", eval(this.onEditApply));	
		}
						
	},
	
	addLinkedEditForm : function(editControl,foreignKey) {
		editControl.convertLegacyCode();
		editControl._setColumnProperty(foreignKey, "foreignKey", true);
		this.addLinkedControl(editControl);
	},	
	
	setFieldProperty : function(cn,property) {
		var pn = property.split(":")[0];
		var pv = property.split(":")[1];
		
		switch(pn.toLowerCase())
		{
			case "validate":
				this.bind("onColumnValidate",eval(pv));
				break;		
			case "html":
				this._setColumnProperty(cn, "editControlType", "Html");
				break;			
			case "editlookup":
				this._setColumnProperty(cn, this.convertPropertyName(pn), pv);
				this._setColumnProperty(cn, "editControlType", "TextBoxLookup");
				break;
			case "lookuptext":
				this._setColumnProperty(cn, "editControlType", "TextBoxLookup");
				break;				
			default:
				this._setColumnProperty(cn, this.convertPropertyName(pn), pv);
				break;
		}			
	},
			
	setSearchFieldProperty : function(cn,property) {
		this._setColumnProperty(cn, this.convertPropertyName(property.split(":")[0]), property.split(":")[1]);
	},
	
	_configColumns : function(columns, labels, columnType) {
		if (columnType != "")
			this.resetProperty(false,columnType);
		for (var i=0; i<columns.length; i++)
		{
			var cn = columns[i];
			
			if (this.columnIndex(cn) == -1)
			{
				this.columns.push( {columnExpression : cn, display : false} );
				this._setColumnProperty(cn, "search", false);
			}
			if (labels)
				this._setColumnProperty(cn, "label", labels[i]);
				
			if (columnType != "")
				this._setColumnProperty(cn, columnType, true);
		}	
	},	
		
	convertPropertyName : function(pn) {
		switch(pn.toLowerCase())
		{
			case "searchlookup":
			case "editlookup":
				return "lookup"
				break;
			default:
				return pn;
				break;
		}		
	},
	
	_convertPlaceHolders: function(){
		var ph = jQuery(this.container).find(".dbnetedit");
		for (var i=0; i < ph.length; i++ )
		{
			var e = jQuery(ph[i]);
			e.after("<span columnExpression='" + e.attr("id") + "'></span>");
		}	
		jQuery(this.container).find(".dbnetedit").hide();
		jQuery(this.container).find("#toolbar").attr("id", this.componentId + "_toolbarPanel");
		jQuery(this.container).find("#messageLine").attr("id", this.componentId + "_messagePanel");
	},	
	
	initialise: function(){
		this.initialize();	
	},			
	
	///////////////////////////////////////////////////////////
	// End legacy compatability code
	///////////////////////////////////////////////////////////
