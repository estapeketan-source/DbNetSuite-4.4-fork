var DbNetSuite = Class.extend({
    init: function (id) {
        if (!id)
            id = "";
        this.id = id;
        this.componentId = id;
        this.displayWin = null;
        this.displayWindowFeatures = "top=100,left=100,toolbar=no,resizable=yes,scrollbars=yes,status=no,height=400,width=800";
        this.initialized = false;
        this.caseInsensitiveSearch = false;
        this.connectionString = "";
        this.commandTimeout = 30;
        this.callbackIndex = 0;
        this.error = false;
        this.callbackList = new Object();
        this.mb = null;
        this.messageLine = null;
        this.errorDialog = null;
        this.childControls = [];
        this.parentControls = [];
        this.serverControl = false;
        this.serverId = "";
        this.messageTimeout = 3;
        this.eventHandlers = {};
        this.buttonText = {};
        this.theme = "";
        this.toolbar = null;
        this.userLanguage = null;
        this.cultureName = null;
        this.timezoneOffset = new Date().getTimezoneOffset();
        this.requestToken = DbNetLink.requestToken;
        this.browser = jQuery.browser;

        if (id != "") {
            this.container = jQuery("#" + id);
            jQuery(this.container).addClass("dbnetsuite");
            if (this.container.length == 0) {
                if (this instanceof DbNetSpell) {
                    this.container = this.addDOMElement(id);
                    this.container.show();
                }
                else if (this instanceof DbNetGrid || this instanceof DbNetEdit || this instanceof DbNetFile || this instanceof DbNetList || this instanceof DbNetCombo) {
                    alert("Component container [ID=" + id + "] not found")
                }
            }
            else if (this.container.attr("ServerID")) {
                this.serverId = this.container.attr("ServerID");
                DbNetLink.components[this.serverId] = this;
            }
        }
        this.ajaxConfig = {
            type: "POST",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: this.createDelegate(this.openErrorDialog)
        };
        this.previewDialogHeight = window.screen.availHeight * 0.8;
        this.previewDialogWidth = window.screen.availWidth * 0.8;

        if (this instanceof DbNetGrid || this instanceof DbNetEdit || this instanceof DbNetFile) {
            this.filePreviewDialog = new FilePreviewDialog(this);
            if (window.FormData === undefined || window.FileReader === undefined || window.AjaxUploadDialog === undefined || document.addEventListener === undefined)
                this.uploadDialog = new UploadDialog(this);
            else
                this.uploadDialog = new AjaxUploadDialog(this);
        }

        if (!(this instanceof Dialog) && this.componentId != "")
            this.mb = new MessageBox(this);
        else if (this instanceof SearchPanel)
            this.mb = new MessageBox(this);

        this.scrollbarWidth = null;
    },

    ajaxDataProperties: function () {
        return ["id", "connectionString", "commandTimeout", "callbackIndex", "timezoneOffset", "serverId", "requestToken", "userLanguage", "browser", "theme", "caseInsensitiveSearch"];
    },

    profileProperties: function () {
        return ["id"];
    },

    $: function (id, searchDoc) {
        if (typeof (id) == "string") {
            var e = jQuery("#" + this.componentId + ((id.indexOf(".") != 0) ? "_" : " ") + id);
            if (e.length == 0 && id != "" && searchDoc)
                e = jQuery("#" + id);
            return e;
        }
        else
            return jQuery(id);
    },

    callServer: function (method, callback, data) {
        if (this.error)
            return;

        var config = { "data": {} };

        for (var i = 0; i < this.ajaxDataProperties().length; i++) {
            var p = this.ajaxDataProperties()[i];
            config.data[p] = this[p];
        }

        if (data)
            for (var p in data)
                config.data[p] = data[p];

        this.callbackIndex++;
        this.serverControl = this.isServerControl();

        for (var i in config)
            if (i == "data") {
                config.data.method = method;
                config.data.callbackIndex = this.callbackIndex;
                this.ajaxConfig[i] = DbNetLink.Util.stringify(config[i]);
            }
            else
                this.ajaxConfig[i] = config[i];


        if (callback != null) {
            this.ajaxConfig.success = this.createDelegate(this.ajaxSuccess);
            this.callbackList[this.callbackIndex] = callback
        }
        else
            this.ajaxConfig.success = function () { };

        this.ajaxConfig.async = (callback != null);
        this.ajaxConfig.headers = { 'method': method }

        var xhr = jQuery.ajax(this.ajaxConfig);

        if (callback != null)
            return;

        if (xhr.status != 200) {
            this.openErrorDialog(xhr);
            return null
        }
        var data;

        try {
            data = JSON.parse(xhr.responseText);
        }
        catch (ex) {
            this.error = true;
            this.openErrorDialog(xhr);
            return null;
        }

        return data;
    },

    ajaxSuccess: function (data) {
        for (var p in data.clientProperties)
            this[p] = data.clientProperties[p];
        var cb = this.callbackList[data.callbackIndex];
        var delegate;
        var params = [data];
        var context = this;
        if (cb.context)
            context = cb.context;
        if (cb.method)
            delegate = this.createDelegate(cb.method, context);
        else
            delegate = this.createDelegate(cb);
        if (cb.params)
            params = params.concat(cb.params);

        if (data.licenseMessage)
            if (data.licenseMessage != "") {
                this.messageLine = null;
                this.showMessage(data.licenseMessage);
            }
        delegate.apply(this, params);
        this.callbackList[data.callbackIndex] = null;
    },

    ajaxError: function (xhr) {
        var win = window.open("about:blank");
        win.document.open();
        win.document.write(xhr.responseText);
        win.document.close();
    },

    openErrorDialog: function (xhr) {
        if (xhr.status == 0)
            return;
        this.errorDialog = new ErrorDialog(this);

        var args = { xhr: xhr };
        this.fireEvent("onBeforeErrorDialogOpened", args);

        this.errorDialog.text = xhr.responseText;
        this.errorDialog.status = xhr.status;
        this.errorDialog.build();
    },

    parentElement: function (e, tagName) {
        return (this.$(e).get(0).tagName != tagName.toUpperCase()) ? this.$(e).parents(tagName).get(0) : e;
    },

    translate: function (key) {
        if (DbNetSuiteText[key])
            return DbNetSuiteText[key];
        else
            return key;
    },

    isServerControl: function (key) {
        return (this.serverId != "");
    },

    addDOMElement: function (id, container, elem) {
        if (this.$(id).length > 0)
            return this.$(id);
        if (!container)
            container = jQuery("body");
        if (!elem)
            elem = "div";

        return jQuery(document.createElement(elem)).attr("id", this.componentId + "_" + id).attr("dependencyId", this.componentId).hide().appendTo(jQuery(container));
    },

    showMessage: function (message, interval) {
        if (typeof (interval) != "number")
            interval = this.messageTimeout * 1000;

        if (interval < 100)
            interval = interval * 1000;

        if (this.messageLine == null)
            this.messageBox("info", message);
        else {
            this.$(this.messageLine).html(message);
            this.$(this.messageLine).addClass("highlight");
        }
        window.clearTimeout(this.clearMessageTimeout);
        this.clearMessageTimeout = window.setTimeout(this.createDelegate(this.clearMessage), interval);
    },

    clearMessage: function () {
        if (this.messageLine == null)
            this.mb.close();
        else {
            this.$(this.messageLine).html("&nbsp;");
            this.$(this.messageLine).removeClass("highlight");
        }
    },

    clone: function (o) {
        return jQuery.extend(true, {}, o);
    },

    messageBox: function (type, message, callback) {
        this.mb.type = type;
        this.mb.message = message;
        this.mb.callback = callback;

        this.openDialog(this.mb, "message-box");
    },

    deserializeRecord: function (r) {
        if (jQuery.isArray(r))
            for (var i = 0; i < r.length; i++)
                r[i] = this.deserializeRecord(r[i]);
        else
            for (var key in r)
                r[key] = this.deserialize(r[key]);

        return r;
    },

    deserialize: function (data) {
        if (typeof (data) == "string")
            if (data.match(/\/Date\((-|)\d{1,}\)\//) != null)
                return eval('new ' + data.replace(/\//g, ''));
        return data;
    },

    createDelegate: function (method, context) {
        if (!context)
            context = this;

        return function () {
            var args = Array.prototype.slice.apply(arguments);
            args.push(this);
            method.apply(context, args);
        }
    },

    createDelegateCallback: function (method, context) {
        var instance = this;
        return function () {
            var l = arguments.length;

            if (l > 3) {
                var args = [];

                for (var i = 2; i < l; i++) {
                    args[i - 2] = arguments[i];
                }

                return method.apply(instance, args);
            }

            return method.call(instance, context);
        };
    },

    addHandler: function (event, handler) {
        this.bind(event, handler);
    },

    bind: function (event, handler) {
        if (!this.eventHandlers[event])
            this.eventHandlers[event] = [];
        this.eventHandlers[event].push(handler);
    },

    unbind: function (event, handler) {
        if (!handler)
            this.eventHandlers[event] = [];
        else {
            if (this.eventHandlers[event] == null)
                return;

            for (var i = 0; i < this.eventHandlers[event].length; i++)
                if (this.eventHandlers[event][i] == handler)
                    this.eventHandlers[event][i] = null;
        }
    },

    fireEvent: function (event) {
        if (!this.eventHandlers[event])
            return false;

        //debug.log(this, event + " before " + this.eventHandlers[event].length.toString());

        var events = this.eventHandlers[event];

        for (var i = 0; i < events.length; i++) {
            var method = events[i];
            //debug.log(this, i.toString() + " " + method.toString());
            if (typeof (method) == "string") {
                if (typeof (window[method]) == "function")
                    method = window[method];
                else
                    alert("Client-side method '" + method + "' not found")
            }
            var args = [this];
            if (arguments.length > 1)
                args = args.concat(Array.prototype.slice.call(arguments, 1));
            method.apply(window, args);
        }

        return true;

        //debug.log(this, event + " after " + this.eventHandlers[event].length.toString());
    },

    invokeHandler: function (idx, method) {
    },

    executeSingletonQuery: function (sql, params) {
        var data = this.executeQuery(sql, params);
        return jQuery.isArray(data) ? data[0] : null;
    },

    executeQuery: function (sql, params) {
        if (!params)
            params = {};
        var args = { sql: sql, params: params };
        var resp = this.callServer("execute-query", null, args);

        if (resp == null)
            return null;

        if (resp.data.length == 0)
            return null;

        if (typeof (resp.data) == "string") {
            return null;
        }

        for (var i = 0; i < resp.data.length; i++)
            resp.data[i] = this.deserializeRecord(resp.data[i]);

        return resp.data;
    },

    typeName: function () {
        if (this instanceof DbNetGrid)
            return "DbNetGrid"
        else if (this instanceof DbNetEdit)
            return "DbNetEdit";
        else if (this instanceof DbNetFile)
            return "DbNetFile";
        alert("Unknown type");
    },

    isObject: function (o) {
        return (o != null && typeof (o) == "object" && o.constructor.toString() == Object.toString());
    },

    isEmptyObject: function (ob) {
        for (var i in ob) { if (ob.hasOwnProperty(i)) { return false; } }
        return true;
    },

    uninitializedControlId: function () {
        var initialized = "";
        for (var i = 0; i < this.childControls.length; i++) {
            if (!this.childControls[i].ctrl.initialized)
                initialized = this.childControls[i].ctrl.id;
            else {
                var id = this.childControls[i].ctrl.uninitializedControlId();
                if (id != "")
                    initialized = id;
            }
        }

        return initialized;
    },

    invokeCallback: function (callback, params) {
        if (!params)
            params = [];
        if (callback.params)
            params = params.concat(callback.params);
        if (callback.method)
            callback.method.apply(this, params);
        else
            callback.apply(this, params);

    },

    configureNavigation: function () {
        this.$("totalPages").val(this.totalPages);

        var factor = 1.5;

        if (this.theme.toLowerCase() == "bootstrap")
            factor = 2;

        var emWidth = parseFloat(this.totalPages.toString().length * factor).toString() + "em";
        this.$("totalPages").css("width", emWidth);
        this.$("pageSelect").css("width", emWidth);

        this.$('totalRows').val(this.totalRows);
        this.$('totalRows').css("width", parseFloat(this.totalRows.toString().length * factor).toString() + "em");

        this.$('pageSelect').val(this.currentPage);

        this.$("nextBtn").prop("disabled", (this.currentPage >= this.totalPages));
        this.$("lastBtn").prop("disabled", (this.currentPage >= this.totalPages));
        this.$("firstBtn").prop("disabled", (this.currentPage <= 1));
        this.$("prevBtn").prop("disabled", (this.currentPage <= 1));
    },

    navigate: function (event) {
        var id = this.parentElement(event.target, "button").getAttribute("buttonType");
        switch (id) {
            case "first":
            case "prev":
                if (this.currentPage <= 1)
                    return;
                break;
            case "next":
            case "last":
                if (this.currentPage >= (this.totalPages))
                    return;
                break;
        }

        if (this instanceof DbNetFile)
            this.navigateCallback("yes", id);
        else
            this.hasUnappliedChanges({ method: this.createDelegate(this.navigateCallback), params: [id] });
    },

    navigateCallback: function (buttonPressed, action) {
        if (buttonPressed != "yes")
            return;

        switch (action) {
            case "first":
                this.currentPage = 1;
                break;
            case "prev":
                this.currentPage--;
                break;
            case "next":
                this.currentPage++;
                break;
            case "last":
                this.currentPage = (this.totalPages);
                break;
        }

        this.loadData();
    },

    pageSelectChanged: function (event) {
        var page = parseInt(this.$("pageSelect").val());

        if (page < 1)
            page = 1;

        if (page > this.totalPages)
            page = this.totalPages;

        if (this instanceof DbNetFile)
            this.pageSelectOnChangeCallback("yes");
        else if (page != this.currentPage)
            this.hasUnappliedChanges(this.createDelegate(this.pageSelectOnChangeCallback));
    },

    pageSelectOnChangeCallback: function (buttonPressed) {
        if (buttonPressed == "yes") {
            this.currentPage = parseInt(this.$("pageSelect").val());
            this.loadData();
        }
        else {
            this.$("pageSelect").val(this.currentPage);
        }
    },

    pageSelectKeydown: function (event) {
        var delegate = this.createDelegate(this.pageSelectChanged);
        var key = event.keyCode;
        switch (key) {
            case 46:
            case 48:
            case 49:
            case 50:
            case 51:
            case 52:
            case 53:
            case 54:
            case 55:
            case 56:
            case 57:
            case 96:
            case 97:
            case 98:
            case 99:
            case 100:
            case 101:
            case 102:
            case 103:
            case 104:
            case 105:
                window.clearTimeout(this.numberTypingTimer);
                this.numberTypingTimer = window.setTimeout(delegate, 800);
                break;

            case 13:
            case 9:
                window.clearTimeout(this.numberTypingTimer);
                this.numberTypingTimer = window.setTimeout(delegate, 1);
                return true;

            case 8:
                window.clearTimeout(this.numberTypingTimer);
                if (event.target.value.length == 1)
                    this.numberTypingTimer = window.setTimeout(delegate, 3000);
                else
                    this.numberTypingTimer = window.setTimeout(delegate, 800);
                break;

            case 35:
            case 36:
            case 37:
            case 39:
                window.clearTimeout(this.numberTypingTimer);
                return true;

            default:
                return false;
        }
    },

    setColumnLabels: function (values) {
        /*
        if (this instanceof DbNetEdit)
            if (this.placeHolders.length > 0)
                return;
        */
        if (arguments.length > 1)
            values = [values].concat(Array.prototype.slice.call(arguments, 1));

        if (jQuery.isArray(values))
            values = values.slice(0, this.columns.length);
        this.setColumnsProperty(values, "label");
    },

    setColumnProperty: function (cn, property, value) {
        if (cn == "*" && this.columns.length > 0)
            for (var i = 0; i < this.columns.length; i++)
                this._setColumnProperty(i, property, value);
        else
            this._setColumnProperty(cn, property, value);
    },

    _setColumnProperty: function (cn, property, value) {
        var idx = cn;

        if (jQuery.isArray(value))
            value = DbNetLink.Util.stringify(value);

        if (jQuery.isArray(idx)) {
            for (var i = 0; i < idx.length; i++)
                this._setColumnProperty(idx[i], property, value);
            return;
        }
        else if (this.isObject(property)) {
            this.setColumnProperties(idx, property);
            return;
        }

        if (typeof (idx) != "number")
            idx = this.columnIndex(idx);

        if (idx < 0) {
            if (!this.columnProperties[cn])
                this.columnProperties[cn] = {};
            this.columnProperties[cn][property] = value
            return;
        }
        if (idx > this.columns.length) {
            alert("Column index exceeds number of columns");
            return;
        }

        if (idx == this.columns.length) {
            var o = this.newColumn();
            o[property] = value;
            this.columns.push(o);
            return;
        }

        property = this.validateColumnPropertyName(property);

        this.columns[idx][property] = value;
    },

    validateColumnPropertyName: function (property) {
        var p = property.toString().toLowerCase();
        for (var i = 0; i < this.columnPropertyNames.length; i++)
            if (p == this.columnPropertyNames[i].toLowerCase())
                return this.columnPropertyNames[i];

        alert("Column property [" + property + "] is not valid");
        return property;
    },

    setColumnProperties: function (c, properties) {
        for (p in properties)
            this._setColumnProperty(c, p, properties[p]);
    },

    getColumn: function (cn) {
        var idx = this.columnIndex(cn);
        if (idx == -1)
            return null;
        else
            return this.columns[idx];
    },

    columnNameByProperty: function (p) {
        for (var i = 0; i < this.columns.length; i++)
            if (this.columns[i][p])
                return this.columns[i].columnExpression;
        return "";
    },

    columnIndex: function (cn) {
        var cn = cn.toLowerCase();

        if (this instanceof DbNetGrid) {
            if (typeof this.columnIndexCache[cn] != "undefined") {
                return this.columnIndexCache[cn];
            }
        }

        var idx = this.getColumnIndex(cn);

        if (this instanceof DbNetGrid)
            this.columnIndexCache[cn] = idx;

        return idx;
    },

    cellIndex: function (cn, row) {
        var idx = this.getColumnIndex(cn);

        if (row) {
            if ($(row).hasClass('aggregate-row')) {
                var i = -1;
                var j = 0;
                $(row).children().each(function () {
                    var colspan = 1;
                    var attr = $(this).attr('colspan');
                    if (typeof attr !== typeof undefined && attr !== false) {
                        colspan = parseInt(attr);
                    }
                    i += colspan;

                    if (i >= idx) {
                        return;
                    }
                    j++;
                })

                return j;
            }
        }

        var idx = this.getColumnIndex(cn);
        for (var i = 0; i < idx; i++) {
            if (i < idx) {
                if (this.columns[i].display == false) {
                    idx--;
                }
            }
        }
        return idx;
    },

    getColumnIndex: function (cn) {
        var pns = (this instanceof DbNetFile) ? ["columnType"] : ["columnExpression", "columnName"];

        for (var i = 0; i < this.columns.length; i++) {
            for (var j = 0; j < pns.length; j++) {
                var pn = pns[j];
                var cp = this.columns[i][pn];
                if (!cp)
                    continue;
                var c = cp.toLowerCase();
                if (c == cn)
                    return i;
                c = c.split(".").pop();
                if (c == cn)
                    return i;
                if (c.split(" ").pop() == cn)
                    return i;
            }
        }

        return -1;
    },

    resetProperty: function (value, property) {
        for (var i = 0; i < this.columns.length; i++)
            this._setColumnProperty(i, property, value);
    },

    setColumnsProperty: function (values, property) {
        switch (typeof (values)) {
            case "string":
                values = values.split(",");
                break;
        }

        if (!jQuery.isArray(values)) {
            alert("Arguments must be an array or comma separated string");
            return;
        }

        for (var i = 0; i < values.length; i++)
            this._setColumnProperty(i, property, values[i]);

    },

    displayDocument: function (filePath) {
        var url = filePath;
        if (this instanceof DbNetFile)
            url = this.streamUrl(filePath) + "&rootfolder=" + encodeURIComponent(this.rootFolder);

        this.fireEvent("onBeforeDocumentDisplayed");

        if (this.displayWin)
            if (!this.displayWin.closed) {
                this.displayWin.location.href = url;
                this.displayWin.focus();
                return;
            }
        this.displayWin = window.open(url, this.componentId + "displayWindow", this.displayWindowFeatures);
    },

    addDatePicker: function (e, col) {
        var options = {
            showOn: "none",
            onSelect: function (text, inst) {
                jQuery(this).trigger("keyup").trigger("change");
                return true;
            }
        };
        var formats = { D: "DD, MM dd, yy", DDDD: "DD", DDD: "D", MMMM: "MM", MMM: "M", M: "m", MM: "mm", yyyy: "yy" };
        if (col) {
            var fmt = col.format;
            let pattern: keyof typeof formats;
            for (var pattern in formats) {
                var re = new RegExp("\\b" + pattern + "\\b");
                fmt = fmt.replace(re, formats[pattern]);
            }
            if (fmt != undefined)
                if (fmt != col.format)
                    options.dateFormat = fmt;

            console.log('format:' + col.searchFormat);
        }
        options.changeMonth = true;
        options.changeYear = true;
        options.constrainInput = false;

        var format = e.attr("format");

        if (format)
            if (format.toLowerCase() == "g")
                if (jQuery().datetimepicker) {
                    options.showSecond = (format == "G");
                    options.timeFormat = (format == "G") ? "hh:mm:ss" : "hh:mm";
                    e.datetimepicker(options);
                    return;
                }
        e.datepicker(options);
    },

    addToolbarButton: function (i) {
        return this.addToolbarElement(i, "button");
    },

    addToolbarElement: function (i, element) {
        if (typeof i == "undefined")
            i = -1;
        var t = this.$("toolbar")[0];
        var btn = jQuery(t).find('button:first');
        var btnHeight = parseInt(btn.css('height') || btn.height());
        var c;
        try {
            c = t.rows[0].insertCell(i);
        }
        catch (e) {
            c = t.rows[0].insertCell(-1);
        }
        var b = document.createElement(element);
        c.appendChild(b);
        b.className = "custom";
        if (element == "button") {
            b.style.padding = "0px"
            b.setAttribute("type", "button");
            jQuery(b).css('height', btnHeight.toString() + 'px').addClass("btn");
        }
        return b;
    },

    toolbarElement: function (id, silent) {
        var e = null;
        switch (id) {
            case "outputPageSelect":
            case "userProfileSelect":
            case "quickSearch":
                e = this.$(id);
                break;
            default:
                e = this.$(id + "Btn");
                break;
        }

        if (e.length > 0)
            return e[0];

        e = this.$(id + "RowBtn");

        if (e.length > 0)
            return e[0];

        if (!silent)
            alert("Toolbar element '" + id + "' not found");
        return null;
    },

    toolButtonText: function (id, text) {
        var b = this.toolbarElement(id, true);
        if (b) {
            jQuery(b).css("width", "").find(".tool-button-text").html("&nbsp;" + text).css("width", "").css("padding-left", "20px");
        }
        else
            this.buttonText[id] = text;
    },


    openUploadDialog: function (data) {
        for (p in data)
            this.uploadDialog[p] = data[p];
        this.openDialog(this.uploadDialog, this.uploadDialog.method);
    },


    openFilePreviewDialog: function (url, args) {
        this.filePreviewDialog.url = url;
        var data = {};

        if (args)
            data = args;
        else
            args = {};

        if (args.inline) {
            window.open(url + "&inline=true");
            return;
        }
        else {
            if (url.indexOf("data:") == 0 || url.indexOf("method=image-data") > -1) {
                if (args.isImage)
                    this.openFilePreviewDialogCallback({ documentIsImage: data.isImage, documentHeight: data.height, documentWidth: data.width, fileName: data.fileName });
                else
                    document.location.href = url;
                return;
            }
        }

        try {
            if (this instanceof DbNetFile)
                data["filepath"] = url;
            else {
                var params = url.split("?")[1].split("&")

                for (var i = 0; i < params.length; i++)
                    data[params[i].split("=")[0]] = params[i].split("=")[1];
            }
        }
        catch (ex) {
            this.openFilePreviewDialogCallback();
            return;
        }

        this.callServer("document-size", this.openFilePreviewDialogCallback, data);
    },

    openFilePreviewDialogCallback: function (response) {
        var maxH = screen.availHeight * 0.6;
        var maxW = screen.availWidth * 0.8;

        var h = maxH;
        var w = maxW;
        var isImage = false;

        if (response) {
            if (response.documentIsImage) {
                h = response.documentHeight;
                w = response.documentWidth;
                isImage = true;
            }

            if (response.fileName)
                this.filePreviewDialog.data.fileName = response.fileName;
        }

        if (h < 150)
            h = 150
        else if (h > maxH)
            h = maxH;
        if (w < 150)
            w = 150;
        else if (w > maxW)
            w = maxW;

        this.filePreviewDialog.data.frameWidth = w;
        this.filePreviewDialog.data.frameHeight = h;
        this.filePreviewDialog.data.isImage = isImage;

        this.openDialog(this.filePreviewDialog, "preview-dialog");
    },

    openDialog: function (dialog, method) {
        if (!dialog.configured)
            this.callServer(method, { method: dialog.build, context: dialog }, dialog.data);
        else
            dialog.open();
    },

    assignHandler: function (h) {
        if (dbnetsuite.applicationPath)
            h = dbnetsuite.applicationPath + h;
        return h;
    },

    assignAjaxUrl: function (h) {
        this.ajaxConfig.url = this.assignHandler(h);
    },

    streamUrl: function (filePath) {
        return this.ajaxConfig.url + "?id=" + this.componentId + "&method=stream&filepath=" + encodeURIComponent(filePath);
    },

    loadCombo: function (combo, items, addEmptyOption, emptyOptionText, value) {
        DbNetLink.Util.loadCombo(combo, items, addEmptyOption, emptyOptionText, value);
    },

    getScrollbarWidth: function () {
        if (this.scrollbarWidth)
            return this.scrollbarWidth;

        var div = jQuery('<div style="width:50px;height:50px;overflow:hidden;position:absolute;top:-200px;left:-200px;"><div style="height:100px;"></div>');
        jQuery('body').append(div);
        var w1 = jQuery('div', div).innerWidth();
        div.css('overflow-y', 'scroll');
        var w2 = jQuery('div', div).innerWidth();
        jQuery(div).remove();
        this.scrollbarWidth = (w1 - w2);
        return this.scrollbarWidth;
    },

    showWait: function () {
        if (this.$("waitText").length == 0) {
            var e = this.addDOMElement("waitText", jQuery("body"));
            e.html("&nbsp;").addClass("dbnetsuite-wait-message");
        }

        this.$("waitText").show();
    },

    hideWait: function () {
        this.$("waitText").hide();
    },

    showWaiting: function (panel) {
        panel.show();
        if (this.$("waitPanel").length == 0) {
            this.addDOMElement("waitPanel", this.container);
            this.$("waitPanel").html("&nbsp;").width(20).height(20).addClass("waiting");
        }
        if (panel.html() == "")
            this.$("waitPanel").show().css({ position: "relative" });
        else {
            var pos = panel.position()
            var top = pos.top + parseInt((panel.children(":first").height() - 20) / 2)
            var left = pos.left + parseInt((panel.children(":first").width() - 20) / 2)
            this.$("waitPanel").show().css({ position: "absolute", top: top, left: left });
        }
    }

});