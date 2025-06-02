var ColumnSortDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"column-sort");
		this.ac = null;
		this.sc = null;
		this.selectionTable = null;
		this.deleteImage = null;
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
		this.selectionTable = this.dialog.find(".column-sort-selection");
		this.deleteImage = this.dialog.find(".delete-sort-selection-image");

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
			if (!c.display || c.dataType == "Byte[]" )
				continue;
			var opt = jQuery(document.createElement("option"));
			opt.text(c.label);				
			opt.val(c.columnExpression);				
				
			this.ac.append(opt);	
		}
		for (var i=0; i<this.selectionTable[0].rows.length; i++)
		{
			var row = jQuery(this.selectionTable[0].rows[i]);
			
			var opt = this.ac.children("[value=\"" + row.attr("columnExpression") + "\"]")
			if (opt.length == 1)
			{
				opt[0].selected = true;
				this.ac.children(":selected").remove().appendTo(this.sc); 	
			}
			else
				row.attr("delete","true");
		}	
		
		this.selectionTable.find("tr[delete=true]").remove();
		
		if (this.selectionTable[0].rows.length == 0)
			this.assignOrderBy();
		
		window.setTimeout(this.createDelegate(this.sizeDialogToContent),1);
	},
	
	shuffleUp : function(event) {
		this.sc.children(":selected").each(function(){$(this).insertBefore($(this).prev());});		
		this.buildSelectionTable();
	},
	
	shuffleDown : function(event) {
		this.sc.children(":selected").reverse().each(function(){$(this).insertAfter($(this).next());});		
		this.buildSelectionTable();
	},
	
	addToAvailable : function(event) {
		this.sc.children(":selected").remove().appendTo(this.ac); 	
		this.buildSelectionTable();
	},	
	
	addToSelected : function(event) {
		this.ac.children(":selected").remove().appendTo(this.sc); 	
		this.buildSelectionTable();
	},	
	
	assignOrderBy : function() {
		var a = this.parentControl.orderBy.split(",");
		
		for (var i=0; i<a.length; i++)
		{
			var s = "asc";
			var p = a[i].split(" ");
			var c = p[0];
			if (p.length > 1)
				s = p[1];

			var pattern = "";
			
			if (isNaN(c))
				pattern = "[value=\"" + c + "\"]";
			else
				pattern = ":nth-child(" + c +")";
				
			var opt = this.ac.children(pattern);
				
			if (opt.length == 1)
			{
				opt[0].selected = true;
				this.ac.children(":selected").remove().appendTo(this.sc); 	
				this.sc.children(":last").attr("sequence",s);
			}
		}
		this.buildSelectionTable();
	},		
	
	buildSelectionTable : function() {
		this.selectionTable.empty();
		for (var i=0; i <this.sc.children().length; i++)
		{
			var row = this.selectionTable[0].insertRow(-1);
			var cell = row.insertCell(-1);
			jQuery(cell).text( (i+1).toString() + ".");
			
			cell = row.insertCell(-1);
			jQuery(cell).text(this.sc.children()[i].text).css("white-space","nowrap");
			jQuery(row).attr("columnExpression", this.sc.children()[i].value);
			
			cell = row.insertCell(-1);
			var select = jQuery(document.createElement("select"));
			select.bind("change", this.createDelegate(this.saveSequence))
			this.loadCombo( select, [{val : "asc", text : "Ascending"},{val : "desc", text : "Descending"}] );
			jQuery(cell).append(select);
			
			var seq = this.sc.children()[i].getAttribute("sequence");
			
			if (seq)
				select.val(seq);
			
			cell = row.insertCell(-1);
			jQuery(cell).append( this.deleteImage.clone().show().bind("click", this.createDelegate(this.remove)));
		}
		
		this.sizeDialogToContent();
	},	
	
	saveSequence : function(event) {
		var idx = jQuery(event.target).parents("tr:first").get(0).rowIndex;
		this.sc.children()[idx].setAttribute("sequence",event.target.value);
	},		
	
	remove : function(event) {
		var idx = jQuery(event.target).parents("tr:first").get(0).rowIndex;
		this.sc.children(":selected").each(function(idx){this.selected = false});
		this.sc.children()[idx].selected = true;
		this.addToAvailable();
	},	
	
	apply : function(event) {
		if ( this.sc.children().length == 0)
			return;
			
		var a = [];
			
		for (var i=0; i <this.sc.children().length; i++)
		{
			var o = jQuery(this.sc.children()[i]);
			var s = "";
			if ( this.parentControl instanceof DbNetGrid)
				s = (this.parentControl.columnIndex(o.val())+1).toString();
			else
				s = o.val();

			s += (this.selectionTable.find("select:eq(" + i.toString() + ")").val() == "desc") ? " desc" : " asc";

			a.push(s);
		}
			
		this.parentControl.orderBy = a.join(",");
		
		if ( this.parentControl instanceof DbNetGrid)
			this.parentControl.loadData();
		else
			this.parentControl.getRecordSet();
				
		this.close();
	}
	
});
