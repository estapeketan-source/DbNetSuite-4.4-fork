var EditDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"edit");
		this.editControl = null;
		this.firstOpen = true;
	},
	
	configure : function() {
		this._super();
		
		this.bind("onOpen",this.createDelegate(this.checkDialogSize));
		this.bind("onClose",this.createDelegate(this.disableInsert));
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.cancel));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".next-button").bind("click",this.createDelegate(this.nextRecord));
		this.dialog.find(".prev-button").bind("click",this.createDelegate(this.prevRecord));
		this.dialog.find(".insert-button").bind("click",this.createDelegate(this.insertRecord));
		this.dialog.find(".spellCheck-button").bind("click",this.createDelegate(this.checkSpelling));

		this.dialog.parents(".ui-dialog:first").css("overflow","visible");
//		this.parentControl.bind("onRowSelected", this.createDelegate(this.selectRecord));
		
		this.createEditControl();
	},
	
	createEditControl : function() {
		this.editControl = new DbNetEdit(this.parentControl.id + "editDialogPanel");
		
		var copyProperties = ["connectionString","fromPart","parentFilterParams","spellCheck","dbNetSpell","audit","auditDateFormat","auditUser","messageTimeout"];
		
		for (var p in copyProperties)
			this.editControl[copyProperties[p]] = this.parentControl[copyProperties[p]];	
		
		/*
		this.editControl.connectionString = this.parentControl.connectionString;	
		this.editControl.fromPart = this.parentControl.fromPart;	
		this.editControl.parentFilterParams = this.parentControl.parentFilterParams;	
		this.editControl.spellCheck = this.parentControl.spellCheck;	
		this.editControl.dbNetSpell = this.parentControl.dbNetSpell;	
		*/
		
		this.editControl.messageLine = this.messageLine;
		this.editControl.toolbarLocation = "hidden";
		this.editControl.fixedFilterSql = "1=2";
		this.editControl.layoutColumns = this.parentControl.editLayoutColumns;
		this.editControl.commandTimeout = this.parentControl.commandTimeout;
		this.editControl.autoSave = this.parentControl.autoSave;
		this.editControl.multiValueLookupSelectStyle = this.parentControl.multiValueLookupSelectStyle;
		this.editControl.dialog = this;
		
		for (var h in this.parentControl.eventHandlers)
		{
			switch(h)
		    {
			    case "onBeforeFileUploadValidate":
			    case "onBeforeFileUploaded":
			    case "onFileUploaded":
			    case "onBeforeRecordValidated":
				case "onBeforeTinyMceInit":
				case "onColumnValidate":
				case "onRecordValidate":
				case "onRecordInserted":
				case "onRecordInsertError":
				case "onInsertInitialize":
				case "onRecordUpdated":
				case "onRecordUpdateError":
				case "onDependentListLoaded":
				case "onRecordSelected":
				case "onFieldValueChanged":
				case "onUniqueConstraintViolated":
				case "onValidationFailed":
					this.editControl.eventHandlers[h] = this.parentControl.eventHandlers[h];
					break;
			}
		}
		
		for (var i=0; i < this.parentControl.columns.length; i++)
			if (this.parentControl.columns[i].edit)
			{
				var c = this.parentControl.columns[i]
				var ec = {};
				for (var p in c)
				{	
					switch(p)
					{
						case "display":
						case "style":
							break;
						default:	
  							if (p.match("edit[A-Z]"))
								continue;
							ec[p] = c[p];
							break;
					}
				}
				for (var p in c)
				{	
					if (!p.match("edit[A-Z]") )
						continue;
						
					switch(p)
					{
						case "editControlType":
						case "editControlProperties":
							ec[p] = c[p];
							break;
						default:	
							np = p.substr(4,1).toLowerCase() + p.substring(5);
							
							if (c[p].toString() != "")
								ec[np] = c[p];

							break;
					}						
				}
				this.editControl.columns.push(ec);
			}
			
		this.position = null;
		this.parentControl.fireEvent("onBeforeEditInitialized", {editControl : this.editControl});	
		this.editControl.bind("onInitialized", this.createDelegate(this.editControlInitialized));	
		this.editControl.bind("onRecordUpdated", this.createDelegate(this.editApplied));	
		this.editControl.bind("onRecordInserted", this.createDelegate(this.editApplied));	
		this.editControl.bind("onImageLoaded", this.createDelegate(this.setTitleWidth));	
		this.editControl.bind("onRecordSelected", this.createDelegate(this.recordSelected));	
		
		if (window.tinyMCE)
			this.open();
			
		this.parentControl.addLinkedControl(this.editControl,true);
		
		//4.4
		if (this.parentControl.selectedRows().length > 0 && this.mode == "update")
			this.editControl.primaryKey = this.parentControl.rowPrimaryKey();
		this.editControl.parentFilterParams = {};
		this.editControl.initialize();
	},	
	
	checkDialogSize : function() {
		this.adjustScrollPanel("edit-dialog-panel",this.parentControl.editDialogHeight);
	},		
	
	editControlInitialized: function () {
	    this.editControl.$("auditPanel").appendTo(this.dialog.find(".edit-dialog-audit-panel"));

		if (window.tinyMCE)
		{
			this.initialiseRecord();	
			this.sizeDialogToContent();
		}
		else
			this.open();
			
		var args = {editControl : this.editControl, editDialog : this};
		this.parentControl.fireEvent("onEditDialogInitialized", args);
	},
	
	open : function() {
		if (this.isOpen())
		{	
			this.initialiseRecord();
			return;
		}
		
		this._super();
		
		if (this.editControl.initialized)
		{
			this.toggleEditors();
			this.initialiseRecord();
		}
		
		this.firstOpen = false;
	},
	
	dialogClosed : function() {
		this._super();
		this.toggleEditors();
	},	
	
	toggleEditors : function() {
		if (!window.tinyMCE)
			return;
		var editors = this.editControl.htmlEditors;
		for (var i=0; i< editors.length; i++)
			this.toggleEditor(editors[i]);
	},		

	toggleEditor : function(id) {
		var cmd = "";
		if (!tinyMCE.get(id))
			cmd = "Add";
		else
			cmd = "Remove";
			
		tinyMCE.execCommand("mce" + cmd + "Control", false, id);
	},	
	
	initialiseRecord : function() {
		if (this.mode == "update")
			//4.4 
			if (this.firstOpen)
				return;
			else
				this.selectRecord(this.parentControl,this.parentControl.selectedRows()[0]);	
		else
			this.insertRecord();	
	},	
	
	recordSelected : function() {
		if (this.editControl.audit)
			this.setTitleWidth();
			
		var row = this.parentControl.selectedRows()[0];
		this.dialog.find(".next-button").prop("disabled",(this.$(row).nextAll(".data-row").length == 0));
		this.dialog.find(".prev-button").prop("disabled",(this.$(row).prevAll(".data-row").length == 0));
	},		
	
	selectRecord : function(control, row) {
		if (!this.isOpen())
			return;
		this.editControl.selectRecord( this.parentControl.rowPrimaryKey() );	
	},	
	
	insertRecord : function() {
		this.editControl.initializeInsert();	
		if (this.editControl.audit)
			this.setTitleWidth();
	},	
	
	checkSpelling : function() {
		this.editControl.checkSpelling();	
	},		
	
	cancel : function() {
		this.editControl.skipUnappliedChangesCheck = true;
		if (this.editControl.mode == "insert")
			this.close();
		else
			this.editControl.cancel();	
		
		//if (this.parentControl.selectedRows().length > 0)
		//	this.selectRecord(this.parentControl,this.parentControl.selectedRows()[0]);	
	},	
	
	disableInsert : function() {
		if (this.editControl.mode == "insert")
		    this.editControl.disable();
		else
		    this.cancel();
	},		
	
	nextRecord : function(event) {
		this.parentControl.navigateRow("next");
	},
	
	prevRecord : function(event) {
		this.parentControl.navigateRow("prev");
	},	
	
	editApplied : function(editControl) {
		if (!this.closeOnApply)
			return;
		this.cancel();	
		this.close();
	},		

	apply : function(event) {
		this.editControl.apply();	
	}
});
