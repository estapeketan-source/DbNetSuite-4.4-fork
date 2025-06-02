var UploadDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"upload");
		this.frameId = this.parentControl.id + "_uploadFrame";
		
		this.column = null;
		this.editField = null;
		this.currentFolder = null;
		this.uploadFrame = null;
		this.method = "upload-dialog";
	},

    /*
	build : function(response) {
		this.addContainer();
		this.configured = true;
		this.dialogContainer.html(response.html);
		window.setTimeout(this.createDelegate(this.checkFrameLoaded),100);
	},
    */
	
	open : function() {
		this.dialog.dialog("option", "width", 600);
		this.dialog.dialog("open");
		
		var disable = true;
		var overwrite = false;
		var rename = false;
		
		if ( this.parentControl.typeName() == "DbNetFile")
		{
			disable = !this.parentControl.uploadOverwrite;
		}
		else if (this.column != null)
		{
			disable = !this.column.uploadOverwrite;
			rename = this.column.uploadRename;
			this.label = this.column.label;
			this.assignLabel();
		}
		else
		{
			overwrite = true;
		}

		var frameContents = jQuery("#" + this.frameId).contents();
	
		frameContents.find("#fileInput").val('');
		frameContents.find("#fileOverwrite").prop("disabled", disable).prop("checked", overwrite);
		
		var cells = frameContents.find("td.rename")
		if (rename)
		    cells.show();
		else
		    cells.hide();

		if (disable && !rename)
		    frameContents.find("#optionsRow").hide();
	},		
	
	checkFrameLoaded : function(response) {
		this.frame = window.frames[this.frameId];
		if (this.frame)
		{
			if ( jQuery("#" + this.frameId).contents().find("#fileInput").length  > 0)
			{
				window.setTimeout(this.createDelegate(this.configure),100);
				return;
			}
		}
		window.setTimeout(this.createDelegate(this.checkFrameLoaded),100);
	},	
	
	configure : function() {
		this._super();
		this.dialog.find(".upload-button").bind("click",this.createDelegate(this.upload));
		this.dialog.find(".cancel-button").bind("click",this.createDelegate(this.close));

		window.setTimeout(this.createDelegate(this.open),100);
	},
	
	frameElementById : function(id) {
		return jQuery("#" + this.frameId).contents().find("#" + id);	
	},	

	upload : function() {
		var file = this.frameElementById("fileInput").val();
		
		if(file == null || file == ""){
			this.showMessage(this.translate("SelectAFile"));
			return;
		}	
		
		file = file.split("\\").pop()
		var data = {};
		data.fileName = file;
		data.fileNameWithoutExtension = file.split(".").slice(0, -1).join(".");
		data.fileNameExtension = file.split(".").pop();
		data.overwrite = this.frameElementById("fileOverwrite").prop("checked");
		this.assignDataProperties(data);

		data.alternateFileName = this.frameElementById("alternateFileName").val();
		this.parentControl.fireEvent("onBeforeFileUploadValidate", data);
		this.frameElementById("alternateFileName").val(data.alternateFileName);

		this.parentControl.callServer("validate-upload", { method : this.validateUploadCallback, context : this}, data);
	},
			
	validateUploadCallback : function(response) {
		if (!response.ok)
		{
			this.showMessage(response.message);
			return;
		}

		var data = {};
		data.fileName = this.frameElementById("fileInput").val().split("\\").pop();
		data.method = "upload";
		this.assignDataProperties(data);
	
		jQuery("#" + this.frameId).unbind("load").bind("load", this.createDelegate(this.iframeLoaded));
		
		this.frameElementById("data").val(JSON.stringify(data));
		
		var form = this.frameElementById("theForm")[0];
		form.setAttribute("action", this.parentControl.ajaxConfig.url);
		
		var args = {};
		args.form = form;
		this.parentControl.fireEvent("onBeforeFileUploaded", args);

		this.parentControl.showWait();		
		form.submit();
	},
	
	assignDataProperties : function(data) {
		if ( this.parentControl.typeName() == "DbNetFile")
		{
			data.currentFolder = this.parentControl.currentFolder;
			data.rootFolder = this.parentControl.rootFolder;
			data.uploadMaxFileSizeKb = this.parentControl.uploadMaxFileSizeKb;
			data.uploadFileTypes = this.parentControl.uploadFileTypes;
		}
		else if (this.column != null)
		{
			data.column = this.column;
		}
		else
		{
			data.uploadDataFolder = this.parentControl.uploadDataFolder;
			data.uploadExtFilter = this.parentControl.uploadExtFilter;
		}	
	},
	
	iframeLoaded : function() {
		var iframe = document.getElementById(this.frameId).contentWindow;
		this.parentControl.hideWait();	
		
		if ( typeof(iframe.uploadGuid) == "undefined" )
		{
			var html = jQuery("#" + this.frameId).contents()[0].documentElement.innerHTML;
			this.openErrorDialog( {responseText : html } );
			this.close();
			return;
		}
		
		if(!iframe.uploadOutcome)
		{
			this.showMessage(iframe.uploadMessage);
			return;
		}
		
		jQuery("#" + this.frameId).unbind("load");
		
		if ( this.parentControl.typeName() == "DbNetFile" || this.column != null)
			this.parentControl.assignUpload(this.editField, iframe.uploadGuid,iframe.uploadUrl,iframe.uploadFileName);
		else
			this.parentControl.dataUploaded(iframe.uploadFileName);
	
		this.close();
		
		var args = {};
		args.fileName = iframe.uploadFileName
		args.fileSize = iframe.uploadFileSize
		if (this.column != null)
		    args.column = this.column;
		
		this.parentControl.fireEvent("onFileUploaded", args);
	}
});
