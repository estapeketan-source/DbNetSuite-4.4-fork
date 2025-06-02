var SearchPanel = SearchDialog.extend({
	init: function(parentControl){
		this._super(parentControl,"searchPanel");
	},
	
	build : function(response) {
	},
	
	open : function() {
	},	
		
	configure : function() {
		this.dialog = this.dialogContainer.children();
		this.dialog.find(".search-options-row").hide();
		this.component = this.parentControl;	
		var args = {};
		args.searchPanel = this;
		this.parentControl.fireEvent("onSearchPanelInitialized", args)
		this._super();
	}
});
