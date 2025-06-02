var AdvancedSearchDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"advanced-search");
		this.searchCriteriaTable = null;
		this.searchDialogs = [];
		this.searchFilter = [];
		this.searchCriteriaStore = [];
		this.paramSuffix = 0;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".add-button").bind("click",this.createDelegate(this.addSearch));
		this.dialog.find(".standard-search-link").bind("click",this.createDelegate(this.standardSearch));
		this.dialog.find(".search-filter-join").bind("change",this.createDelegate(this.updateJoinOperator));
		this.dialog.find(".clear-link").bind("click",this.createDelegate(this.removeSearch));
		this.dialog.find(".edit-link").bind("click",this.createDelegate(this.updateSearch));
		this.dialog.find(".filter-text").html("&nbsp;")
		this.searchCriteriaTable = this.dialog.find(".search-criteria-table")[0];
		
		this.bind("onOpen",this.createDelegate(this.deferredRestore));

		this.addSearchDialog();

		this.open();
	},
		
	addSearch : function(event) {
		this.addSearchCriteriaTableRow();
		this.openStandardSearchDialog(this.searchCriteriaTable.rows.length-1);
	},	
	
	addSearchCriteriaTableRow : function() {
		var row = jQuery(this.searchCriteriaTable.rows[0]).clone(true);
		row.find(".filter-text").html("&nbsp;")
		jQuery(this.searchCriteriaTable).append(row);
		this.updateJoinOperator();
	},	

	updateSearch : function(event) {
		var index = parseInt(jQuery(event.target).parents("tr:first").prop("rowIndex"));
		this.openStandardSearchDialog(index);
	},		
	
	removeSearch : function(event) {
		if (this.searchCriteriaTable.rows.length == 1)
			return;
		
		var index = parseInt(jQuery(event.target).parents("tr:first").prop("rowIndex"));
		
		this.removeSearchInstance(index);
		this.updateJoinOperator();
	},				
	
	removeSearchInstance : function(index) {
		this.searchDialogs[index].close();
		this.searchDialogs.splice(index,1);
		this.searchCriteriaTable.deleteRow(index);
	},				
			
	standardSearch : function(event) {
		this.close();
		this.parentControl.openStandardSearchDialog();
	},		
	
	updateJoinOperator : function() {
		var joinOp = this.dialog.find(".search-filter-join").val();
		jQuery(this.searchCriteriaTable).find(".join-operator").text(joinOp);
		jQuery(this.searchCriteriaTable).find(".join-operator:first").text("");
		this.sizeDialogToContent();
	},		
	
	applySearch : function(dialog) {
		var row = this.searchCriteriaTable.rows[dialog.advancedSearchDialogIndex];
		var html = dialog.filterText();
		if (html == "")
			html = "&nbsp;";
		jQuery(row).find(".filter-text").html(html);
	},	
		
	openStandardSearchDialog : function(index) {
		var sd = this.getSearchDialog(index);
		sd.advancedSearchDialogIndex = index;
		
		this.parentControl.openDialog(sd, "search-dialog");
	},	
	
	getSearchDialog : function(index) {
		if (index < this.searchDialogs.length)
			return this.searchDialogs[index];
		else
			return this.addSearchDialog();
	},	
	
	addSearchDialog : function() {
		var	sd = new SearchDialog( this );
		sd.paramSuffix = this.paramSuffix.toString();
		sd.data.advancedSearchDialog = true;
		this.paramSuffix++;
		this.searchDialogs.push(sd);
		return sd;
	},
	
	apply : function(event) {
		this.searchFilter = [];
		this.searchCriteriaStore = [];
		
		for (var i=0; i < this.searchDialogs.length; i++)
		{
			var d = this.searchDialogs[i];
			this.searchFilter.push( d.searchFilter );
			
			var row = this.searchCriteriaTable.rows[i];
			d.searchCriteriaStore["filterText"] = jQuery(row).find(".filter-text").html();
			
			this.searchCriteriaStore.push( d.searchCriteriaStore );
			
			if (this.closeOnApply)
				d.close();		
		}	
			
		this.parentControl.advancedSearchFilterJoin = this.dialog.find("#searchFilterJoin").val();
		this.parentControl.applySearch(this);
		
		if (this.closeOnApply)
			this.close();		

	},
	
	filterText : function() {
		var a = [];
		
		for (var i=0; i < this.searchCriteriaStore.length; i++)
		{
			var sd = this.parentControl.searchDialog;
			sd.searchCriteriaStore = this.searchCriteriaStore[i];
			a.push( sd.filterText() );	
		}
			
		var joinText = ") <br/>" + this.wrap(this.parentControl.advancedSearchFilterJoin,"advanced-join-op") + "<br/> ("
	
		if (a.join(joinText).length == 0)
		    return '';
        else
	    	return "(" + a.join( joinText ) + ")";
	},
	
	wrap : function(v, cl) {
		if (!cl)
			cl = "value";
		return "<span class='filter-text-" + cl + "'>" + v + "</span>";
	},
	
	restore : function(searchCriteria) {
		this.searchCriteriaStore = searchCriteria;
		this.searchFilter = [];
		for (var i=0; i < this.searchCriteriaStore.length; i++)
		{
			var sd = this.parentControl.searchDialog;
			sd.searchCriteriaStore = this.searchCriteriaStore[i];
			sd.paramSuffix = i.toString();
			sd.buildFilter();
			this.searchFilter.push( sd.searchFilter );
		}		
		
		if (!this.isOpen())
			return;
		
		this.restore2();
	},
	
	deferredRestore : function() {
		window.setTimeout( this.createDelegate(this.restore2), 100);

		if ( this.searchCriteriaStore.length == 0)
			if ( jQuery(this.searchCriteriaTable.rows[0]).find(".filter-text").html() == "&nbsp;")
				this.openStandardSearchDialog(0);
	},
	
	restore2 : function() {
		if ( this.searchCriteriaStore.length == 0)
			return;
	
		for (var i=0; i<this.searchCriteriaStore.length; i++)
		{
			if (i > this.searchCriteriaTable.rows.length-1)
				this.addSearchCriteriaTableRow();
			var sd = this.getSearchDialog(i);
			var sc = this.searchCriteriaStore[i];
			sd.restore(sc);
			
			var row = this.searchCriteriaTable.rows[i];
			jQuery(row).find(".filter-text").html(sc["filterText"]);
		}
		
		for (var i=this.searchCriteriaTable.rows.length-1; i >= this.searchCriteriaStore.length; i--)
			this.removeSearchInstance(i);
		
		this.dialog.find("#searchFilterJoin").val(this.parentControl.advancedSearchFilterJoin);
		this.sizeDialogToContent();
		this.searchCriteriaStore = [];		
	},		
	
	dialogClosed : function() {
		this._super();
		
		for (var i=0; i < this.searchDialogs.length; i++)
			this.searchDialogs[i].close();	
	}	
});
