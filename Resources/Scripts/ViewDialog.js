var ViewDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"view");
		this.action = "print";
		this.viewElements = [];
		this.record = null;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".close-button").click(this.createDelegate(this.close));
		this.dialog.find(".print-button").click(this.createDelegate(this.print));
		this.dialog.find(".copy-button").click(this.createDelegate(this.copy));
		
		this.dialog.find(".next-button").click(this.createDelegate(this.nextRecord));
		this.dialog.find(".prev-button").click(this.createDelegate(this.prevRecord));
		this.dialog.find("input[type=checkbox]").click(function (){return false;});

		var template = this.parentControl.container.find(".view-dialog-template")
		if ( template.length )
			this.dialog.find(".view-dialog-panel").html(template.html());

		this.viewElements = this.dialog.find("[columnName]");	
		this.updateAttributes();	
		
		this.parentControl.fireEvent("onViewDialogInitialized", {dialog : this});
		
		this.parentControl.bind("onRowSelected", this.createDelegate(this.selectRecord));		
		
		this.open();
	},
	
	updateAttributes : function() {	
		for (var i=0; i < this.parentControl.columns.length; i++) {
			var c = this.parentControl.columns[i];
			var e = this.viewElement(c.columnName);
				
			if (e == null)
				continue;
			c.view = true;
			e.addClass("view-element");	
			if (e.is("img"))
				e.addClass("view-image");
			else if (e.is("[type=checkbox]"))
				e.addClass("view-checkbox");
			else
				e.addClass("view-text");
			
			e.attr("dataType", c.dataType);
			e.attr("label", c.label);
		}		
	},
	
	viewElement : function(cn) {
		for (var i=0; i < this.viewElements.length; i++) {
			var e = jQuery(this.viewElements[i]);
			if (cn.toLowerCase() == e.attr("columnName").toLowerCase())
				return e
		}
		return null;
	},	
	
	open : function() {
		if (this.parentControl.selectedRows().length == 0)
			return;

		this._super();
		this.selectRecord();
	},

	selectRecord : function() {
		if (!this.isOpen())
			return;
			
		var data = {};
		data.primaryKey = this.parentControl.rowPrimaryKey();
		this.parentControl.callServer("view-record", this.createDelegate(this.selectRecordCallback) ,data );		
	},
		
	selectRecordCallback : function(response) {
		if (!response.record) {
			return;
		}
			
		this.record = response.record;
		
		for (var i=0; i < this.parentControl.columns.length; i++) {
			var c = this.parentControl.columns[i];
			if (c.view) {
				var v = response.record[c.columnName.toLowerCase()];
				var cell = this.viewElement(c.columnName);
				
				if (cell == null)
					continue;
				
				if (cell.is("img"))
					cell.attr("src",v);
				else if (cell.is("[type=checkbox]"))
					cell[0].checked = (v.toLowerCase() == "true");
				else
					cell.html(v);
				
				var args = {};
				args.cell = cell;
				args.row = this.parentControl.currentRow;
				args.columnName = c.columnName;
				args.context = "view";
				
				this.parentControl.fireEvent("onCellTransform", args);
			}	
		}			
	
		this.setTitleWidth();
		var args = {};
		args.dialog = this;
		args.record = response.record;
		this.parentControl.fireEvent("onViewRecordSelected", args);			
	},	
		
	nextRecord : function(event) {
		this.parentControl.navigateRow("next");
	},
	
	prevRecord : function(event) {
		this.parentControl.navigateRow("prev");
	},	
	
	copy : function() {	
		this.action = "copy";
		this.parentControl.callServer("view-css", this.createDelegate(this.printCallback));
	},
		
	print : function() {	
		this.action = "print";
		this.parentControl.callServer("view-css", this.createDelegate(this.printCallback));
	},
	
	printCallback : function(response) {
		var iframe = this.parentControl.$("outputFrame")[0];
		var doc = (iframe.contentWindow || iframe.contentDocument);
		if (doc.document) doc = doc.document;
		var body = jQuery(iframe).contents().find("body");
		
		var link = jQuery("<style>" + response.css + "</style>");
		body.empty();
		body.append(link);
		body.append(this.dialog.find(".view-dialog-panel").clone());
		
		this.parentControl.fireEvent("onViewOutput", { context : this.action, window : iframe.contentWindow, document : doc, body : body, record : this.record });		
		
		switch (this.action )
		{
			case "print":
				var w = iframe.contentWindow;
				w.focus();
				w.print();
				break;
			case "copy":
				var textRange = doc.body.createTextRange();
				textRange.execCommand('Copy');
				this.showMessage( this.parentControl.translate("Copied") );
				break;
		}		
	}
});
