var TextEditor = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"text-editor");
		this.targetElement = null;
		this.dbNetSpell = new DbNetSpell(parentControl.id + "_textSpellChecker");
	},
	
	build : function(response) {
		this._super(response);
	},
	
	configure : function() {
		this.resizable = true;
	
		this._super();

		this.dialog.find(".spellCheck-button").bind("click",this.createDelegate(this.checkSpelling));
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		
		this.dbNetSpell.registerElement(this.dialog.find(".text-editor"));
		this.dbNetSpell.connectionString = this.parentControl.dbNetSpell.connectionString;
		this.dbNetSpell.dictionaryTableName = this.parentControl.dbNetSpell.dictionaryTableName;
		
		this.dbNetSpell.createButton = false;
		this.dbNetSpell.initialize();

		var w = (screen.availWidth * 0.8);
		var h = (screen.availHeight * 0.6);
			
		this.dialog.find(".text-editor").width(w);
		this.dialog.find(".text-editor").height(h);
		this.dialog.find(".text-editor-toolbar").width(w);
		
		this.open();
	},
	
	resize : function() {
		this._super();
		this.onResize();
	},
	
	checkSpelling : function() {
		this.dbNetSpell.checkSpelling();
	},	
		
	onResize : function(event) {
		var w = this.dialog.width() -5;
		var h = this.dialog.height() -35;
		this.dialog.find(".text-editor").width(w);
		this.dialog.find(".text-editor").height(h);
		this.dialog.find(".text-editor-toolbar").width(w);
		this.setTitleWidth();
	},	
	
	open : function(event) {
		this.update();
		this._super();
	},
	
	update : function() {
		if (this.targetElement.attr("spellCheck").toLowerCase() == "true") 	
			this.dialog.find(".spellCheck-button").show();
		else			
			this.dialog.find(".spellCheck-button").hide();
			
		this.assignLabel();
		this.dialog.find(".text-editor").val(this.targetElement.val());
	},
		
	apply : function(event) {
		this.targetElement.val(this.dialog.find(".text-editor").val());
		this.targetElement.trigger("change");
		
		this.close();
	}
	
});
