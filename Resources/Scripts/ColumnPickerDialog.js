var ColumnPickerDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"column-picker");
		this.ac = null;
		this.sc = null;
		this.columns = [];
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));

		this.ac = this.dialog.find(".available-columns");
		this.sc = this.dialog.find(".selected-columns");
		this.dialog.find(".up-toolbutton").bind("click",this.createDelegate(this.shuffleUp));
		this.dialog.find(".down-toolbutton").bind("click",this.createDelegate(this.shuffleDown));

		this.dialog.find(".add-toolbutton").bind("click",this.createDelegate(this.addToSelected));
		this.dialog.find(".remove-toolbutton").bind("click",this.createDelegate(this.addToAvailable));
		this.bind("onOpen", this.createDelegate(this.loadCombos));
		
		this.open();
	},

	loadCombos : function() {
		this.ac.empty();
		this.sc.empty();
		
		for (var i=0; i<this.parentControl.columns.length; i++)
		{
		    var c = this.parentControl.columns[i];

		    if (!c.columnPicker)
		        continue;

			var combo;
			var opt = jQuery(document.createElement("option"));
			if (c.display)
				combo = this.sc;
			else
				combo = this.ac;
			
			opt.text(c.label);				
			opt.val(c.columnExpression);				
				
			combo.append(opt);	
		}
	},
	
	shuffleUp : function(event) {
		this.sc.children(":selected").each(function(){jQuery(this).insertBefore(jQuery(this).prev());});		
	},
	
	shuffleDown : function(event) {
		this.sc.children(":selected").reverse().each(function(){jQuery(this).insertAfter(jQuery(this).next());});		
	},
	
	addToAvailable : function(event) {
		this.sc.children(":selected").remove().appendTo(this.ac); 	
	},	
	
	addToSelected : function(event) {
		this.ac.children(":selected").remove().appendTo(this.sc); 	
	},	

	apply : function(event) {
		if ( this.sc.children().length == 0)
			return;
			
		var args = {cancel:false, message : ""}; 
		args.selectedColumns = this.sc.children();
		args.unselectedColumns = this.ac.children();
		args.dialog = this;
		
		this.parentControl.fireEvent("onColumnsSelected",args);

		if (args.cancel)
		{
			if (args.message != "")
				this.showMessage(args.message);
			return;
		}		
	
		this.columns = [];
		this.sc.children().each(this.createDelegate(this.addDisplayColumn));	
		this.ac.children().each(this.createDelegate(this.addNonDisplayColumn));

		for (var i = 0; i < this.parentControl.columns.length; i++) {
		    var c = this.parentControl.columns[i];
		    if (c.columnPicker == false) {
		        this.columns.push(c);
		    }
		}

		this.parentControl.reconfigureColumns(this.columns);
		this.close();
	},

	addDisplayColumn : function(idx) {
		this.addColumn(idx,this.sc,true); 
	},
	
	addNonDisplayColumn : function(idx) {
		this.addColumn(idx,this.ac,false); 
	},
	
	addColumn : function(idx, combo, display) {
		var ce = combo.children()[idx].value;
	
		for ( var i=0; i < this.parentControl.columns.length; i++)
		{
			var c = this.parentControl.columns[i];
			if (c.columnExpression == ce)
			{
				c.display = display;
				this.columns.push(c);
				break;
			}
		}
	}
	
});
