var AuditDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"audit");
		this.columnKey = null;
		this.primaryKey = null;
		this.tableName = null;
	},
	
	open : function() {
		this.loadHistory();
	},		
		
	loadHistory : function() {
		var data = {};
		
		this.label = "";
		
		if (this.columnKey) {
			data.columnKey = this.columnKey;
			
			var c = this.parentControl.columnFromKey(this.columnKey);
			if (c)
				this.label = c.label;
		}
		this.assignLabel();
		
		data.primaryKey = this.primaryKey;
		data.tableName = this.tableName;
	
		this.parentControl.callServer("audit-history", { method : this.loadHistoryCallback, context : this}, data);
	},	
		
	loadHistoryCallback : function(response) {
		var panel = this.dialog.find(".audit-dialog-panel");
		panel.html(response.html);
		this.dialog.dialog("open");
		this.setPanelHeight(panel);
		this.setTitleWidth();	
	},	
	
	build : function(response) {
		this._super(response);
	},
	
	configure : function() {
		this._super();

		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.open();
	}
});
