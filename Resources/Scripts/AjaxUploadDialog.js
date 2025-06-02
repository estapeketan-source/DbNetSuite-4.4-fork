var AjaxUploadDialog = Dialog.extend({
    init: function (parentControl) {
        this._super(parentControl, "upload");
        this.column = null;
        this.editField = null;
        this.currentFolder = null;
        this.uploadFrame = null;
        this.files = null;
        this.method = "ajax-upload-dialog";
    },

    build: function (response) {
        this.addContainer();
        this.configured = true;
        this.dialogContainer.html(response.html);
        this.configure();
    },

    open: function () {
        this.dialog.dialog("option", "width", 600);
        this.dialog.dialog("open");

        var disable = true;
        var overwrite = false;
        var rename = false;

        if (this.parentControl.typeName() == "DbNetFile") {
            disable = !this.parentControl.uploadOverwrite;
        }
        else if (this.column != null) {
            disable = !this.column.uploadOverwrite;
            rename = this.column.uploadRename;
            this.label = this.column.label;
            this.assignLabel();
        }
        else {
            overwrite = true;
        }

        this.dialog.find("form")[0].reset();
        this.dialog.find("#fileOverwrite").prop("disabled", disable).prop("checked", overwrite);

        var cells = this.dialog.find("td.rename")
        if (rename)
            cells.show();
        else
            cells.hide();

        if (disable && !rename)
            this.dialog.find("#optionsRow").hide();

        this.dialog.find(".upload-button").prop('disabled', true);
        this.dialog.find("#fileDataRow").hide();
        this.dialog.find("#progressBar").progressbar("value", 0);
    },

    selectFile: function () {
        this.dialog.find("#fileInput").change(this.createDelegate(this.fileSelected));
        this.dialog.find("#fileInput").click();
    },

    fileOver: function (e) {
        this.dialog.find('.drop-zone').addClass('hover');
        if (e.preventDefault)
            e.preventDefault();
    },

    fileLeave: function (e) {
        this.dialog.find('.drop-zone').removeClass('hover');
        if (e.preventDefault)
            e.preventDefault();
    },

    fileDropped: function (e) {
        this.dialog.find('.drop-zone').removeClass('hover');
        e.preventDefault();
        this.readFiles(e.target.files || e.dataTransfer.files);
    },

    fileSelected: function (event) {
        this.readFiles(event.target.files);
    },

    readFiles: function (files) {
        this.files = files;
        this.dialog.find("#fileDataRow").show();
        this.dialog.find(".upload-button").prop('disabled', false);

        var file = files[0];
        for (var p in file) {
            var val = file[p];
            if (p.replace(/Date/, '') == 'lastModified')
                val = DbNetLink.Util.dateToString(new Date(file[p]), null, true);
            this.dialog.find("#fileDataRow").find('#' + p.replace(/Date/, '')).text(val);
        }

        var maxFileSizeKb = 0;
        var extFilter = '';

        if (this.parentControl.typeName() == "DbNetFile") {
            maxFileSizeKb = this.parentControl.uploadMaxFileSizeKb;
            extFilter = this.parentControl.uploadFileTypes;
        }
        else if (this.column != null) {
            maxFileSizeKb = this.column.uploadMaxFileSize;
            extFilter = this.column.uploadExtFilter;
        }
        else {
            extFilter = this.parentControl.uploadExtFilter;
        }

        if (maxFileSizeKb > 0)
            if (maxFileSizeKb * 1024 < file.size) {
                this.showMessage(this.parentControl.translate('FileExceedsMaxSize').replace('{0}', maxFileSizeKb.toString()));
                this.dialog.find(".upload-button").prop('disabled', true);
                return;
            }

        if (extFilter != '') {
            var ext = file.name.split('.').pop().toLowerCase();
            var exts = extFilter.replace(/\./g, '').toLowerCase().split(',');

            if (jQuery.inArray(ext, exts) == -1) {
                this.showMessage(this.parentControl.translate('InvalidFileType'));
                this.dialog.find(".upload-button").prop('disabled', true);
                return;
            }
        }
                
        this.showThumbnail(file);
    },

    configure: function () {
        this._super();
        this.dialog.find(".upload-button").bind("click", this.createDelegate(this.upload));
        this.dialog.find(".cancel-button").bind("click", this.createDelegate(this.close));
        this.dialog.find("#selectFileBtn").click(this.createDelegate(this.selectFile));

        var dropZone = this.dialog.get()[0];

        dropZone.addEventListener("dragover", this.createDelegate(this.fileOver), false);
        dropZone.addEventListener("dragleave", this.createDelegate(this.fileLeave), false);
        dropZone.addEventListener("drop", this.createDelegate(this.fileDropped), false);

        this.dialog.find("#progressBar").progressbar();

        window.setTimeout(this.createDelegate(this.open), 100);
    },

    formElementById: function (id) {
        return this.dialog.find("#" + id);
    },

    upload: function () {
        var file = this.files[0].name;
        var data = {};
        data.fileName = file;
        data.fileNameWithoutExtension = file.split(".").slice(0, -1).join(".");
        data.fileNameExtension = file.split(".").pop();
        data.overwrite = this.formElementById("fileOverwrite").prop("checked");
        this.assignDataProperties(data);

        data.alternateFileName = this.formElementById("alternateFileName").val();
        this.parentControl.fireEvent("onBeforeFileUploadValidate", data);
        this.formElementById("alternateFileName").val(data.alternateFileName);

        this.parentControl.callServer("validate-upload", { method: this.validateUploadCallback, context: this }, data);
    },

    validateUploadCallback: function (response) {
        if (!response.ok) {
            this.showMessage(response.message);
            return;
        }

        var data = {
            method: "ajax-upload",
            data: this.files[0].name
        };

        this.assignDataProperties(data);

        //this.formElementById("data").val(JSON.stringify(data));

        var formData = new FormData();
        formData.append('data', JSON.stringify(data));
        formData.append('fileOverwrite', this.formElementById("fileOverwrite").prop('checked'));
        formData.append('alternateFileName', this.formElementById("alternateFileName").val());
        formData.append('fileInput', this.files[0]);

        var args = {};
        args.form = this.dialog;
        this.parentControl.fireEvent("onBeforeFileUploaded", args);
        this.parentControl.showWait();

        var updateProgress = this.createDelegate(this.updateProgress);

        jQuery.ajax({
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", updateProgress, false);
                return xhr;
            },
            url: this.parentControl.ajaxConfig.url,
            type: 'POST',
            data: formData,
            async: true,
            cache: false,
            contentType: false,
            processData: false,
            success: this.createDelegate(this.fileUploaded)
        });
    },

    updateProgress: function (evt) {
        if (evt.lengthComputable) {
            this.dialog.find("#progressBar").progressbar("value", evt.loaded / evt.total);
        }
    },

    assignDataProperties: function (data) {
        if (this.parentControl.typeName() == "DbNetFile") {
            data.currentFolder = this.parentControl.currentFolder;
            data.rootFolder = this.parentControl.rootFolder;
            data.uploadMaxFileSizeKb = this.parentControl.uploadMaxFileSizeKb;
            data.uploadFileTypes = this.parentControl.uploadFileTypes;
        }
        else if (this.column != null) {
            data.column = this.column;
        }
        else {
            data.uploadDataFolder = this.parentControl.uploadDataFolder;
            data.uploadExtFilter = this.parentControl.uploadExtFilter;
        }
    },

    fileUploaded: function (response) {
        response = jQuery.parseJSON(response);
        this.parentControl.hideWait();

        if (response.uploadMessage != '') {
            this.showMessage(response.message);
            return;
        }

        if (this.parentControl.typeName() == "DbNetFile" || this.column != null)
            this.parentControl.assignUpload(this.editField, response.uploadGuid, response.uploadUrl, response.uploadFileName);
        else
            this.parentControl.dataUploaded(response.uploadFileName);

        this.close();

        var args = {};
        args.fileName = response.uploadFileName;
        args.fileSize = response.uploadFileSize;
        if (this.column != null)
            args.column = this.column;

        this.parentControl.fireEvent("onFileUploaded", args);
    },

    showThumbnail: function (file) {
        var $row = this.dialog.find("#fileDataRow");
        var thumbnail = $row.find("#thumbnail");
        thumbnail.empty();

        var space = {}
        space.height = $row.height();
        space.width = $row.width() - $row.find("table").width();

        if (!file.type.match(/image.*/)) {
            return
        }

        var image = document.createElement("img");
        jQuery(image).load(function (e) {
            if (e.target.width > space.width || e.target.height > space.width) {
                var w = e.target.width;
                var h = e.target.height;
                var wf = space.width / w;
                var hf = space.height / h;

                if (hf < wf) {
                    jQuery(e.target).width(w * hf);
                    jQuery(e.target).height(h * hf);
                }
                else {
                    jQuery(e.target).width(w * wf);
                    jQuery(e.target).height(h * wf);
                }
            }
        })

        image.file = file;
        thumbnail.append(image)
        var reader = new FileReader()
        reader.onload = (function (aImg) {
            return function (e) {
                aImg.src = e.target.result;
            };
        }(image))
        var ret = reader.readAsDataURL(file);
        var canvas = document.createElement("canvas");
        ctx = canvas.getContext("2d");
        image.onload = function () {
            ctx.drawImage(image, 100, 100)
        }
    }

});