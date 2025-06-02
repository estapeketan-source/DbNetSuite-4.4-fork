var PdfSettingsDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"pdf-settings");
		this.html = null;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));

		this.open();
	},
	
	assignSettings : function(settings) {
		settings.portrait = this.dialog.find("input[value=portrait]")[0].checked
		settings.fontFamily = this.dialog.find(".font-family").val()
		settings.fontSize = this.dialog.find(".font-size").val()
		settings.html = this.html;
		
		var totalWidth = 0;
		var cellWidths = [];
		var row = this.parentControl.table[0].rows[0];
		
		if ( !this.parentControl.nestedGrid )
		{		
			for( var i=0; i < row.cells.length; i++)
			{
				var w = jQuery(row.cells[i]).width();
				cellWidths.push(w);
				totalWidth += w;
			}
			for( var i=0; i < cellWidths.length; i++)
				cellWidths[i] = (cellWidths[i] / totalWidth) * 100;
			
			settings.cellWidths = cellWidths;	
		}	
	},	
	
	apply : function(event) {
		this.parentControl.exportGrid2( this.html );
		this.close();
	}	
});