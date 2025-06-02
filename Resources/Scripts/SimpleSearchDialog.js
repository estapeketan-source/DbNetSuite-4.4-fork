var SimpleSearchDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"simple-search");
		this.searchCriteriaStore = "";
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".standard-search-link").bind("click",this.createDelegate(this.standardSearch));

		this.bind("onOpen",this.createDelegate(this.restore));
		
		this.open();
	},
	
	standardSearch : function(event) {
		this.close();
		this.parentControl.openStandardSearchDialog();
	},	
	
	restore : function(searchCriteria) {
		if (typeof searchCriteria == "string")
			this.searchCriteriaStore = searchCriteria;
			
		if (!this.isOpen() || this.searchCriteriaStore == "")
			return;
			
		this.dialog.find(".simple-search-token").val(this.searchCriteriaStore);
		this.searchCriteriaStore = "";	
	},		
	
	filterText : function() {
		return this.searchCriteriaStore;
	},		
	
	apply : function(event) {
		this.parentControl.runSimpleSearch( this.dialog.find(".simple-search-token").val() );
		if (this.closeOnApply)
			this.close();		
	}	
});
