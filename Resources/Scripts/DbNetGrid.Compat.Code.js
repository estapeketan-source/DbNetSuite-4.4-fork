	///////////////////////////////////////////////////////////
	// Legacy compatability code
	///////////////////////////////////////////////////////////

	convertLegacyCode : function() {
		if (this.selectPart)
			this.setColumnsProperty(this.selectPart,"columnExpression");
		if (this.headings)
			this.setColumnsProperty(this.headings,"label");
		if (this.groupBy)
			this.groupBy = true;
		if (this.filterPart)
			this.searchFilter = this.filterPart;
		if (this.fixedFilterPart)
			this.fixedFilterSql = this.fixedFilterPart;
		if (this.filterColumns)
			this.setColumnProperty(this.filterColumns, "filter", true);
	
		if (this.procedure)
			this.procedureName = this.procedure;
			
		if (this.orderColumn)
		{
			this.orderBy = this.orderColumn;
			if (this.orderSequence)
				this.orderBy += " " + this.orderSequence;
		}	
		
		if ( this.filterType )
			this.filterColumnMode = this.filterType;

		if (this.searchColumns)
			this._configColumns(this.searchColumns,this.searchLabels, "search");			
		
		if (this.editFields)
			this._configColumns(this.editFields,this.editLabels, "edit");			

		if (this.dataOnlyColumns)
			this._configColumns(this.dataOnlyColumns,null, "");			

		if (this.primaryKeyColumn)
		{
			var a = this.primaryKeyColumn.split(",");
			for (var i=0; i < a.length; i++)
				this._setColumnProperty( jQuery.trim(a[i]) , "primaryKey", true);
		}	

		if (this.searchDialogCloseOnApply)
			 this.searchDialog.closeOnApply = this.searchDialogCloseOnApply;
		if (this.editDialogCloseOnApply)
			 this.editDialog.closeOnApply = this.editDialogCloseOnApply;
			 
		this.editDialogCloseOnApply = null;


		if (this.editRow != null)
		{
			this.updateRow = eval(this.editRow);
			this.insertRow = eval(this.editRow);
			this.deleteRow = eval(this.editRow);
		}	
		
		if (this.onRowSelected)
			this.bind("onRowSelected", eval(this.onRowSelected));	
		if (this.onPageLoaded)
			this.bind("onPageLoaded", eval(this.onPageLoaded));	
		if (this.rowValidation)
			this.bind("onRecordValidate", eval(this.rowValidation));	
		if (this.onEditApply)
		{
			this.bind("onRecordInserted", eval(this.onEditApply));	
			this.bind("onRecordUpdated", eval(this.onEditApply));	
		}
		if (this.editRowInitialisation)
			this.bind("onRecordSelected", eval(this.editRowInitialisation));	

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
				if (labels)
					this._setColumnProperty(cn, "label", labels[i]);
				this._setColumnProperty(cn, "search", false);
				this._setColumnProperty(cn, "edit", false);
			}
				
			if (columnType != "")
				this._setColumnProperty(cn, columnType, true);
		}	
	},	
	
	addDetailGrid : function(grid,foreignKey) {
		grid.convertLegacyCode();
		grid._setColumnProperty(foreignKey, "foreignKey", true);
		this.addLinkedControl(grid);
	},
	
	setColumnProperty : function(cn,property,propertyValue) {
		var pn, pv;
		if (property.indexOf(":") > -1)
		{
			pn = property.split(":")[0];
			pv = property.split(":")[1];
		}
		else
		{
			pn = property;
			pv = propertyValue;
		}
		
		switch(pn.toLowerCase())
		{
			case "nowrap":
				this._setColumnProperty(cn, "style", "white-space:nowrap;");
				break;
			case "transform":
				this.bind("onCellTransform",eval(pv))
				break;
			case "onchange":
				this.bind("onFieldValueChanged",eval(pv))
				break;
			default:
				this._setColumnProperty(cn, this.convertPropertyName(pn), pv);
				break;
		}		
	},

	setGridColumnProperty : function(cn,property) {
		this.setColumnProperty(cn,property);
	},	
			
	setSearchColumnProperty : function(cn,property) {
		this._setColumnProperty(cn, this.convertPropertyName(property.split(":")[0]), property.split(":")[1]);
	},
	
	setEditColumnProperty : function(cn,property) {
		var pn = property.split(":")[0];
		var pv = property.split(":")[1];
		
		switch(pn.toLowerCase())
		{
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

	setViewColumnProperty : function(cn,property) {
		this._setColumnProperty(cn, this.convertPropertyName(property.split(":")[0]), property.split(":")[1]);
	},	
	
	setColumnLookup : function(cn,c1,c2,tn) {
		this._setColumnProperty(cn, "lookup", "select " + c1 + "," + c2 + " from " + tn);
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

    addToolbarButton : function( img, text, index, style, handler )
    { 
        var b = { img:img, text:text, index:index, style:style, handler:handler };
        this.deferredButtons.push(b);
        return b;
    },

    toolbarElement : function( id )
    { 
        for (var i=0; i < this.deferredButtons.length; i++ )
        {
            var b = this.deferredButtons[i];
            if (b["id"])
                if (b["id"] == id)
                    return b;
        }

        return {};
    },

    addDeferredButtons : function()
    { 
        for (var i=0; i < this.deferredButtons.length; i++ )
        {
            var b = this.deferredButtons[i];
            var btn = jQuery(this.addToolbarElement(b.index, "button"));

            btn.css("height","26px");

            if (b.id)
                btn.attr("id", b.id);

            btn.append("<table cellspacing=0><tr><td/><td/></tr></table>");
            btn.find("td").css("padding","0px");

            if (b.img)
            {
                var img = jQuery("<img/>");
                img.attr("src",b.img);
                btn.find("td:eq(0)").append(img);
            }

            if (b.text)
            {
                btn.find("td:eq(1)").html("&nbsp;" + b.text);
            }

            if (b.title)
                btn.attr("title", b.title);

            if (b.handler)
                btn.click(window[b.handler]);

        }
    },
	
	///////////////////////////////////////////////////////////
	// End legacy compatability code
	///////////////////////////////////////////////////////////
