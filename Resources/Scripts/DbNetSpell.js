$.fn.textNodes = function() {
  var ret = [];
  this.each( function() {
	var fn = arguments.callee;
	jQuery(this).contents().each( function() {
	  if ( this.nodeType == 3 ) 
		ret.push( this );
	  else fn.apply( jQuery(this) );
	});
  });
  return jQuery(ret);
}

$.fn.textNodes2 = function() {
    var ret = [];

    (function(el){
        if (!el) return;
        if ((el.nodeType == 3))
            ret.push(el);
        else
            for (var i=0; i < el.childNodes.length; ++i)
                arguments.callee(el.childNodes[i]);
    })(this[0]);
    return jQuery(ret);
}



var DbNetSpell = DbNetSuite.extend({
	init: function(id){
		if (id)
			DbNetLink.components[id] = this;
		this._super(id);
		
		jQuery(this.container).addClass("dbnetspell");
		this.assignAjaxUrl("dbnetspell.ashx");
		//this.ajaxConfig.url = "dbnetspell.ashx";

		this.createButton = (id != undefined);
		this.corrections = null;
		this.dialog = null;
		this.dictionaryTableName = "";
		this.elements = [];
		this.elementIndex = 0;
		this.errors = null;
		this.maximumSuggestions = 20;
		this.soundexGroups = "BP,FV,CKS,GJ,QXZ,DT,L,MN,R";
		this.tokenBoundary = "[^a-zA-Z]";
		this.elementWindow = null;
		this.elementDocument = null;
		this.preBuildDialog = false;
	},
	
	ajaxDataProperties: function(values){
		return this._super().concat([
			"tokenBoundary",
			"soundexGroups",
			"maximumSuggestions",
			"dictionaryTableName"
			]);
	},
	
	registerElement: function(selector){
		var elements = [];
		if (typeof(selector) == "string" )
		{
			elements = jQuery(selector);
			if (elements.length == 0)
				elements = jQuery("#" + selector);
		}
		else if ( jQuery.isArray(selector) )
			elements = selector;
		else
			elements = [selector];
			
		if (elements.length == 0)
			alert("registerElement ==> no matching elements found");
		else	
			for (var i=0; i < elements.length; i++ )
				this.elements.push(jQuery(elements[i]));
	},		
	
	initialize: function(){
		if (this.createButton)
		{
			var btnId = this.componentId + 'SpellCheckBtn';
		    this.container.append("<button id='" + btnId + "' type='button'><img src='" + dbnetsuite.spellCheckPng + "'/></button>");
			jQuery("#" + btnId).bind("click", this.createDelegate(this.checkSpelling));
		}
		
		var response = this.callServer("initialize", null);	

		this.connectionString = response.connectionString;
		this.dictionaryTableName = response.dictionaryTableName;
		
		this.initialized = true;		
		this.bind("onElementSpellCheckCompleted", this.createDelegate(this.elementSpellCheckComplete));
		
		if (this.preBuildDialog)
		{
			this.dialog = new DbNetSpellDialog( this );
			var response = this.callServer("dialog");
			this.dialog.build(response);
		}
		
		this.fireEvent("onInitialzed");
		
	},	
	
	checkSpelling: function(){
		if (this.elements.length == 0)
			alert("No elements for spell checking have been registered");
	
		this.elementIndex = 0;
		this.checkElementSpelling();
	},
	
	elementSpellCheckComplete: function(){
		this.elementIndex++;
		if (this.elementIndex < this.elements.length)
		{
			this.checkElementSpelling();
		}
		else
		{
			if (this.dialog)
				this.dialog.close();
			this.fireEvent("onSpellCheckCompleted");
		}
	},	
		
	checkElementSpelling: function(){
		this.element = jQuery(this.elements[this.elementIndex]);
		this.elementWindow = window;
		this.elementDocument = window.document;
					
		if (this.element.prop("tagName").toLowerCase() == "iframe")
		{
			this.elementDocument = (this.element[0].contentWindow || this.element[0].contentDocument);
			if (this.elementDocument.document) this.elementDocument = this.elementDocument.document;
		
			this.elementWindow = this.element[0].contentWindow;
			
			this.element = jQuery(this.elementDocument.body);
		}

		var data = { text : this.getText() };
		this.callServer("check", this.checkSpellingCallback, data);	
	},	
	
	getText: function(){
		switch (this.element.prop("tagName").toLowerCase())
		{
			case "textarea":
			case "input":
				return this.element.val();
			default:
				var textNodes = this.element.textNodes2();
				var text = [];
				
				for (var i=0; i < textNodes.length; i++)
					text.push(textNodes[i].nodeValue);
			
				return text.join(" ");
				break;
		}
	},
		
	checkSpellingCallback: function(response){
		this.openSpellCheckDialog(response);		
	},
	
	openSpellCheckDialog : function(response) {
		if (response.errors.length == 0)
		{
			this.elementSpellCheckComplete();
			return;
		}
		this.errors = response.errors;
		this.corrections = response.corrections;
	
		if (!this.dialog)
		{
			this.dialog = new DbNetSpellDialog( this );
			this.callServer("dialog", {method : this.dialog.build, context:this.dialog} );
		}
		else
			this.dialog.open();	
	},
	
	addTokenToDictionary: function(token){
		var data = { word : token };
		this.callServer("add", null, data);	
	}			
	
});	