var FileSearchDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"file_search");
		this.helpWindow = null;
	},
	
	configure : function() {
		this._super();
		
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".help-button").bind("click", this.createDelegate(this.showHelp));
	
		this.dialog.find("[dataType='DateTime']").each( this.createDelegate(this.addSearchDatePicker) ) ;
		this.dialog.find("[buttonType='calendar']").bind("click", this.createDelegate(this.showCalendar));
		
		this.dialog.find(".search-criteria-operator" ).bind("change",this.createDelegate( this.searchOperatorChanged));

		this.open();
	},
	
	showCalendar : function(event) {
		var tb = jQuery(event.target).parents("td:first").prev(":first").find("input");
		tb.datepicker("show");
	},	
	
	searchOperatorChanged : function(e) {
		if (e.target.id.indexOf("_search_operator") > -1 )
		{
			var state = (e.target.value == "between") ? "visible" : "hidden";
			this.$(e.target).parents("tr:first").contents().find(".between").css("visibility", state); 
		}
		if ( jQuery(e.target).hasClass("size-unit") )
		{
			var idx = e.target.selectedIndex;
			this.$(e.target).parents("tr:first").contents().find(".size-unit").attr("selectedIndex", idx); 
		}		
    },
	
	showHelp : function(event)	{
		if (this.helpWindow)
			if (!this.helpWindow.closed)
			{
				this.helpWindow.focus();
				return;
			}	
		this.helpWindow = window.open( this.$(event.target).attr("helpUrl"), null, "scrollbars=yes,width=500,height=500,toolbar=no" );		
	},	
	
    apply : function() {
		this.searchCriteria = new Object();
        this.dialog.find(".search-criteria" ).each( this.createDelegate(this.saveCriteria) ) ;
        var data = {searchCriteria : this.searchCriteria};
       
		this.parentControl.callServer("validate-search-params", { method : this.applyCallback, context : this}, data);
	},
	
    addSearchDatePicker : function(idx,e) {
		this.addDatePicker(jQuery(e));
	},	
	
    saveCriteria : function(idx,e) {
		var v;
		if ( e.type != "checkbox" )
	        v = (e.value.toString().length == 0) ? "" : e.value.toString();
		else
			v = e.checked.toString();
			
	    this.searchCriteria[e.id] = v;    
	},	
		
	applyCallback : function(response) {
		if (response.message != "")
		{
			var e = this.$(response.elementId );
			e.addClass("highlight"); 
			e.focus();
			window.setTimeout(this.createDelegate(this.clearHighlight),3000);
			this.showMessage(response.message);
		}
		else
		{
			this.parentControl.runSearch(this.searchCriteria);
			if (this.closeOnApply)
				this.close();		
		}
	},
	
	clearHighlight : function()	{
		this.dialog.find(".search-criteria" ).removeClass("highlight");
	}	
	
});
