var ConfigDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"config");
		this.html = null;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));

		this.open();
	},
	
	open : function() {
		this.dialog.dialog("open");
		this.dialog.find(".page-size-set").val( this.parentControl.pageSize);
		this.dialog.find(".output-page-size-set").val( this.parentControl.outputPageSize);
		this.dialog.find(".output-current-page-only-set")[0].checked = this.parentControl.outputCurrentPage;
		this.dialog.find(".custom-save-set")[0].checked = this.parentControl.customSave;
	},	
		
	apply : function(event) {
		this.parentControl.pageSize = this.dialog.find(".page-size-set").val();
		this.parentControl.outputPageSize = this.dialog.find(".output-page-size-set").val();
		this.parentControl.outputCurrentPage = this.dialog.find(".output-current-page-only-set")[0].checked;
		this.parentControl.customSave = this.dialog.find(".custom-save-set")[0].checked;
		this.parentControl.loadData();
		this.close();
	}	
});