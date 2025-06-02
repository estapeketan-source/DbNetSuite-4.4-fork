var HtmlEditor = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"html-editor");
		this.targetElement = null;
		this.activeEditor = null;
		this.editors = [];
		this.dbNetSpell = new DbNetSpell( parentControl.id + "_htmlSpellChecker");
		this.currentEditor = "simple";
	},
	
	build : function(response) {
		this._super(response);
	},
	
	configure : function() {
		this.resizable = (window.tinyMCE != undefined);
	
		this._super();
		
		if (window.tinyMCE)
		{
			this.dialog.dialog("option", "width", 500);
			this.dialog.dialog("option", "height", 400);
		}
		else
		{
			this.dialog.dialog("option", "width", screen.availWidth * 0.8);
			this.dialog.dialog("option", "height", (screen.availHeight * 0.6) + 100);
		}
						
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".simple-button").bind("click",this.createDelegate(this.switchEditor));
		this.dialog.find(".advanced-button").bind("click",this.createDelegate(this.switchEditor));
		this.dialog.find(".spellCheck-button").bind("click",this.createDelegate(this.checkSpelling));
		
		this.dbNetSpell.connectionString = this.parentControl.dbNetSpell.connectionString;
		this.dbNetSpell.dictionaryTableName = this.parentControl.dbNetSpell.dictionaryTableName;
		this.dbNetSpell.createButton = false;
		this.dbNetSpell.initialize();	
				
		this.dialog.find(".simpleHtmlEditor").attr("id", this.componentId + "_simpleHtmlEditor");
		this.dialog.find(".advancedHtmlEditor").attr("id", this.componentId + "_advancedHtmlEditor");
		
		if (window.tinyMCE)
		{
			this.tinyMCEInit(
				"simple",
				{
					theme_advanced_buttons1 : "bold,italic,underline,strikethrough,|,undo,redo,|,cleanup,|,bullist,numlist|,formatselect,fontselect,fontsizeselect",
					theme_advanced_buttons2 : "",
					theme_advanced_buttons3 : ""
				}
			);

			this.tinyMCEInit(
				"advanced",
				{
					plugins : "table,advimage,advlink,emotions,insertdatetime,media,searchreplace,print,contextmenu,paste,directionality,noneditable,visualchars",
					theme_advanced_buttons1 : "newdocument,|,bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,sub,sup,|,formatselect,fontselect,fontsizeselect",
					theme_advanced_buttons2 : "cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,undo,redo,|,link,unlink,anchor,image,cleanup,help,code,|,insertdate,inserttime,|,forecolor,backcolor",
					theme_advanced_buttons3 : "tablecontrols,|,hr,removeformat,visualaid,|,charmap,emotions,media,|,print,|,ltr,rtl"
				}
			);
		}
		else
		{
	
			this.editorInit();
		}
	},
	
	resize : function() {
		this._super();
		this.onResize();
	},
		
	onResize : function(event) {
		var w = this.dialog.width() -5;
		var h = this.dialog.height() -60;
		if (window.tinyMCE)
		{
			this.$("simpleHtmlEditor_ifr").height(h).width(w);
			this.$("advancedHtmlEditor_ifr").height(h-50).width(w);
		}
		this.setTitleWidth();
	},		
	
	tinyMCEInit : function(mode, config) {
		config.isAdvancedMode = (mode == "advanced");
		config.mode = "exact";
		config.elements = this.componentId + "_" + mode + "HtmlEditor";
		config.theme = "advanced";
		config.theme_advanced_toolbar_location = "top";
		config.theme_advanced_toolbar_align = "left";			
		config.theme_advanced_toolbar_location = "top";
		config.oninit = this.createDelegate(this.editorInit);

		this.parentControl.fireEvent("onBeforeTinyMceInit", {config : config});
			
		tinyMCE.init(config);	
	},		
	
	editorInit : function(ed) {
		if (window.tinyMCE)
		{
			this.editors.push(ed);
			if (this.editors.length < 2)
				return;
		}
			
		var h = screen.availHeight * 0.5
		var w = screen.availWidth * 0.5
		
		if (window.tinyMCE)
		{
			this.activeEditor = tinymce.EditorManager.editors[this.componentId + "_simpleHtmlEditor"];
			this.$("simpleHtmlEditor_ifr").height(h).width(w);
			this.$("advancedHtmlEditor_ifr").height(h).width(w);
			this.dialog.find(".advanced-button").show();				
		}

		
		this.dialog.find(".advancedPanel").hide();
		this.dialog.find(".simple-button").hide();
		this.dialog.find(".simplePanel").show();
		
		this.dialog.dialog("open");
		this.yOffset = parseInt(jQuery(this.dialog).offset().top);
		this.fireEvent("onOpen");	
			
		if (!window.tinyMCE)
		{
			var w = 
			this.dialog.find(".simpleHtmlEditor").css("width", (screen.availWidth * 0.8).toString() + "px").css("height", (screen.availHeight * 0.6).toString() + "px");

			this.dialog.find(".html-editor-toolbar").width((screen.availWidth * 0.8));
			
			var config = {};
			config.iconsPath = dbnetsuite.nicEditGif;
			config.fullPanel = true;
			config.maxHeight = this.dialog.find(".simpleHtmlEditor").height();
		
			var ne = new nicEditor(config);
			ne.panelInstance(this.componentId + "_simpleHtmlEditor");
			
			var ed = nicEditors.findEditor(this.componentId + "_simpleHtmlEditor");
			this.dialog.find(".simpleHtmlEditor").parent().find(".nicEdit-main").css("backgroundColor","#FFFFFF"); 
			
			this.activeEditor = nicEditors.findEditor(this.componentId + "_simpleHtmlEditor");
			this.dialog.find(".advanced-button").hide();
		}	
		
		this.update();
	},
	
	open : function() {
	    this.update()

	    if (window.tinyMCE) {
	        var w = screen.availWidth * 0.5
	        this.$("simpleHtmlEditor_ifr").width(w);
	        this.$("advancedHtmlEditor_ifr").width(w);
	    }

		this._super();
	},
	
	checkSpelling : function() {
		this.dbNetSpell.elements = [];
		if (window.tinyMCE)
			this.dbNetSpell.registerElement(this.$( this.currentEditor + "HtmlEditor_ifr"));
		else
			this.dbNetSpell.registerElement(this.dialog.find(".simpleHtmlEditor").parent().find(".nicEdit-main"));

		this.dbNetSpell.checkSpelling();
	},
	
	update : function() {
		if (this.targetElement.attr("spellCheck").toLowerCase() == "true") 	
			this.dialog.find(".spellCheck-button").show();
		else			
			this.dialog.find(".spellCheck-button").hide();
			
		this.assignLabel();			
				
		var html;
		if (this.targetElement.attr("editFieldType") == "HtmlPreview")
			html = this.targetElement.html()
		else
			if (window.tinyMCE)
				html = tinymce.EditorManager.editors[this.targetElement.attr("id")].getContent();
			else
				html = nicEditors.findEditor(this.targetElement.attr("id")).getContent();
					
		if (window.tinyMCE)
		{
			tinymce.EditorManager.editors[this.componentId + "_simpleHtmlEditor"].setContent(html);
			tinymce.EditorManager.editors[this.componentId + "_advancedHtmlEditor"].setContent(html);
		}
		else
			nicEditors.findEditor(this.componentId + "_simpleHtmlEditor").setContent(html);
			
		this.resize();	
		
	},
		
	apply : function(event) {
		var content = this.activeEditor.getContent();
		
		this.targetElement.parents("table:first").find(".html-content").val(content);
		this.targetElement.parents("table:first").find(".html-content").trigger("change");

		if (this.targetElement.attr("editFieldType") == "HtmlPreview")
		{
			this.targetElement.html(content);
			this.targetElement.attr("modified", "true");
		}
		else
		{
			if (window.tinyMCE)
			{
				var ed = tinymce.EditorManager.editors[this.targetElement.attr("id")];
				ed.setContent(content);
				ed.isNotDirty = false;
			}
			else
			{
				nicEditors.findEditor(this.targetElement.attr("id")).setContent(content);
			}
		}	
		
		this.close();
	},
	
	switchEditor : function(event) {
		var newEd = (event.target.className == "simple-button") ? "simple" : "advanced";
		var oldEd = (event.target.className == "simple-button") ? "advanced" : "simple";
		
		this.activeEditor = tinymce.EditorManager.editors[this.componentId + "_" + newEd + "HtmlEditor"];
		
		var content = tinymce.EditorManager.editors[this.componentId + "_" + oldEd + "HtmlEditor"].getContent();
		this.activeEditor.setContent(content);
		
		this.dialog.find("." + newEd + "Panel").show();
		this.dialog.find("." + oldEd + "Panel").hide();
		
		this.dialog.find("." + newEd + "-button").hide();
		this.dialog.find("." + oldEd + "-button").show();
		
		this.currentEditor = newEd;
	}
	
});
