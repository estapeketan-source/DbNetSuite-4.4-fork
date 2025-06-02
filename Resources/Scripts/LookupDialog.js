var LookupDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"lookup");

		this.targetElement = null;
		this.targetElementType = "";
		this.columnKey = null;
		this.selectionMode = "single";
		this.dialogType = "single";
	},
	
	open : function() {
		if ( this.targetElementType == "TextBoxSearchLookup" )
			this.dialog.find(".search-row").show();
		else
			this.dialog.find(".search-row").hide();
			
		var c = this.parentControl.columnFromKey(this.columnKey);
		
		if (c)
		{
			this.label = c.label;
			this.assignLabel();
		}
		
		this.dialog.find(".search-token-input").val("");
		this.loadOptions();
	},		
		
	loadOptions : function() {
		var data = {columnKey : this.columnKey};
		data.targetElementType = this.targetElementType;
		data.searchToken = this.dialog.find(".search-token-input").val();
		data.selectionMode = this.selectionMode;
		data.dialogType = this.dialogType;
		data.params = {};
		
		if ( typeof this.targetElement != "function" )
			this.parentControl.addParentParameter(this.targetElement, data);
		else {
			data.params = this.clone(this.parentControl.parentFilterParams);
			this.parentControl.fireEvent("onBeforeBulkInsertLookup",data);
		}
		
		this.parentControl.callServer("lookup-options", { method : this.loadOptionsCallback, context : this}, data);
	},	
		
	loadOptionsCallback : function(response) {
		this.dialog.find(".list-box-container").html(response.html);
		this.dialog.find(".select-button")[0].disabled = (this.dialog.find(".lookup-options")[0].options.length == 0);
		this.dialog.dialog("open");
		this.assignCurrentValue();

		if (this.parentControl.multiValueLookupSelectStyle.toString().toLowerCase() == "checkbox")
		    this.convertToCheckboxes();
	},

	convertToCheckboxes : function(response) {
	    this.dialog.find("select[multiple]").each(function () {  //Loop over all multi-select boxes
	        
	        jQuery(this).wrap("<div class='selectWrap'></div>"); //wrap a div around it

	        jQuery(this).parents("div:first").css("background-color", "white").css("overflow-y", "auto").height(jQuery(this).height()).width(jQuery(this).width());

	        jQuery(this).children("option").each(function () {  //loop over the option tags
	            var curVal = jQuery(this).attr("value");  //get value
	            var curText = jQuery(this).text(); //get display name

	            if ( jQuery(this).is(":selected")) {  //if its selected
	                var curStatus = "checked='checked'";  //prep the checked for the insert
	            }
	            else {
	                var curStatus = "";  //not checked
	            }
	            if (typeof curVal != "undefined") {  //if the current value isn't empty
	                var curCheckInsert = "<div class='" + ( jQuery(this).is(":selected") ? "selected" : "") + "'><input type='checkbox' id='" + curVal + "Check' " + curStatus + " value='" + curVal + "' />" +
                        "<label for='" + curVal + "Check'>" + curText + "</label></div>";
	            }

	            jQuery(this).parents("div:first").append(curCheckInsert); //do the actual insert

	        });
	        jQuery(this).hide();  //hide the original select box

	    });

	    this.dialog.find("div.selectWrap input[type=checkbox]").click(function () {  //add click handler to the new checkboxes
	        var checkVal = jQuery(this).attr("value");  //Get the value to match with the selectbox
	        var checked = jQuery(this).is(":checked");
	        if (checked) {
	            jQuery(this).parent().addClass("selected");
	        }
	        else {
	            jQuery(this).parent().removeClass("selected");
	        }
	        jQuery(this).parent().siblings("select").children("option[value=" + checkVal + "]").prop("selected", checked);
	    });

	},


	
	build : function(response) {
		this._super(response);
	},
	
	configure : function() {
		this._super();

		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".select-button").bind("click",this.createDelegate(this.select));
		this.dialog.find(".search-button").bind("click",this.createDelegate(this.search));
		this.dialog.find(".search-row").hide();
		
		this.open();
	},
	
	select : function(event) {
		if ( typeof this.targetElement == "function")
		{
			this.targetElement.apply(this, [this.selectedValues()]);
			this.close();
			return;
		}	
	
		switch( this.targetElementType )
		{
			case "TextBoxLookup":
			case "TextBoxSearchLookup":
				if ( this.targetElement.attr("fieldValue") != null )
				{
					var lookupList = this.dialog.find(".lookup-options")[0];
					for (var i = 0; i < lookupList.options.length; i++)
						if(lookupList.options[i].selected)
						{
							this.targetElement.val(lookupList.options[i].text);
							this.targetElement.attr("fieldValue", lookupList.options[i].value);
							this.targetElement.attr("displayValue", lookupList.options[i].text);
						}
				}
				else
				{
					this.targetElement.val(this.selectedValues());
					this.targetElement.attr("lookupValue", this.selectedValues(true));
                }
				break;
			default:
				this.targetElement.val(this.selectedValues());
				this.targetElement.attr("lookupValue", this.selectedValues(true));
				break;
		}
		
		if(this.targetElement[0].onchange != null)
			this.targetElement[0].onchange();
		
		if(this.targetElement[0].onkeyup != null)
			this.targetElement[0].onkeyup();
		
		this.targetElement.trigger("change");
		this.targetElement.trigger("keyup");
		
		this.close();
	},
	
	selectedValues : function(lookupValue) {
		var lookupList = this.dialog.find(".lookup-options")[0];
		var selectedItems = [];

		var selected = this.dialog.find(".lookup-options").children(":selected").length;

		var escape = '\\,';//selected > 1 ? '\\,' : ','
		
		for (var i = 0; i < lookupList.options.length; i++)
			if(lookupList.options[i].selected)
			    selectedItems.push(lookupList.options[i].value.replace(/,/g, escape));
						
		return selectedItems.join(",");	
	},	
		
	search : function(event) {
		this.loadOptions();
	},			
	

	assignCurrentValue: function() {
		if ( typeof this.targetElement == "function")
			return;
			
		var value = this.targetElement.attr("lookupValue") || this.targetElement.val();
		switch( this.targetElementType )
		{
			case "TextBoxLookup":
			case "TextBoxSearchLookup":
				if (this.targetElement.attr("fieldValue") != null)
					value = this.targetElement.attr("fieldValue");
				break;
		}
		var lookupList = this.dialog.find(".lookup-options")[0];

		if (value == "")
		    return;

		if ($(lookupList).attr("multiple") == "multiple") {
		    var a = value.toString().replace(/\\,/g, '@@@').split(",");

		    for (var ai = 0; ai < a.length; ai++) {
		        for (var i = 0; i < lookupList.options.length; i++)
		            if (lookupList.options[i].value == jQuery.trim(a[ai]).replace(/@@@/g, ','))
		                lookupList.options[i].selected = true;
		    }
		}
		else {
		    lookupList.value = value.toString();
		}
		
		if ( lookupList.options.length > 0 )
			if ( lookupList.selectedIndex == -1 )
				lookupList.options[0].selected = true;
					
		
//		if( this.targetElementType == "TextBoxSearchLookup" )
//			this.$get("searchLookupToken").focus();
	
	}
});
