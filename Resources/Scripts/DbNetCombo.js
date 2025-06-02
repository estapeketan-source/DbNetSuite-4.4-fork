var DbNetCombo = DbNetSuite.extend({
	init: function(id){
		DbNetLink.components[id] = this;
		this._super(id);
		
		this.combo = this.container;

		if (this.combo.prop("tagName").toLowerCase() != "select")
			alert("ID must specify a <select> element");
			
		jQuery(this.combo).addClass("dbnetcombo");
		this.assignAjaxUrl("dbnetcombo.ashx");
		//this.ajaxConfig.url = "dbnetcombo.ashx";
		
		this.linkedControls = [];
		this.addEmptyOption = false;
		this.emptyOptionText = "";
		this.sql = "";
		this.parameters = {};
		this.currentValue = "";
		this.newValue = "";
	},
	
	ajaxDataProperties: function(values){
		return this._super().concat([
				"sql",
				"parameters"
				]);
	},
	
	initialize: function(){
		this.combo.bind("change", this.createDelegate(this.comboValueChanged));
		this.bind("onItemsLoaded", this.createDelegate( this.comboValueChanged) )
		this.bind("onItemsCleared", this.createDelegate( this.comboValueChanged) )
		
		this.initialized = true;		
		this.fireEvent("onInitialzed");
		this.load();

	},
	
	load: function(value){
		if (!this.initialized)
			this.initialize();

		this.fireEvent("onBeforeItemsLoaded");
		this.callServer("load", { method : this.loadCallback, params : [value]});	
	},
	
	loadCallback: function(response, value){
	    this.loadCombo(this.combo, response.items, this.addEmptyOption, this.emptyOptionText, value);
	    this.currentValue = this.combo.val();
		this.fireEvent("onItemsLoaded");
	},
	
	clear: function(){
		if (this.combo.children().length > 0)
		{
			this.combo.empty();
			this.fireEvent("onItemsCleared");
		}
	},	

	addLinkedControl: function(ctrl, oneToOne){
	    if (!oneToOne)
	        oneToOne = false;
	    this.linkedControls.push({ ctrl: ctrl, oneToOne: oneToOne });
	},		
	
	comboValueChanged: function(combo){
	    this.newValue = this.combo.val();
	    this.fireEvent("onChange");
		if (this.loadChildControls())
		    this.currentValue = this.combo.val();
		else
		    this.combo.val(this.currentValue);
	},
	
	loadChildControls : function()
	{
		for (var i=0; i<this.linkedControls.length; i++)
		{
		    var lc = this.linkedControls[i];

		    if (lc.ctrl.connectionString == "") {
		        lc.ctrl.connectionString = this.connectionString;
		        lc.ctrl.commandTimeout = this.commandTimeout;
			}
		}
			
		if (this.combo.children().length == 0)
		{
			for (var i=0; i<this.linkedControls.length; i++)
				this.linkedControls[i].clear()
			return true;
		}
	
		var opt = jQuery(this.selectedOption());
		for (var i=0; i<this.linkedControls.length; i++)
		{
			var lc = this.linkedControls[i];

			if (lc.ctrl instanceof DbNetCombo)
			{	
			    for (p in lc.ctrl.parameters)
				{
					if ( opt.attr(p) != undefined)
					    lc.ctrl.parameters[p] = opt.attr(p);
					else
					    lc.ctrl.parameters[p] = opt.val();
				}
			    lc.ctrl.load();
			}
			else
			{
			    if (lc.ctrl instanceof DbNetEdit) {
			        if (lc.ctrl.saveAutomatically(this.createDelegate(this.controlSaved), lc.ctrl, { linkedControl: lc })) {
			            return false;
			        }
			    }
			    this.loadChildControl(lc);
			}
		}

		return true;
	},

	loadChildControl: function (lc) {
	    lc.ctrl.assignParentFilter(this, { key: this.combo.val() }, lc.oneToOne);
	    lc.ctrl.currentPage = 1;
	    lc.ctrl.getRecordSet();
	},

	controlSaved: function (params) {
	    this.combo.val(this.newValue);
	    this.currentValue = this.combo.val();
	    this.loadChildControl(params.linkedControl);
	},

	selectedOption: function(){
		return this.combo.children(":selected");
	}		
});	