var DataUploadDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"upload-data");
		this.loaded = false;
		this.fileName = null;
		this.tableName = "";
		this.selects = null;
		this.fixedColumnMapping = false;
		this.fixedTableMapping = false;
		this.columnPanel = null;
		this.tablePanel = null;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.open();
	},
	
	open : function() {
		this.loadColumnMapping(true);
		this.fixedTableMapping = (this.tableName != "");
	},	
	
	tableSelected : function(event) {
		var s = jQuery(event.target);
		if (this.fixedTableMapping) {
				s.val(this.tableName)
			this.showMessage("Table mapping is fixed.");
			return;
		}	
		this.tableName = s.val();
		this.loadColumnMapping(false);
	},		
	
	loadColumnMapping : function(loadTables) {
		var data = {};
		data.fileName = this.fileName;
		data.tableName = this.tableName;
		data.loadTables = loadTables;
		this.parentControl.showWait();
		this.parentControl.callServer("column-mapping", { method : this.loadColumnMappingCallback, context : this}, data);
	},	
		
	loadColumnMappingCallback : function(response) {
		this.parentControl.hideWait();

		this.tablePanel = this.dialog.find(".table-select-panel");
		this.columnPanel = this.dialog.find(".column-select-panel");
		
		if (response.message != "") {
		    if (response.tableNotFound)
		        this.showMessage(response.message);
		    else {
		        this.parentControl.messageBox("error", response.message);
		        this.columnPanel.empty();
		        if (!this.isOpen())
		            return;
		    }
		}
		
		if (response.table_select) {
			this.tablePanel.html(response.table_select);
			this.tableName = this.tablePanel.find(".table-select").val();
			this.tablePanel.find(".table-select").change(this.createDelegate(this.tableSelected))
		}
		
		if (response.column_select)	{
			this.columnPanel.html(response.column_select);
			
			this.selects = this.columnPanel.find("select");
			this.selects.change(this.createDelegate(this.columnMappingChanged))
			
			var selectedValues = []
			this.selects.each( function(idx){ if (jQuery(this).prop("selectedIndex") > 0) selectedValues.push(jQuery(this).val()) } )
			
			this.selects.each( function(idx){ 
				for (var i = 0; i < selectedValues.length; i++) {
					if (jQuery(this).val() != selectedValues[i] )
						jQuery(this).find("option[value='" + selectedValues[i] + "']").remove();
				}
			});
			 
			this.fixedColumnMapping = response.fixedColumnMapping;
			 
			if (this.fixedColumnMapping){	
				this.columnPanel.find("select").each(function(){ 
					if (!jQuery(this).attr("uploadDataColumn"))
						jQuery(this).val("").addClass("un-mapped");
					}) 
			}
			 
			this.setApplyState();
		}
		
		this.dialog.dialog("open");
		this.setPanelHeight(this.columnPanel);
		this.sizeDialog();
		this.setTitleWidth();	
		
		this.parentControl.fireEvent("onColumnMappingConfigured", {dialog : this});
	},	
	
	setApplyState : function() {
		var disable = (this.columnPanel.find("select").filter(function() { return this.value != ""; }).length == 0);
		this.dialog.find(".apply-button").prop("disabled",disable);
	},
	
	columnMappingChanged : function(event) {
		var s = jQuery(event.target);

		if (this.fixedColumnMapping) {
			if (s.attr("currentValue"))	
				s.val(s.attr("currentValue"))
			else
				s.val("")
			this.showMessage("Column mapping is fixed.");
			return false;
		}
		
		if (s.prop("selectedIndex") == 0) 
			s.addClass("un-mapped");
		else
			s.removeClass("un-mapped");
			
		if (s.attr("currentValue"))	{
			this.addValueFromSelects(s.attr("currentValue"));
			s.removeAttr("currentValue");
		}
			
		if (s.val() != ""){
			s.attr("currentValue",s.val());
			this.removeValueFromSelects(s.val());
		}
		this.setApplyState();
	},	
	
	addValueFromSelects : function(value) {
		this.selects.each( function(idx){ 
			var s = jQuery(this);
			if ( s.find("option[value='" + value + "']").length == 0){
				s.children().each(function(idx){ 
					var opt = jQuery("<option/>", {value: value, text: value	});
					var o = jQuery(this);
					var v = o.attr("value");
	
					if (value < v && v != "") {
						o.before(opt);
						return false;
					}
					else if ( o.next().length == 0) {
						o.after(opt);
						}
					})
			}
		});	
	},		
			
	removeValueFromSelects : function(value) {
		this.selects.each( function(idx){ 
			if (jQuery(this).val() != value )
				jQuery(this).find("option[value='" + value + "']").remove();
		 });
	},	
		
	apply : function() {
		var mapping = {};
		this.dialog.find("tr.column-map-data").each( function(){
			mapping[jQuery(this).children(":first").text()] = jQuery(this).find("select").val();
		});
		var data = {};
		data.tableName = this.tableName;
		data.fileName = this.fileName;
		data.mapping = mapping;
		this.parentControl.showWait();
		this.parentControl.callServer("load-data-upload", { method : this.applyCallback, context : this}, data);
	},	
	
	applyCallback : function(response) {
		this.parentControl.hideWait();
		if (response.message == "") {
			if (this.fixedTableMapping || this.tablePanel.find(".table-select").children().length == 1)
				this.close();
			response.dialog = this;
			this.parentControl.fireEvent("onDataUploaded", response);	
			this.parentControl.messageBox("info", response["record_count"].toString() + " records loaded");
			this.parentControl.getRecordSet();
		}
		else {
			this.parentControl.messageBox("error", response.message);
		}
	}			
});
