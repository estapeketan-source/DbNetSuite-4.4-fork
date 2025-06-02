var DbNetSpellDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"dbnetspell-dialog");
		this.errors = null;
		this.corrections = null;
		this.element = null;
		this.tokenIndex = 0;
		this.nodeIndex = 0;
		this.currentPosition = -1;
		this.token = "";
		this.tokenRegExp = null;	
		this.skipTokens = {};
		this.textNode = null;
		this.elementWindow = null;
		this.elementDocument = null;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".replace").bind("click",this.createDelegate(this.replace));
		this.dialog.find(".replace-all").bind("click",this.createDelegate(this.replaceAll));
		this.dialog.find(".skip").bind("click",this.createDelegate(this.skip));
		this.dialog.find(".skip-all").bind("click",this.createDelegate(this.skipAll));
		this.dialog.find(".add").bind("click",this.createDelegate(this.add));
		this.dialog.find(".cancel").bind("click",this.createDelegate(this.cancel));
		this.dialog.find(".suggestions").bind("change",this.createDelegate(this.setChangeTo));
		this.dialog.find(".change-to").bind("keyup",this.createDelegate(this.filterSuggestions));

		if (this.parentControl.preBuildDialog)
			return;
			
		this.open();
	},
	
	open : function() {
		this._super();

		this.errors = this.parentControl.errors;
		this.corrections = this.parentControl.corrections;
		this.elementWindow = this.parentControl.elementWindow;
		this.elementDocument = this.parentControl.elementDocument;
		
		this.skipTokens = {};
		this.tokenIndex = 0;
		this.nodeIndex = 0;
		this.currentPosition = -1;
		
		this.textNodes = this.parentControl.element.textNodes();
		if (this.textNodes.length == 0)
			this.textNodes.push(this.parentControl.element);

		this.processErrors();
	},
		
	processErrors : function() {
		while (this.tokenIndex < this.errors.length && this.nodeIndex < this.textNodes.length)
			if (!this.processErrorToken())
				break;
	
		if (this.tokenIndex >= this.errors.length || this.nodeIndex >= this.textNodes.length)
		{
			var args = {element:this.parentControl.element[0]} ;
			this.parentControl.fireEvent("onElementSpellCheckCompleted", args);
		}
	},
	
	processErrorToken : function() {
		while (this.nodeIndex < this.textNodes.length)
			if ( this.processTextNode() )
				return false;
				
		return true;
	},
	
	processTextNode : function(  ) {
		this.token = this.errors[this.tokenIndex];
		this.textNode = this.textNodes[this.nodeIndex];
		
		var wordBoundary = "(\\b)";
		
		this.tokenRegExp = new RegExp(wordBoundary + this.token + wordBoundary,"g");
		
		var match = null;
		
		var s = this.textNode.nodeValue ? this.textNode.nodeValue : this.textNode.val();

		while ((arr = this.tokenRegExp.exec( s )) != null)
		{
			if (arr.index > this.currentPosition)
			{
				match = arr;
				break;
			}
		}
		if (!match)
		{	
			if (this.skipTokens[this.token])
			{
				this.tokenIndex++;
			}
			else
			{
				this.nodeIndex++;
				this.currentPosition = -1;
			}
			return false;
		}
			
		var start = match.index;
		var end = start + this.token.length;
		this.currentPosition = start;

		if (this.skipTokens[this.token])
		{
			this.tokenIndex++;
			return false;
		}
		
		this.dialog.find(".not-found").val(this.token);
		this.assignSuggestions("");
		this.setChangeTo();

		var el;
		
		if (this.textNode.nodeValue)
			el = this.textNode.parentNode;
		else
			el = this.textNode[0];
			
		if (el.setSelectionRange)
		{
			el.setSelectionRange(start, end);
		}
		else if (window.getSelection)
		{
			this.elementWindow.getSelection().removeAllRanges();
			var range = this.elementDocument.createRange();
            range.setStart(this.textNode,start); 
            range.setEnd(this.textNode,end);	
            this.elementWindow.getSelection().addRange(range);	
		}
		else if (document.body.createTextRange)
		{
			var range = this.elementWindow.document.body.createTextRange();
			range.moveToElementText( el );
			range.collapse(true);
			
			var findText = false;
			
			if (this.textNode.nodeValue)
				if (this.textNode.parentNode.id == this.parentControl.element.attr("id"))
					findText = true;
			
			if (findText)
			{
				if (start == 0)
					range.findText(this.token);
				else
					range.findText(this.token, start);
			}
			else
			{
				range.moveStart('character', start);
				range.moveEnd('character', this.token.length);
			}
			
			range.select();
			range.scrollIntoView();
		}
		el.focus();
		
		return true;
	},
	
	setChangeTo : function() {
		var combo = this.dialog.find(".suggestions")[0];
		var idx = combo.selectedIndex;
		this.dialog.find(".change-to").val(combo.options[idx].text);
	},	
	
	filterSuggestions : function(event) {
		this.assignSuggestions(event.target.value.toLowerCase());
	},		
	
	assignSuggestions : function(filter) {
		var combo = this.dialog.find(".suggestions");
		combo.empty();

		var corrections = this.corrections[this.token];
		
		for (var i=0; i<corrections.length; i++)
		{
			var c = corrections[i];
			
			if (c.toLowerCase().indexOf(filter) == 0 || filter == "")
				combo.append(jQuery('<option>').text(c).val(c));
		}
		if (combo[0].options.length > 0)
			combo[0].selectedIndex = 0;			
	},		
		
	replace : function() {
		this.replaceToken(this.textNode, false, true);
		this.skip();
	},	

	replaceAll : function() {
		this.replaceToken(this.textNode, true, true);
		for (var i=this.nodeIndex+1; i< this.textNodes.length; i++)
			this.replaceToken(this.textNodes[i], true, false);
	
		this.skipTokens[this.token] = true;
		this.skip();
	},	
	
	replaceToken : function(textNode, replaceAll, fromCurrentPosition) {
		var pos = fromCurrentPosition ? this.currentPosition : 0;
		var s = textNode.nodeValue ? textNode.nodeValue : textNode.val();
		var oldWord = this.dialog.find(".not-found").val();
		var newWord = this.dialog.find(".change-to").val();
		var newText = s.substr(0,pos);
		
		if (replaceAll)
			newText += s.substr(pos).replace(this.tokenRegExp,"$1"+newWord+"$2");
		else
			newText += s.substr(pos).replace(oldWord,newWord);
			
		if (textNode.nodeValue)
			textNode.nodeValue = newText;
		else
			textNode.val(newText);
	},

	skip : function() {
		this.tokenIndex++;
		this.processErrors();
	},	

	skipAll : function() {
		this.skipTokens[this.token] = true;
		this.skip();
	},	

	add : function() {
		this.messageBox("question","Please confirm addition of <b>" + this.token.toLowerCase() + "</b> to the dictionary",this.createDelegate(this.addConfirmed));
	},	

	addConfirmed : function(buttonPressed) {
		if (buttonPressed != "yes")
			return;
	
		this.parentControl.addTokenToDictionary(this.token);
		this.showMessage("Word added to dictionary");
		this.skipTokens[this.token] = true;
		this.skip();		
	},	

	cancel : function() {
		this.close();
	}
});