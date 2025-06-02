var ChartConfigDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"chart-config");
		this.xc = null;
		this.yc = null;
		this.chartProperties = ["Height","Width","BackColor","BackGradientStyle","BackSecondaryColor","BackHatchStyle","AutoLoad","ChartPanel"];
		this.seriesProperties = ["ChartType","Palette","IsValueShownAsLabel","BorderColor"];
		this._3dProperties = ["Enable3D","Inclination","IsClustered","IsRightAngleAxes","LightStyle","Perspective","PointDepth","PointGapDepth","Rotation"];
		this.borderSkinProperties = ["SkinStyle"];
		this.titleProperties = ["Text","Docking","Alignment","BackColor","ForeColor"];
		this.chartAreaProperties = ["BackColor","BackGradientStyle","BackSecondaryColor","BorderColor","BackHatchStyle","BorderDashStyle","BorderWidth"];
		this.legendProperties = ["Enabled","ForeColor","BackColor","BorderColor","BorderDashStyle", "BorderWidth","LegendStyle","TableStyle","Docking"];
		this.chartPanel = null;
		this.serializeDialog = null;
		this.tb = null;
	},
	
	configure : function() {
		this._super();
		this.chartPanel = this.dialog.find(".chart-config-panel");
		this.chartPanel.css("overflow","auto").css("height","200px").css("width","680px");
		
		if (!this.parentControl.chartSerialize)
			this.dialog.find(".serialize-button").hide();
		
		this.dialog.find(".serialize-button").bind("click",this.createDelegate(this.serializeChart));
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".apply-button").bind("click",this.createDelegate(this.apply));
		this.dialog.find(".colour-select").bind("change",this.createDelegate(this.assignColour));
		this.xc = this.dialog.find(".x-axis-column");
		this.yc = this.dialog.find(".y-axis-column");
		
		this.tb = this.dialog.find(".serialize-textbox");
		this.serializeDialog = this.dialog.find(".serialize-dialog");

		var o = {};
		o.autoOpen = false;
		o.resizable = false;
		o.bgiframe = true;
	    o.open = DbNetLink.Util.sizeDialog;

		this.serializeDialog.dialog(o);	
		this.bind("onOpen", this.createDelegate(this.assignConfiguration));

		this.open();
	},
	
	restore : function(event) {
		if (this.configured)
			this.assignConfiguration();
	},	

	assignConfiguration : function() {
		this.xc.empty();
		this.yc.empty();
		
		for (var i=0; i<this.parentControl.columns.length; i++)
		{
			var c = this.parentControl.columns[i];
			var combo;
			var opt = jQuery(document.createElement("option"));
			if (c.dataType == "String" || c.dataType == "DateTime" || c.lookup != "")
				combo = this.xc;
			else
				combo = this.yc;
			
			opt.text(c.label);				
			opt.val(c.columnName);				
				
			combo.append(opt);	
		}
		
		var o = this.parentControl.chartConfig;
		
		this.assignProperties(o, "chart");
		this.assignProperties(o.chartArea, "chart-area");
		
		if (jQuery.isArray(o.series))
			for (var i=0; i < o.series.length; i++)
				this.assignSeriesProperties(o.series[i]);
		else
			this.assignSeriesProperties(o.series);
		
		if (o.titles.length)
			this.assignProperties(o.titles[0], "title");
		else
			this.assignProperties(o.title, "title");
			
		this.assignProperties(o.legend, "legend");
		this.assignProperties(o.area3DStyle, "three-d");
		this.assignProperties(o.borderSkin, "border-skin");
	},
	
	buildConfig : function(event) {
		if (this.xc.children(":selected").length == 0)
		{
			this.showMessage("An X-axis column must be selected");
			return false;
		}
		
		if (this.yc.children(":selected").length == 0)
		{
			this.showMessage("At least one Y-axis column must be selected");
			return false;
		}		
			
		var o = this.parentControl.chartConfig;
		
		this.setProperties(this.chartProperties, o, "chart");
		this.setProperties(this.chartAreaProperties, o.chartArea, "chart-area");
		
		var series = this.yc.children(":selected");
		
		if (series.length > 1)
		{
			o.series = [];
			for ( var i=0; i <series.length; i++)
				o.series.push( this.setSeriesProperties(series[i],{}));
		}
		else
			o.series = this.setSeriesProperties(series[0],o.series);
		
		this.setProperties(this._3dProperties, o.area3DStyle, "three-d");
		this.setProperties(this.borderSkinProperties, o.borderSkin, "border-skin");
		
		if (!o.title.Font)
			o.title.Font = {};
			 
		this.setProperties(this.titleProperties, o.title, "title");
		this.setProperty(o.title.Font,"Family", "title");
		this.setProperty(o.title.Font,"Size", "title");

		this.setProperties(this.legendProperties, o.legend, "legend");
		this.parentControl.chartConfig = o;
		
		return true;
	},
	
	serializeChart : function(event) {
		if (!this.buildConfig())
			return;
			
		var s = JSON.stringify(this.parentControl.chartConfig, null, 4);	
		
		if (this.parentControl.isServerControl())
			s = "ChartConfig = \"" + s.replace(/"/g,"'") + "\"";
		else
			s = "DbNetLink.components[\"" + this.parentControl.id + "\"].chartConfig = " + s;
		this.tb.val(s);
		this.serializeDialog.dialog("open");	
	},	
		
	apply : function(event) {
		if (!this.buildConfig())
			return;
		this.parentControl.buildChart();
	},
	
	assignColour : function(e) {
		var c = jQuery(e.target);
		c.css("background-color", c.children(":selected").css("background-color"));
	},	
	
	cap : function(s) {
		return DbNetLink.Util.capitalise(s);
	},
	
	assignSeriesProperties : function(s) {
		this.assignProperties(s,"series");

		if (s.xValueMember)
			this.xc.val(s.xValueMember);
		if (s.yValueMembers)
			this.yc.children("[value=" + s.yValueMembers + "]")[0].selected = true;
	},

	assignProperties : function(o, container) {
		for ( var p in o )
			if (this.isObject(o[p]) && container != "chart")
				this.assignProperties(o[p],container);
			else
			{
				var e = this.dialog.find("table." + container ).find("." + this.cap(p));
				this.parentControl.setInputValue( e,o[p] );	
			}
	},
	
	setSeriesProperties : function(option, s) {
		this.setProperties(this.seriesProperties, s, "series");

		s.xValueMember = this.xc.val();
		s.yValueMembers = option.value;
		s.name = option.text;	
		return s;
	},	
	
	setProperties : function(properties, o, container) {
		for ( var i=0; i <properties.length; i++)
		{
			var p = properties[i];
			this.setProperty(o,p, container);
		}
	},
	
	setProperty : function(o, p, container) {
		var e;
		if ( container )
			e = this.dialog.find("table." + container).find("." + p);
		else
			e = this.dialog.find("." + p);
			
		if (e.length == 0)
			alert( p + ":" + container);
			
		var v = this.parentControl.getInputValue( e );
		
		p = p.substr(0, 1).toLowerCase() + p.substr(1);
		
		o[p] = v;
		
		if (v == "" && e[0].tagName.toLowerCase() == "select")
			delete o[p];
	}
	
});
