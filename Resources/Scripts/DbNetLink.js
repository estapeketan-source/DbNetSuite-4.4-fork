var DbNetLink = {
	components : {},
	requestToken : ""
};

window.DbNetGridArray = {};
window.DbNetEditArray = {};
window.DbNetFileArray = {};
window.DbNetSpellArray = {};
window.DbNetComboArray = {};
window.DbNetListArray = {};

Date.prototype.toJSON = function() {
	var offset = (0 - this.getTimezoneOffset()) * 60 * 1000;
	return ('/Date(' + (this.getTime() + offset).toString() + ')/');
}
 
jQuery.fn.reverse = [].reverse;

String.prototype.toProperCase = function () {
    return this.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
};

DbNetLink.Util = {
	addScript : function(fileName) {
		var head = document.getElementsByTagName("head")[0];
		var script = document.createElement('script');
		script.id = 'extraScript';
		script.type = 'text/javascript';
		script.src = fileName;
		head.appendChild(script)		
	},
	
	jQueryVersion : function() {
		return parseFloat(jQuery.fn.jquery.split(".").slice(0,2).join("."));		
	},	

	capitalise : function(s) {
		return(s.charAt(0).toUpperCase()+ s.substr(1));		
	},

	dateFormat : function() {
		return jQuery.datepicker._defaults.dateFormat;
	},
	
	stringify : function(data) {
		return JSON.stringify(data).replace(/(\/Date\((-)?\d{1,}\))\//g, "\\$1\\/");
	},
	
	dateToString : function(d,f,addTime) {
		if (typeof(d) == "string")
			d = DbNetLink.Util.stringToDate(d);
			
        if (!f)
			f = this.dateFormat();
		
		var s = jQuery.datepicker.formatDate(f, d);
		
		if (addTime)
			s += " " + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
			
		return s;
	},	
	
	stringToDate : function(d,f) {
        if (!f)
			f = this.dateFormat();
		return jQuery.datepicker.parseDate(f, d);	
	},		
	
	JSONDate : function(d) {
		return ('/Date(' + d.getTime() + ')/');
	},	

	webMethod : function(method, callback, data, url)
	{
		if (!data)
			data = {};

		if (!url)
			url = (window.location.href.split("?")[0].split("/").pop());
			
		url = url.replace(/#.*$/,"");
		
		if (url.split("/")[url.split("/").length-1] == "")
			url += "default.aspx";
		
		var config = {
			type: "POST",
			url: url + "/" + method,
			data: DbNetLink.Util.stringify(data),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			dataFilter: DbNetLink.Util.filterResponse,
			success : callback, 
			error: DbNetLink.Util.webMethodError
			};
			
		if (callback != null)
			config.success = callback;
		else
			config.success = function(){};			

		config.async = (callback != null);
		
		var xhr = jQuery.ajax(config);  
		
		if (callback != null)
			return null;
			
		if (xhr.status != 200)
		{
			this.webMethodError(xhr);
			return null
		}
		var data = null;
		
		
		try
		{
			if (xhr.responseText != "null") {
				data = jQuery.parseJSON(xhr.responseText);
				if (data.hasOwnProperty("d"))
					data = data.d;
			}
		}
		catch(ex)
		{
			this.webMethodError(xhr.responseText);
			return null;
		}
		
		return data;		
	},
	
	filterResponse : function(data, type)
	{
		if (data == "null")
			return null;

		var msg = jQuery.parseJSON(data);
 
		if (msg.hasOwnProperty("d"))
			return DbNetLink.Util.stringify(msg.d);
		else
			return data;
	},	
	
	webMethodError : function(xhr)
	{
		try
		{
			var o = JSON.parse(xhr.responseText);
			alert(o.Message + o.StackTrace);
		}
		catch(ex)
		{
			alert(xhr.responseText);
		}
	},
	
    sizeDialog : function(event, ui){
        jQuery(event.target).dialog("option", "width", jQuery(event.target).children().width());
        jQuery(event.target).dialog("option", "position", "center");
    },
		 
	sizeWindowToContent : function() {
        var height = jQuery(document).height();
        var width = jQuery(document).width();
    
        if (typeof (window.sizeToContent) == "function")
            window.sizeToContent();
        else
            window.resizeTo(width + 30, height + 30);

        var winl = (screen.width - width) / 2;
        var wint = (screen.height - height) / 2;
        
		window.moveTo(winl, wint);
	},
	
	loadCombo: function(combo, items, addEmptyOption, emptyOptionText, value){
		jQuery(combo).empty();
	
		var f = document.createDocumentFragment();
		
		if (addEmptyOption)
			f.appendChild(jQuery( document.createElement("option") ).val("").text(emptyOptionText)[0]);

		for (var i=0; i<items.length; i++)
		{
			var item = items[i];
			var opt = jQuery(document.createElement("option"));
			
			for (p in item)
			{
				switch(p)
				{
					case "text":
						opt.text(item[p]);
						break;
					case "val":
					case "value":
						opt.val(item[p]);
						break;
					default:
						opt.attr(p,item[p]);
						break;
				}		
			}
			
			f.appendChild(opt[0]);
		}
		
		jQuery(combo)[0].appendChild(f);
		
		if (value)
			jQuery(combo).val(value);
	}				
};