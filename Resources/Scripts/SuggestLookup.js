var SuggestLookup = DbNetSuite.extend({
	init: function(element, editControl){
		this._super("");
		this.component = null;
		this.container = null;
		this.dbColumn = null;
		this.dbTable = null;
		this.element = null;
		this.hideContainerTimer = null;
		this.keyPauseTimer = null;
		this.maxItems = 10;
		this.minimumCharacters = 1;
		this.pause = 100;
		this.responseData = null;
		this.selectedRecord = null;
		this.selectedItem = null;
		this.sql = null;
		this.valueColumn = null;
		
		this.element = element;
		
		this.component = editControl;
				
		this.container = document.createElement("div");
		with(this.container)
		{
			className = "suggest-lookup-container";
			tabIndex = "0";
			style.position = "absolute";
			style.display = "none";
			style.zIndex = 99999;
		}
		jQuery(this.element[0].parentNode).append(this.container);

		jQuery(this.container).bind("focus", this.createDelegate( this.containerFocus));
		jQuery(this.container).bind("blur", this.createDelegate( this.containerBlur));
			
		this.element.bind("focus", this.createDelegate( this.autoSuggest));
		this.element.bind("keyup", this.createDelegate( this.keyUpEvent));
		this.element.bind("keydown", this.createDelegate( this.keyDownEvent));
		this.element.bind("click", this.createDelegate(this.elementBlur));		
//		this.element.bind("blur", this.createDelegate(this.elementBlur));		
		
	},

	keyDownEvent : function(e) {
		if(this.container.style.display == "none")
			return;
			
		switch (e.keyCode)
		{
			case 38:
				this.scroll(-1);
				break;
			case 40:
				this.scroll(1);
				break;
			case 13:
			case 9:
				this.enterSelection();
		}	
	},	

	keyUpEvent : function(e)
	{		
		if ( this.element.val() == "")
		{
			this.assignValue("","");
			return;
		}
							
		switch (e.keyCode)
		{
			case 38:
			case 40:
			case 13:
			case 9:
				break;
			default:
				window.clearTimeout(this.keyPauseTimer);
				
				this.selectedItem = null;
				this.container.scrollTop = 0;

				if(this.element.val().length >= this.minimumCharacters && this.element.val() != "")
					this.keyPauseTimer = window.setTimeout(this.createDelegate(this.getData), this.pause);
				else
					this.container.style.display = "none";
				break;
		}
	},

	getData : function()
	{
		this.component.getSuggestedItems(this)
	},
		
	getDataCallback : function(response)
	{		
		var html = response.html;	

		this.container.innerHTML = html;
		
		var rows = this.container.childNodes[0].rows;

		for(var i=0; i< rows.length; i++)
		{
			var row = jQuery(rows[i]);
			row.children().bind("click", this.createDelegate( this.mouseClick));
			row.children().bind("mouseover", this.createDelegate( this.mouseIn));
			row.children().bind("mouseout", this.createDelegate( this.mouseOut));
		}
		
		if(rows.length == 0)
		{
			this.container.style.display = "none";
			return;
		}
		
		this.container.style.display = "";
	//	this.element.show();
		var adj = 2;
		if (document.all)
		{	
			jQuery(this.container).css("left",this.element.position().left);
			jQuery(this.container).css("top",this.element.position().top + 22);
			adj += 2;
		}
		jQuery(this.container).width(this.element.width() + adj);
		jQuery(this.container).height(this.calcContainerHeight());
		this.setSelectedItem(rows[0]);
		
		if (rows.length == 1)
		{
			var cell = jQuery(rows[0]).children(":first");
			if( cell.text().toLowerCase() == this.element.val().toLowerCase())
				this.enterSelection();
		}
	},

	elementBlur : function(e)
	{	
		this.hideContainerTimer = window.setTimeout(this.createDelegate(this.hideContainer), 100);
	},
	
	hideContainer : function(e)
	{	
		if (this.container.style.display == "none")
			return;
	
		if( this.element.attr("displayValue") != null)
			this.element.val( this.element.attr("displayValue") );
			
		if (this.keyPauseTimer != null)
			window.clearTimeout(this.keyPauseTimer);

		this.container.style.display = "none";
		this.selectedItem = null;
	},

	containerFocus : function()
	{
		window.clearTimeout(this.hideContainerTimer);
	},
	
	autoSuggest : function()
	{
		if (!this.component.autoSuggest)
			return;

		if (this.element.val() == "" && this.element.attr("mandatory").toString().toLowerCase() == "true" )
		{
			this.element.attr("autosuggest","true");
			this.keyPauseTimer = window.setTimeout(this.createDelegate(this.getData), 2000);	
		}	
	},	

	containerBlur : function()
	{
		this.hideContainer();
	},

	calcContainerHeight : function()
	{
		if (document.all)
		{
			var borderWidth = (parseInt(this.container.currentStyle.borderWidth) * 2);
			if(isNaN(borderWidth))
				borderWidth = 2
		}
		else
			var borderWidth = 0;

		if(this.container.childNodes[0].rows.length > this.maxItems)
		{
			var maxHeight = (parseInt(this.container.childNodes[0].rows[0].offsetHeight) * parseInt(this.maxItems));
			return (maxHeight + borderWidth);
		}
		else
			return (parseInt(this.container.childNodes[0].offsetHeight) + borderWidth);
	},

	enterSelection : function()
	{
		if(!this.selectedItem)
			return;
			
		this.assignValue(this.selectedItem["value"], this.selectedItem["text"]);
	},
	
	assignValue : function(value, text)
	{		
		this.element.val(text);
		
		this.element.attr("displayValue", text);
		this.element.attr("fieldValue", value);
		
		this.container.style.display = "none";
		this.selectedItem = null;

		this.element.trigger("change");
	},

	mouseClick : function(e)
	{
		this.mouseIn(e);
		this.enterSelection();
	},

	mouseIn : function(e)
	{
		var row = e.target;
		while(row.tagName.toLowerCase() != "tr")
			row = row.parentNode;
		
		this.setSelectedItem(row);
	},

	mouseOut : function(e)
	{
	},

	scroll : function(direction)
	{
		var row = null
		var selectedRow = jQuery( this.selectedItem["row"] ); 
		if (direction < 0 )
			if ( selectedRow.prev().length == 0 )
				row = selectedRow.parent().children(":last")[0];
			else
				row = selectedRow.prev()[0];
		else if ( selectedRow.next().length == 0 )
			row = selectedRow.parent().children(":first")[0];
		else
			row = selectedRow.next()[0];
							
		this.setSelectedItem(row);

		this.scrollIntoView(direction);
	},

	scrollIntoView : function(direction)
	{
		var row = this.selectedItem["row"];
		var top = row.offsetTop
		if(top < this.container.scrollTop || top > this.container.scrollTop + this.container.offsetHeight - row.offsetHeight)
			row.scrollIntoView( (direction < 0) );
	},

	setSelectedItem : function(row)
	{
		jQuery(this.container.childNodes[0]).find("tr").removeClass("suggest-lookup-item-highlighted");

		this.selectedItem = new Object();
		this.selectedItem["row"] = row;
		this.selectedItem["value"] = row.getAttribute("value");
		this.selectedItem["text"] = row.cells[0].innerHTML;

		jQuery(row).addClass("suggest-lookup-item-highlighted");
	}
});
