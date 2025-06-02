var SearchDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"search");
		this.searchFilterSql = [];
		this.searchFilterParams = {};
		this.searchFilterText = [];
		this.searchCriteriaStore = {};
		this.paramSuffix = "";
		this.advancedSearchDialogIndex = 0;
		this.searchFilter = { sql: [], params: {}, join: "" };
		this.autoOpen = true;
	},
	
	configure : function() {
		if (!(this instanceof SearchPanel)) {
			this._super();
			this.bind("onOpen",this.createDelegate(this.checkDialogSize));
		}
		
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".clear-button").bind("click",this.createDelegate(this.clear));
		this.dialog.find(".simple-search-link").bind("click",this.createDelegate(this.simpleSearch));
		this.dialog.find(".advanced-search-link").bind("click",this.createDelegate(this.advancedSearch));

		this.dialog.find(".input-control[dataType=DateTime]").attr("suffix",this.paramSuffix).each(function(n){this.id = this.id + "_" + this.getAttribute("suffix")});		
		
	    //this.dialog.find(".input-control[dataType=DateTime]").datepicker({showOn : "none", onSelect : function(text,inst) {jQuery(this).trigger("keyup");return true;}});

		var ctrl = this

		this.dialog.find(".input-control[dataType=DateTime]").each(function () {
		    var e = jQuery(this);
		    var columnName = e.closest("tr.search-row").attr("columnName");

		    var parentControl = ctrl.parentControl;
		    if (!parentControl.columns) {
		        parentControl = parentControl.parentControl;
		    }
		    var col = parentControl.getColumn(columnName);
		    col = jQuery.extend(true, {}, col);
		    col.format = 'd';
		    if (col.searchFormat != '') {
		        col.format = col.searchFormat;
		    }
		    ctrl.addDatePicker(e, col);
		})
		this.dialog.find(".input-control").bind("keyup", this.createDelegate(this.searchInputOnKeyUp));
		this.dialog.find(".input-control").bind("paste", this.createDelegate(this.onPaste));
		this.dialog.find(".search-operator").bind("change", this.createDelegate(this.searchOperatorChange));
		this.dialog.find(".lookup-button").bind("click", this.createDelegate(this.showLookup)).css("padding","1px");
		this.dialog.find(".calendar-button").bind("click", this.createDelegate(this.showCalendar)).css("padding","1px");
		
		this.bind("onOpen",this.createDelegate(this.deferredRestore));
		
		if (this.autoOpen) 
		    this.open();
	},
	
	addInputControlButton : function(columnName) {
		var row = this.getSearchRow(columnName);
		if (!row)
			return;
		
		var c = row.find("table").rows[0].insertCell(-1);
		var b = document.createElement("button");
		c.appendChild(b);
        b.style.padding = "0px"
        b.setAttribute("type","button");
        return b;
	},	
	
	getSearchRow : function(columnName) {
		var c = this.parentControl.getColumn(columnName);
		if (c == null)
			return null;
		else
			return this.dialog.find("#searchRow" + c.columnKey);
	},	
	
	checkDialogSize : function() {
		this.adjustScrollPanel("search-dialog-scroll-container",this.parentControl.searchDialogHeight);
	},
		
	simpleSearch : function(event) {
		this.close();
		this.component.openSimpleSearchDialog();
	},		
	
	advancedSearch : function(event) {
		this.storeCriteria();
		this.close();
		this.component.openAdvancedSearchDialog(this.searchCriteriaStore);
	},		
	
	showCalendar : function(event) {
		var tb = this.$(this.parentElement(event.target,"button")).parent("td").prev().children(":first");
		tb.datepicker("show");
	},	
	
	showLookup : function(event) {
		var row = this.$(event.target).parents("tr[columnIndex]")[0];
		var tb = this.$(this.parentElement(event.target,"button")).parent("td").prev().children(":first");
		
		this.component.openLookupDialog(row.getAttribute("columnKey"),tb);
	},					

	apply : function(event) {
		this.storeCriteria();
		this.buildFilter();
		
		var data = {searchFilterParams : this.searchFilterParams};
		this.component.callServer("validate-search-params", { method : this.validateCallback, context : this}, data);
	},
	
	buildFilter : function() {
		this.searchFilterSql = [];
		this.searchFilterParams = {};
	
		for (var c in this.searchCriteriaStore )
			this.buildFilterRow( this.searchCriteriaStore[c] );

		this.assignSearchFilter();
	},		
	
	assignSearchFilter: function () {
	    this.searchFilter = {};
		this.searchFilter.sql = this.searchFilterSql;
		this.searchFilter.params = this.searchFilterParams;
		this.searchFilter.join = this.searchCriteriaStore["join"];
	},		
	
	validateCallback : function(response) {
		if(response.result)
		{
			this.assignSearchFilter();			
			this.parentControl.applySearch(this);
			
			if (this.parentControl instanceof AdvancedSearchDialog || this.closeOnApply)
				this.close();
		}
		else
		{
			this.searchFilterParams = new Object();
			this.searchFilterSql = [];
			
			var row = this.dialog.find("#searchRow" + response.columnKey);

			var op = row.find(".search-operator").val().toLowerCase();
			var selector = ".input-control:" + ((response.key.indexOf("Param0") > -1) ? "first" : "last");

			row.find(selector).addClass("field-highlight").focus(); 
			
			window.setTimeout(this.createDelegate(this.clearHighlight),3000);
						
			this.showMessage(response.message);
		}		
	},	
	
	storeCriteria : function(event) {
		this.searchCriteriaStore = {};
		var rows = this.dialog.find(".search-row");
		
		for (var i = 0; i < rows.length; i++ )
		{
			var row = jQuery(rows[i]);
			var op = row.find(".search-operator").children(":selected");

			if (op.text() == "")
				continue;
				
			var criteria = {};
			criteria.label = row.find(".search-label").text()
			criteria.op = op.val();
			criteria.opText = op.text();
			criteria.sqlOp = op.attr("operator")			
			criteria.value1 = row.find(".input-control:first").val();
			criteria.value2 = row.find(".input-control:last").val();
			criteria.columnIndex = row.attr("columnIndex");
			criteria.columnKey = row.attr("columnKey");
			criteria.lookupMode = row.attr("lookupMode");
			
			this.searchCriteriaStore[row.attr("columnName")] = criteria;	
		}
		
		this.searchCriteriaStore["join"] = this.dialog.find("#searchFilterJoin").val();
	},
	
	restore : function(searchCriteria) {
		this.searchCriteriaStore = searchCriteria;
		this.buildFilter();
		
		if (!this.isOpen())
			return;
			
		this.restore2();
	},
	
	deferredRestore : function() {
		window.setTimeout( this.createDelegate(this.restore2), 100);
	},
	
	restore2 : function() {		
		if (this.isEmptyObject(this.searchCriteriaStore))
			return;
				
		this.clear();
		for (var cn in this.searchCriteriaStore)
		{
			var c = this.searchCriteriaStore[cn];
			var row = this.dialog.find("tr[columnName='" + cn + "']")
			if (row.length == 0)
				continue;
			var so = row.find(".search-operator");
			so.val(c.op);
			this.configureSearchOperator(so);
			row.find(".input-control:first").val(c.value1);	
			row.find(".input-control:last").val(c.value2);	
		}
		
		this.dialog.find("#searchFilterJoin").val(this.searchCriteriaStore["join"]);
		this.searchCriteriaStore = {};
	},		

    filterText: function (event) {
		var data = {searchCriteriaStore : this.searchCriteriaStore};
		var response = this.component.callServer("update-search-filter-text", null, data);

		this.searchFilterText = [];
		
		for (var c in response.searchCriteriaStore )
			this.filterRowText( response.searchCriteriaStore[c] );
			
		var joinText = " " + this.wrap(response.searchCriteriaStore["join"],"join-op") + "<br/> "
	
		return this.searchFilterText.join( joinText );
	},
	
	filterRowText : function(criteria) {
		if (typeof criteria == "string")
			return;
	
		var filterText = this.wrap( criteria.label, "label" );
		var mode = "op";
		switch( criteria.op )
		{	
			case "true":
			case "false":
				filterText += " " + this.wrap( this.component.translate("EqualTo"), "op");
				mode = "value";
				break;
		}		

		filterText += " " + this.wrap( criteria.opText, mode);
		
		switch( criteria.op )
		{
			case "between":		
			case "not_between":		
				var and = this.component.translate("And");	
				filterText += " " + this.wrap(criteria.value1) + " " + and + " " + this.wrap(criteria.value2);
				break;                    
			case "is_not_null":
			case "is_null":
			case "true":
			case "false":
				break;
			default:
				filterText += " " + this.wrap(criteria.value1);
				break;
		}
		
		this.searchFilterText.push(filterText);
	},
	
	wrap : function(v, cl) {
		if (!cl)
			cl = "value";
		return "<span class='filter-text-" + cl + "'>" + v + "</span>";
	},

	buildFilterRow : function(criteria) {
		if (typeof criteria == "string")
			return;
			
		var sqlToAdd = "";
		var p1 = "searchCol" + criteria.columnKey + "Param0" + this.paramSuffix;
		var p2 = "searchCol" + criteria.columnKey + "Param1" + this.paramSuffix;
		var value1 = criteria.value1;
		var value2 = criteria.value2;
	
		switch(criteria.op)
		{
			case "contains":
			case "does_not_contain":
				value1 = "%" + value1 + "%"
				break;
			case "starts_with":
			case "does_not_start_with":
				value1 = value1 + "%"
				break;
			case "ends_with":
			case "does_not_end_with":
				value1 = "%" + value1
				break;
		}			
		
		switch(criteria.op)
		{
			case "in":						
			case "not_in":
				var params = value1.replace(/\\,/g,"_@@@_").split(",");				
				sqlToAdd = [];
				for( var j=0; j < params.length; j++ )
				{
					sqlToAdd.push(p1 + j);
					this.searchFilterParams[p1 + j] = params[j].replace(/_@@@_/g,",");
				}
				sqlToAdd = criteria.sqlOp.replace("{0}", sqlToAdd.join(", "))
				break;
			case "between":		
			case "not_between":			
				sqlToAdd = criteria.sqlOp.replace("{0}",p1).replace("{1}",p2);
				this.searchFilterParams[p1] = value1;
				this.searchFilterParams[p2] = value2;
				break;                    
			case "is_not_null":
			case "is_null":
				sqlToAdd = criteria.sqlOp;
				break;
			default:
				sqlToAdd = criteria.sqlOp.replace("{0}", p1)
				this.searchFilterParams[p1] = value1;
				break;
		}
		
		switch(criteria.lookupMode)
		{
			case "none":
			case "SearchValue":
			case "SearchText":
				this.searchFilterSql.push("{col" + criteria.columnKey + "} " + sqlToAdd );
				break;
		}
	},

	clearHighlight : function()	{
		this.dialog.find(".input-control" ).removeClass("field-highlight");
	},		
		
	clear : function(event) {
		this.dialog.find(".input-control").attr("value","");
		this.dialog.find(".search-operator").attr("value","");
		this.dialog.find(".between-info").hide()
		this.dialog.find(".input-control").width(240)
		this.dialog.find(".input-control:first").show();
	},
	
	searchOperatorChange : function (e) {
		this.configureSearchOperator(this.$(e.target));
	},
	
	configureSearchOperator : function (combo) {
		var row = combo.parents("tr:eq(0)");
	
		switch( combo.val().toLowerCase() )
		{
			case "is_not_null":
			case "is_null":
				row.find(".input-control:first").hide();
				break;
			case "true":
			case "false":
				row.find(".input-control:first").val("0");
				break;
			default:
				if (combo.attr("dataType") != "Boolean")
					row.find(".input-control:first").show();
				break;
		}

		switch( combo.val().toLowerCase() )
		{
			case "between":
			case "not_between":
				row.find(".between-info").show()
				var w = (row.find(".input-control").parent().next().children().length == 1) ? 100 : 88;
				row.find(".input-control").width(w)				
				break;
			default:
				row.find(".between-info").hide()
				row.find(".input-control").width(240)
				break;
		}

		switch (combo.val().toLowerCase()) {
		    case "in":
		    case "not_in":
		        break;
		    default:
		        row.find(".input-control").val(row.find(".input-control").val().replace(/\\/g, ''))
		        break;
		}
	},
	
	onPaste: function (e) {
	    var self = this;
	    setTimeout(function () { self.assignDefaultOperator(e.target); }, 1)
	},

	searchInputOnKeyUp : function(e) {
		this.assignDefaultOperator(e.target);
	},
	 
	assignDefaultOperator : function(e) {
		var row = this.$(e).parents("tr[columnIndex]");
		
		var filter = row.find(".search-operator");
		var input = jQuery(e);

		if( filter.val().toLowerCase().indexOf("between") > -1)
		    return;

		var hasLookup = filter.closest("tr").find(".lookup-button").length !== 0;
		var hasContains = filter.find("option[value='contains']").length !== 0;

		var escapeRe = /\\,/g;
		if (input.val() == "") {
		    filter[0].selectedIndex = 0
		}
		else {
		    if (input.val().replace(escapeRe, '').indexOf(",") == -1) {
		        if (filter.val() == '') {
		            input.val(input.val().replace(/\\/g, ''))
		            if (hasContains && hasLookup == false)
		                filter.val("contains");
		            else
                        filter.val("equal_to");
		        }
		    }
		    else if (input.val().replace(escapeRe, '').indexOf(","))
		        filter.val("in");
		}
	}
	
});
