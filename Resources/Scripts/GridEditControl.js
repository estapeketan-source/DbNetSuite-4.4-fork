var GridEditControl = DbNetSuite.extend({
    init: function (id) {
        this._super(id);
        this.audit = "none";
        this.auditDateFormat = "d";
        this.auditDialog = new AuditDialog(this);
        this.auditUser = "";
        this.autoSave = false;
        this.autoSuggest = true;
        this.allowLinkedDeletion = false;
        this.advancedSearchDialog = new AdvancedSearchDialog(this);
        this.advancedSearchFilterJoin = "and";
        this.advancedSearch = true;
        this.columns = [];
        this.columnProperties = {};
        this.columnSortDialog = new ColumnSortDialog(this);
        this.confirmDeletion = true;
        this.currentPage = -1;
        this.customProfileProperties = [];
        this.dataUploadDialog = new DataUploadDialog(this);
        this.dbNetSpell = new DbNetSpell(id + "_dbnetspell");
        this.configureToolbarptyOptionText = "";
        this.fromPart = "";
        this.fixedFilterSql = "";
        this.fixedFilterParams = {};
        this.htmlEditor = new HtmlEditor(this);
        this.inputControls = {};
        this.ignorePrimaryKeys = false;
        this.lookupDialog = new LookupDialog(this);
        this.multiValueLookupSelectStyle = "";
        this.navigation = true;
        this.orderBy = "";
        this.parentControl = null;
        this.parentFilterSql = [];
        this.parentFilterParams = {};
        this.primaryKeyList = [];
        this.profileKey = window.location.pathname.replace(/\W/g, "_") + "_" + this.componentId;
        this.profileUser = "";
        this.quickSearch = false;
        this.quickSearchDelay = 1000;
        this.quickSearchMinChars = 3;
        this.quickSearchTimerId = null;
        this.rowInfo = true;
        this.reloadAfterUpdate = true;
        this.search = true;
        this.searchDialog = new SearchDialog(this);
        this.searchFilter = [];
        this.advancedSearchFilterJoin = "and";
        this.searchFilterSql = [];
        this.searchFilterText = "";
        this.searchFilterParams = {};
        this.searchLayoutColumns = 1;
        this.searchValuesOnly = false;
        this.searchPanelId = "";
        this.simpleSearch = true;
        this.simpleSearchDialog = new SimpleSearchDialog(this);
        this.searchCriteriaStore = {};
        this.searchDialogHeight = "";
        this.searchDialogMode = "Standard";
        this.sort = false;
        this.standardSearch = true;
        this.textEditor = new TextEditor(this);
        this.toolbarLocation = "top";
        this.toolbarButtonStyle = "image";
        this.totalPages = 0;
        this.totalRows = 0;
        this.updateRow = true;
        this.upload = false;
        this.uploadExtFilter = "";
        this.uploadDataFolder = "";
        this.uploadDataTable = "";
        this.insertRow = true;
        this.deleteRow = true;
        this.searchColumnOrder = 0;
        this.spellCheck = false;
        this.userProfile = false;
        this.userProfileDialog = new UserProfileDialog(this);
        this.userProfileSelect = false;
        this.profileOnLoad = null;

        jQuery("[dependencyId=" + id + "]").remove();
    },

    ajaxDataProperties: function (values) {
        return this._super().concat([
			"advancedSearch",
			"advancedSearchFilterJoin",
			"audit",
			"auditDateFormat",
			"auditUser",
			"columns",
			"columnProperties",
			"currentPage",
			"deleteRow",
            "emptyOptionText",
			"insertRow",
			"ignorePrimaryKeys",
			"fixedFilterSql",
			"fixedFilterParams",
			"fromPart",
			"navigation",
			"orderBy",
			"parentFilterSql",
			"parentFilterParams",
			"profileKey",
			"profileUser",
			"quickSearch",
			"rowInfo",
			"search",
			"searchColumnOrder",
			"searchDialogHeight",
			"searchDialogMode",
			"searchFilter",
			"searchFilterSql",
			"searchFilterParams",
			"searchLayoutColumns",
			"searchValuesOnly",
			"searchPanelId",
			"simpleSearch",
			"sort",
			"spellCheck",
			"toolbarLocation",
			"toolbarButtonStyle",
			"updateRow",
			"upload",
			"uploadDataFolder",
			"uploadDataTable",
			"userProfile",
			"userProfileSelect"
        ]);
    },

    profileProperties: function (values) {
        return this._super().concat([
			"searchDialogMode",
			"searchFilter",
			"fixedFilterSql",
			"fixedFilterParams",
			"searchCriteriaStore",
			"orderBy",
			"advancedSearchFilterJoin"
        ]);
    },

    initialize: function (parentControl) {
        this.advancedSearchDialog.options.width = 500;
        /*
        if (jQuery.browser.webkit) {
            this.searchDialogHeight = "";
            this.editDialogHeight = "";
        }
        */
        if (this.searchPanelId != "")
            this.search = false;

        this.bind("onBeforePageLoaded", this.createDelegate(this.beforeGetRecordSet));
    },

    beforeGetRecordSet: function () {
        var f = ["searchFilter"];

        for (var i = 0; i < f.length; i++) {
            if (typeof (this[f[i]]) == "string")
                this[f[i]] = [{ sql: this[f[i]] }];
            if (!jQuery.isArray(this[f[i]]))
                this[f[i]] = [this[f[i]]];
        }

        if (typeof (this.searchFilterSql) == "string")
            this.searchFilterSql = [this.searchFilterSql];
        if (typeof (this.columnFilterSql) == "string")
            this.columnFilterSql = [this.columnFilterSql];

    },

    setColumnExpressions: function (values) {
        /*
        if (this instanceof DbNetEdit)
        if (this.placeHolders.length > 0)
        return;
        */

        if (arguments.length > 1)
            values = [values].concat(Array.prototype.slice.call(arguments, 1));

        this.setColumnsProperty(values, "columnExpression");
    },

    newColumn: function () {
        var column = {};
        column.display = true;
        column.filter = false;
        column.search = true;
        column.edit = true;
        return column;
    },

    configureToolbar: function () {
        var dis = (this.totalPages == 0);

        this.$("updateRowBtn").prop("disabled", dis);
        this.$("deleteRowBtn").prop("disabled", dis);
        this.$("viewBtn").prop("disabled", dis);

        for (var i = 0; i < this.parentControls.length; i++) {
            var pc = this.parentControls[i];
            if (pc.ctrl.totalRows > 0 && !pc.oneToOne && !pc.ctrl.allowLinkedDeletion)
                pc.ctrl.$("deleteRowBtn").prop("disabled", (this.totalPages > 0));
        }

        this.$("deleteRowBtn").prop("disabled", dis);
        this.$("applyBtn").prop("disabled", dis);
        this.$("cancelBtn").prop("disabled", dis);

        disabled = "";
        if (this.parentControls.length > 0)
            if (this.parentControls[0].ctrl.isEmpty()) {
                if (!this.parentControls[0].oneToOne)
                    disabled = "disabled";
                else {
                    if (this.parentControls[0].ctrl.parentControls.length > 0)
                        if (this.parentControls[0].ctrl.parentControls[0].ctrl.isEmpty())
                            disabled = "disabled";
                }
            }

        dis = (disabled == "disabled");

        this.$("insertRowBtn").prop("disabled", dis);
        this.$("uploadBtn").prop("disabled", dis);
        this.$("saveBtn").prop("disabled", dis);
        this.$("printBtn").prop("disabled", dis);
        this.$("copyBtn").prop("disabled", dis);

        this.configureNavigation();
        this.fireEvent("onToolbarConfigured");
    },

    isEmpty: function () {
        if (this.totalRows == 0)
            return true;

        if (this instanceof DbNetEdit)
            return (this.mode == "insert")
        else
            return false;
    },

    hasUnappliedChanges: function (callback) {
        if (this.recordHasChanged())
            this.notifyChanges(callback);
        else
            this.invokeCallback(callback, ["yes"]);
    },

    notifyChanges: function (callback, params) {
        if (this.checkAutosave(callback, params))
            return;

        this.messageBox("question", "UnappliedChanges", callback);
    },

    checkAutosave: function (callback, params) {
        if (this.autoSave == false) {
            return false;
        }

        if (this instanceof DbNetGrid) {
            if (this.rowModified()) {
                if (this.childControls.length == 1) {
                    var editControl = this.childControls[0].ctrl;
                    if (editControl.saveAutomatically(this.createDelegate(this.controlSaved), this, { callback: callback })) {
                        return true;
                    }
                }
            }
            if (jQuery.isArray(this.inputControls) && this.inputControls.length > 0) {
                if (this.saveAutomatically(this.createDelegate(this.controlSaved), this, { callback: callback })) {
                    return true;
                }
            }
        }

        if (this instanceof DbNetEdit) {
            if (this.modifiedInputControls()) {
                if (this.saveAutomatically(this.createDelegate(this.controlSaved), this, { callback: callback })) {
                    return true;
                }
            }
        }

        return false;
    },

    controlSaved: function (params) {
        this.invokeCallback(params.callback, ["yes"]);
    },

    quickSearchKeyPress: function (e) {
        window.clearTimeout(this.quickSearchTimerId);

        if (e.target.value.length >= this.quickSearchMinChars || e.target.value.length == 0)
            this.quickSearchTimerId = window.setTimeout(this.createDelegate(this.runQuickSearch), this.quickSearchDelay);
    },

    runQuickSearch: function () {
        this.runSimpleSearch(this.$("quickSearch")[0].value);
    },

    showSimpleSearch: function () {
    },

    runSimpleSearch: function (token) {
        var o = this.simpleSearchDialog;
        o.searchFilter = {};
        o.searchFilter.sql = "{simplesearchsql}";
        o.searchFilter.params = { "simpleSearchToken": token };
        o.filterText = function () { return token };
        o.searchCriteriaStore = token;

        this.applySearch(o);
    },

    openSearchDialog: function () {
        switch (this.searchDialogMode.toLowerCase()) {
            case "simple":
                this.openSimpleSearchDialog();
                break;
            case "advanced":
                this.openAdvancedSearchDialog();
                break;
            default:
                this.openStandardSearchDialog();
                break;
        }
    },

    openStandardSearchDialog: function (event) {
        this.searchDialogMode = "Standard";
        this.openDialog(this.searchDialog, "search-dialog");
    },

    openSimpleSearchDialog: function (event) {
        this.searchDialogMode = "Simple";
        this.openDialog(this.simpleSearchDialog, "simple-search-dialog");
    },

    openAdvancedSearchDialog: function (searchCriteriaStore) {
        this.searchDialogMode = "Advanced";
        /*
        if (searchCriteriaStore)
        if (this.advancedSearchDialog.searchCriteriaStore.length == 0)
        this.advancedSearchDialog.searchCriteriaStore.push(searchCriteriaStore);
        */
        this.openDialog(this.advancedSearchDialog, "advanced-search-dialog");
    },

    openUserProfileDialog: function (event) {
        this.openDialog(this.userProfileDialog, "user-profile-dialog");
    },

    openLookupDialog: function (columnKey, tb) {
        this.lookupDialog.columnKey = columnKey;
        this.lookupDialog.targetElement = tb;

        var tet = "";
        var sm = "single";
        var dt = "Edit";

        if (typeof tb == "function") {
            dt = "BulkInsert"
            sm = "multiple";
        }
        else if (tb.attr("editFieldType")) {
            dt = "Edit";
            tet = tb.attr("editFieldType")
            if (tet == "MultiValueTextBoxLookup")
                sm = "multiple";
        }
        else if (tb.attr("lookupType")) {
            tet = tb.attr("lookupType");
            dt = "Search";
            sm = "multiple";
        }

        if (tb.attr)
            if (tb.attr("selectionMode"))
                sm = tb.attr("selectionMode");

        this.lookupDialog.dialogType = dt;
        this.lookupDialog.selectionMode = sm;
        this.lookupDialog.targetElementType = tet;

        this.openDialog(this.lookupDialog, "lookup-dialog");
    },

    auditHistory: function (event) {
        var tb = jQuery(event.target).parents(".edit-field-table:first").find(":input");
        if (tb.length > 0)
            this.auditDialog.columnKey = tb.attr("columnKey");
        else
            this.auditDialog.columnKey = null;

        if (this instanceof DbNetEdit)
            this.auditDialog.primaryKey = this.primaryKey;
        else
            this.auditDialog.primaryKey = this.rowPrimaryKey(jQuery(event.target).parents("tr.data-row:first"));

        this.auditDialog.tableName = this.fromPart;

        this.openDialog(this.auditDialog, "audit-dialog");
    },

    openHtmlEditor: function (e) {
        this.openEditor(e, this.htmlEditor, "html");
    },

    openTextEditor: function (e) {
        this.openEditor(e, this.textEditor, "text");
    },

    openEditor: function (e, dialog, id) {
        dialog.targetElement = e;
        dialog.label = "";

        var c = this.columnFromKey(jQuery(e).attr("columnKey"));
        if (c)
            dialog.label = c.label;

        this.openDialog(dialog, id + "-editor");
    },

    initialiseInput: function (e, col, row) {
        switch (e.attr("editFieldType")) {
            case "AutoCompleteLookup":
                e.attr("componentId", this.id);
                e.autocomplete(
					{
					    source: function (request, response) {
					        if (!this.element.is(":visible")) {
					            response([])
					            return;
					        }

					        var data = {};
					        data.token = request.term;
					        data.columnIndex = this.element.attr("columnIndex");
					        var c = DbNetLink.components[e.attr("componentId")];
					        c.addParentParameter(e, data);
					        var result = c.callServer("get-suggested-items", null, data);
					        response(result.items)
					    },

					    select: function (event, ui) {
					        var e = jQuery(this);
					        e.val(ui.item.label);
					        e.attr("displayValue", ui.item.label);
					        e.attr("fieldValue", ui.item.value);
					        e.trigger("change");
					        return false;
					    },

					    focus: function (event, ui) {
					        return false;
					    }
					}
				);
                break;
            case "SuggestLookup":
                var sg = new SuggestLookup(e, this);
                break;
            case "Selectmenu":
                e.selectmenu({
                    width: 200
                });
                break;
        }

        if (e.prop("tagName").toUpperCase() == "TEXTAREA")
            e.maxlength();

        if (e.attr("deleteToClear") == "true")
            e.keydown(this.createDelegate(this.clearTextboxLookup));

        var table = e.parents("table:first");

        if (this instanceof DbNetEdit) {
            table.find(".thumbnail-image").bind("load", this.createDelegate(this.showLoadedImage));
            table.find(".thumbnail-image").bind("click", this.createDelegate(this.openThumbnail));
            table.find(".thumbnail-panel").hide();
            table.find("button[inputButtonType=htmlEdit]").bind("click", this.createDelegate(this.showHtmlEditor));
        }

        table.find("button[inputButtonType=textEdit]").bind("click", this.createDelegate(this.showTextEditor));

        if (e.attr("dataType") == "DateTime")
            this.addDatePicker(e, col);

        if (e.attr("placeholder"))
            e.placeholder();

        table.find(".calendar-button").bind("click", this.createDelegate(this.showCalendar));
        table.find(".lookup-button").bind("click", this.createDelegate(this.showLookup));
        table.find(".imageAdd-button").bind("click", this.createDelegate(this.doUpload));
        table.find(".imageDelete-button").bind("click", this.createDelegate(this.clearUpload));
        table.find(".audit-history").bind("click", this.createDelegate(this.auditHistory));

        var d = this.createDelegate(this.fireOnFieldValueChangedEvent);

        e.attr("alt", e.attr("label"));

        switch (e.attr("editFieldType")) {
            case "CheckBox":
                e.bind("click", d);
                break;
            case "RadioButtonList":
                table.find("input[type=radio]").bind("click", d);
                break;
            case "Html":
                this.htmlEditors.push(e.attr("id"));
                break;
            default:
                e.bind("change", d);
                break;
        }

        e.bind("focus", this.createDelegate(this.inputFocus));
        e.bind("blur", this.createDelegate(this.inputBlur));

        if (e.attr("parentColumn") == null)
            return;

        var ic = this.getInputControl(e.attr("parentColumn"), row);

        if (ic) {
            var dci = this.$(ic).attr("dependentColumnIndex");
            if (dci == "" || dci == null) {
                dci = e.attr("columnIndex");
                this.$(ic).bind("change", this.createDelegate(this.parentColumnChanged));
            }
            else
                dci += "," + e.attr("columnIndex");
            this.$(ic).attr("dependentColumnIndex", dci);

            //this.$(ic).attr("dependentColumnIndex", e.attr("columnIndex"));
            //this.$(ic).bind("change",this.createDelegate(this.parentColumnChanged));
        }
    },

    bindSpellChecker: function (e) {
        if (this.spellCheck) {
            if (e.attr("editFieldType") == "Html")
                e.attr("spellCheck", "true");
            else if (e.attr("dataType") == "String")
                if (parseInt(e.attr("maxlength")) > 29)
                    e.attr("spellCheck", "true");
        }

        if (!e.attr("spellCheck") || e.attr("spellCheck").toLowerCase() != "true")
            return;

        if (e.attr("editFieldType") == "Html")
            if (window.tinyMCE)
                this.dbNetSpell.registerElement(jQuery("#" + e.attr("id") + "_ifr"));
            else
                this.dbNetSpell.registerElement(e.parent().find(".nicEdit-main"));
        else
            this.dbNetSpell.registerElement(e);
    },

    clearTextboxLookup: function (event) {
        if (event.which != 46)
            return;

        this.clearEditFieldValue(jQuery(event.target));
    },

    modifiedInputControls: function () {
        var modified = false;

        var ic = this.allInputControls();

        for (var i = 0; i < ic.length; i++) {
            var editField = ic[i];

            if (editField.prop("disabled") && editField.attr("editFieldType") != "Upload")
                continue;

            var currentValue = this.getEditFieldValue(editField).toString();
            var originalValue = this.getDbValue(editField).toString();

            originalValue = (originalValue == null) ? "" : originalValue;

            var fieldType = editField.attr("editFieldType");
            switch (fieldType) {
                case "HtmlPreview":
                case "Html":
                    originalValue = "";
                    currentValue = "";
                    if (fieldType == "HtmlPreview")
                        if (editField.attr("modified") == "true")
                            currentValue = "1";
                    if (fieldType == "Html")
                        if (window.tinyMCE) {
                            if (tinymce.EditorManager.get(editField[0].id).isDirty())
                                currentValue = "1";
                        }
                        else {
                            if (nicEditors.findEditor(editField[0].id).getContent() != this.nicEditorsContent[editField[0].id])
                                currentValue = "1";
                        }
                    break;
            }

            if (currentValue.replace(/\s/g, "") != originalValue.replace(/\s/g, "")) {
                modified = true;
                switch (fieldType) {
                    case "Upload":
                        editField.parents("table:first").find(".thumbnail-panel").addClass("field-modified");
                        break;
                    default:
                        editField.addClass("field-modified");
                        break;
                }
            }
        }

        if (modified)
            window.setTimeout(this.createDelegate(this.clearModifiedClass), 3000);

        return modified;
    },

    allInputControls: function (ic) {
        var a = [];

        if (!ic)
            ic = this.inputControls;

        if (jQuery.isArray(ic)) {
            for (var i = 0; i < ic.length; i++)
                a = a.concat(this.allInputControls(ic[i]));
            return a;
        }
        for (var cn in ic)
            a.push(ic[cn]);

        return a;
    },

    openThumbnail: function (event) {
        var url = jQuery(event.target).attr("src").replace("method=thumbnail", "method=stream");
        var o = {};

        var args = {};
        args.cell = jQuery(event.target).parents("td:first");
        args.row = args.cell.parent();
        args.fileName = "";
        args.download = false;
        args.inline = false;
        this.fireEvent("onBeforeFilePreview", args);

        if (url.indexOf("data:") == 0) {
            o = this.getDataUri(jQuery(event.target), args.fileName);
            url = o.url;
            delete o.url;
        }

        o.inline = args.inline;

        if (args.download) {
            this.downloadDocument(url, args.fileName);
            return;
        }

        this.openFilePreviewDialog(url, o);
    },

    getDataUri: function (img, fileName) {
        var data = {};
        data.primaryKey = this.rowPrimaryKey(jQuery(img).parents("tr.data-row:first"));

        if (img.attr("columnname"))
            data.columnName = img.attr("columnname");
        else
            data.columnName = img.parent().attr("columnname");

        data.fileName = fileName;

        var r = this.callServer("get-data-uri", null, data);
        return (r);
    },


    clearModifiedClass: function () {
        var ic = this.allInputControls();

        for (var i = 0; i < ic.length; i++) {
            ic[i].removeClass("field-modified");
            ic[i].parents("table:first").find(".thumbnail-panel").removeClass("field-modified");
        }
    },

    getEditFieldValue: function (editField) {
        switch (this.getEditFieldType(editField)) {
            case "CheckBox":
                return editField[0].checked.toString();
                break;
            case "RadioButtonList":
                return this.getRadioButtonListValue(editField);
                break;
            case "HtmlPreview":
                return editField.parents("table:first").find(".html-content").val();
                break;
            case "Html":
                if (window.tinyMCE)
                    return tinymce.EditorManager.get(editField[0].id).getContent();
                else
                    return nicEditors.findEditor(editField[0].id).getContent();
                break;
            case "TextBoxLookup":
            case "TextBoxSearchLookup":
                if (editField.prop("readOnly"))
                    return editField.attr("fieldValue");
                else
                    return editField.val();
                break;
            case "SuggestLookup":
            case "AutoCompleteLookup":
                if (editField.attr("LookupColumnCount"))
                    if (editField.attr("LookupColumnCount") == "1")
                        return editField.val();
                return ((editField.attr("fieldValue") == undefined) ? "" : editField.attr("fieldValue"));
                break;
            default:
                if (editField[0].style.textTransform) {
                    switch (editField[0].style.textTransform.toString()) {
                        case "uppercase":
                            editField.val(editField.val().toUpperCase());
                            break;
                        case "lowercase":
                            editField.val(editField.val().toLowerCase());
                            break;
                    }
                }
                // editField.val(jQuery.trim(editField.val()));
                // return editField.val();
                // jQuery val() method strips carriage returns
                if (editField.attr("placeholder"))
                    return editField.val();
                else
                    return editField[0].value;
                break;
        }
    },

    getRadioButtonListValue: function (editField) {
        var items = editField.parents("table:first").find("input[type=radio]");

        for (var i = 0; i < items.length; i++) {
            var e = items[i];
            if (e.checked)
                return e.value;
        }

        return "";
    },

    getDbValue: function (editField) {
        var v = "";

        switch (this.getEditFieldType(editField)) {
            case "TextBoxLookup":
            case "TextBoxSearchLookup":
            case "SuggestLookup":
            case "AutoCompleteLookup":
                v = editField.attr("dbValue");
                break;
            default:
                v = editField.attr("fieldValue");
                break;
        }

        if (v == null)
            v = "";

        return v;
    },

    setDbValue: function (editField, v) {
        switch (this.getEditFieldType(editField)) {
            case "TextBoxLookup":
            case "TextBoxSearchLookup":
            case "SuggestLookup":
            case "AutoCompleteLookup":
                v = editField.attr("dbValue", v);
                break;
            default:
                v = editField.attr("fieldValue", v);
                break;
        }
    },

    getEditFieldType: function (editField) {
        if (typeof (editField.attr("editFieldType")) == "string") {
            return editField.attr("editFieldType");
        }
        else {
            return "";
        }
    },

    assignToolbarHandlers: function () {
        this.$("toolbar .navigation").bind("click", this.createDelegate(this.navigate));
        this.$("pageSelect").bind("keydown", this.createDelegate(this.pageSelectKeydown));
        this.$("quickSearch").bind("keyup", this.createDelegate(this.quickSearchKeyPress));
        this.$("printBtn").bind("click", this.createDelegate(this.printCopyButtonClick));
        this.$("copyBtn").bind("click", this.createDelegate(this.printCopyButtonClick));
        this.$("xcopyBtn").bind("click", this.createDelegate(this.printCopyButtonClick));
        this.$("saveBtn").bind("click", this.createDelegate(this.saveButtonClick));
        this.$("searchBtn").bind("click", this.createDelegate(this.openSearchDialog));
        this.$("updateRowBtn").bind("click", this.createDelegate(this.editButtonClick));
        this.$("insertRowBtn").bind("click", this.createDelegate(this.editButtonClick));
        this.$("deleteRowBtn").bind("click", this.createDelegate(this.deleteRecord));
        this.$("outputPageSelect").bind("click", this.createDelegate(this.setOutputPage));
        this.$("applyBtn").bind("click", this.createDelegate(this.applyClicked));
        this.$("cancelBtn").bind("click", this.createDelegate(this.cancel));
        this.$("userProfileBtn").bind("click", this.createDelegate(this.openUserProfileDialog));
        this.$("mailMergeBtn").bind("click", this.createDelegate(this.storeMailMergeData));
        this.$("columnPickerBtn").bind("click", this.createDelegate(this.openColumnPickerDialog));
        this.$("sortBtn").bind("click", this.createDelegate(this.openColumnSortDialog));
        this.$("userProfileSelect").bind("change", this.createDelegate(this.userProfileSelected));
        this.$("configBtn").bind("click", this.createDelegate(this.openConfigDialog));
        this.$("chartBtn").bind("click", this.createDelegate(this.chartButtonClick));
        this.$("viewBtn").bind("click", this.createDelegate(this.openViewDialog));
        this.$("uploadBtn").bind("click", this.createDelegate(this.doDataUpload));

        for (b in this.buttonText)
            this.toolButtonText(b, this.buttonText[b]);

        this.addDeferredButtons();
    },

    addDeferredButtons: function () {
        // Overridden by 3.x compatability code
    },

    highlightField: function (editField, msg) {
        this.showMessage(msg);
        try {
            if (editField.attr("editFieldType") == "Upload") {
                var t = editField.parents("table:first");
                t.find(".thumbnail-image").attr("src", dbnetsuite.requiredImgUrl);
                t.find(".thumbnail-panel").addClass("field-highlight");
            }

            editField.addClass("field-highlight");
            editField.focus();
        }
        catch (ex) { };

        window.setTimeout(this.createDelegate(this.clearHighlight), 3000);
    },

    clearHighlight: function (ic) {
        var ic = this.allInputControls();

        for (var i = 0; i < ic.length; i++) {
            if (ic[i].attr("editFieldType") == "Upload") {
                var t = ic[i].parents("table:first");
                t.find(".thumbnail-image").css({ "visibility": "hidden" });
                t.find(".thumbnail-panel").removeClass("field-highlight");
            }
            ic[i].removeClass("field-highlight");
        }
    },

    clearUpload: function (event) {
        var table = jQuery(event.target).parents(".edit-field-table:first");
        var element = table.find("." + this.typeName().toLowerCase() + "-input");
        element.val("");
        var img = table.find(".thumbnail-image");
        img.css({ "visibility": "hidden" });

        if (typeof (elementId) == "string")
            element.trigger("change");
    },

    doUpload: function (event) {
        var element = this.$(event.target).parents(".edit-field-table:first").find("." + this.typeName().toLowerCase() + "-input");
        var data = {}
        data.column = this.columns[element.attr("columnIndex")];
        data.editField = element;

        this.openUploadDialog(data);
    },

    assignUpload: function (editField, key, url, fileName) {
        var img = editField.parents(".edit-field-table:first").find(".thumbnail-image");
        img.attr("src", url);
        this.showLoadedImage({ target: img });

        if (editField.attr("uploadFileNameColumn")) {
            var cn = editField.attr("uploadFileNameColumn");
            if (this.columnIndex(cn) > -1) {
                var row;
                if (this instanceof DbNetGrid)
                    row = this.parentDataRow(editField);

                var e = this.getInputControl(cn, row)
                if (e != null)
                    e.val(fileName);
                else {
                    editField.attr("uploadedFileName", fileName);
                }
            }
            else {
                editField.attr("uploadedFileName", fileName);
            }
        }

        editField.val(key);
        editField.trigger("change");
    },

    addUploadedFileName: function (parameters, inputControls) {
        for (var cn in inputControls) {
            var e = inputControls[cn];
            if (e.attr("uploadedFileName")) {
                parameters[e.attr("uploadFileNameColumn")] = e.attr("uploadedFileName");
            }
        }
    },

    dataUploaded: function (fileName) {
        this.dataUploadDialog.fileName = fileName;
        this.dataUploadDialog.tableName = this.uploadDataTable;
        this.openDialog(this.dataUploadDialog, "data-upload-dialog");
    },

    showCalendar: function (event) {
        var tb = jQuery(event.target).parents(".edit-field-table:first").find("input");
        tb.datepicker("show");
    },

    showHtmlEditor: function (event) {
        /* 
        if (!window.tinyMCE)
        {
        alert("TinyMCE libraries not available");
        return;
        }
        */

        var e = jQuery(event.target).parents(".edit-field-table:first").find(".html-preview");
        this.openHtmlEditor(e);
    },

    showTextEditor: function (event) {
        var tb = jQuery(event.target).parents(".edit-field-table:first").find("textarea");
        this.openTextEditor(tb);
    },

    showLookup: function (event) {
        var tb = jQuery(event.target).parents(".edit-field-table:first").find("input");
        this.openLookupDialog(tb.attr("columnKey"), tb);
    },

    inputFocus: function (event) {
        var e = this.$(event.target);
        e.addClass(this.typeName().toLowerCase() + "-focus");

        if (e.attr("editFieldType") == "AutoCompleteLookup") {
            if (e.attr("fieldValue") == "" || e.attr("fieldValue") == undefined)
                if (e.attr("mandatory").toString().toLowerCase() == "true") {
                    var data = {};
                    data.columnIndex = e.attr("columnIndex");
                    this.addParentParameter(e, data);
                    this.callServer("peek-lookup", { method: this.peekLookupCallback, params: [e] }, data);
                }
        }
    },

    peekLookupCallback: function (r, e) {
        if (r.items == 1) {
            e.val(r.label);
            e.attr("displayValue", r.label);
            e.attr("fieldValue", r.value);
            e.trigger("change");
        }
        else if (r.items < 10) {
            if (e.val() == "")
                e.autocomplete("search", "%");
        }
    },

    inputBlur: function (event) {
        this.$(event.target).removeClass(this.typeName().toLowerCase() + "-focus");
    },

    fireOnFieldValueChangedEvent: function (e) {
        var editField = this.$(e.target).parents("table.edit-field-table:first").find("." + this.typeName().toLowerCase() + "-input")[0];
        var args = {};
        args.editField = editField;
        args.columnName = editField.getAttribute("columnName").toLowerCase();

        if (this instanceof DbNetGrid)
            args.row = this.parentDataRow(editField);

        var e = jQuery(editField)
        switch (this.getEditFieldType(e)) {
            case "AutoCompleteLookup":
                if (e.val() == "") {
                    e.attr("displayValue", "");
                    e.attr("fieldValue", "");
                }
                break;
        }

        this.fireEvent("onFieldValueChanged", args);
    },

    preValidationCheck: function (updateMode) {
        var args = { cancel: false, message: "" };
        this.fireEvent("onBeforeRecordValidated", args);

        if (args.cancel) {
            if (args.message != "")
                this.showMessage(args.message);
            return false;
        }

        return this.checkRequiredFields(updateMode);
    },

    checkRequiredFields: function (updateMode) {
        var ic = this.allInputControls();

        for (var i = 0; i < ic.length; i++) {
            var editField = ic[i];
            if (this.getEditFieldValue(editField) == "") {
                //if (editField.attr("encryption").toString().toLowerCase() != "none" && updateMode)
                //    continue;

                if (editField.attr("mandatory").toString().toLowerCase() == "true") {
                    var msg = this.translate("ValueRequired")
                    if (!editField.is(":visible"))
                        msg += " (" + editField.attr("label") + ")"
                    this.highlightField(editField, msg);
                    return false;
                }
            }
        }

        return true;
    },

    getUpdateParams: function (inputControls) {
        var params = {};
        for (var cn in inputControls) {
            var editField = inputControls[cn];
            var colIndex = editField.attr("columnIndex");

            /*
            if ( this.mode == "update" )
            if (editField.attr( this.mode + "ReadOnly").toLowerCase() == "true")
            continue;	
            */

            if (this.mode == "insert") {
                if (this.getEditFieldValue(editField) == "")
                    continue;
            }
            else if (this.getEditFieldValue(editField).toString() == this.getDbValue(editField).toString())
                continue;

            params[cn.toLowerCase()] = this.getEditFieldValue(editField);
        }
        return params;
    },

    applySearch: function (searchDialog) {
        var args = { dialog: searchDialog, cancel: false, message: '' };
        this.fireEvent("onSearchDialogApply", args);

        if (args.cancel) {
            if (args.message != '')
                searchDialog.showMessage(args.message);
            return;
        }

        this.searchFilterText = searchDialog.filterText();
        this.searchFilter = searchDialog.searchFilter;
        this.searchCriteriaStore = searchDialog.searchCriteriaStore;

        this.searchFilterSql = [];
        this.searchFilterParams = {};

        this.currentPage = 1;
        this.getRecordSet();
    },

    saveAutomatically: function (savedCallback, parentControl, params) {
        if (!this.autoSave)
            return false;

        if (this.recordHasChanged() == false)
            return false;

        this.reloadAfterUpdate = false;

        var eventName = (parentControl instanceof DbNetGrid) ? "onGridAutoSaveCompleted" : "onEditAutoSaveCompleted";

        var handler = function (sender, args) { sender.invokeCallback(savedCallback, [params]) };
        parentControl.unbind(eventName)
        parentControl.bind(eventName, handler)

        this.apply();

        return true;
    },

    deleteRecord: function (e) {
        if (!this.isRowSelected())
            return;
        var args = { cancel: false, message: "" };
        this.fireEvent("onBeforeRecordDeleted", args);

        if (args.cancel) {
            if (args.message == "")
                args.message = "Record cannot be deleted";
            this.showMessage(args.message);
            return;
        }
        if (this.confirmDeletion)
            this.messageBox("question", this.translate("ConfirmDelete"), this.createDelegate(this.deleteRecordCallback));
        else
            this.deleteRecordCallback();
    },

    isRowSelected: function () {
        if (this instanceof DbNetGrid)
            if (this.selectedRows().length == 0) {
                this.showMessage(this.translate("ARowMustBeSelected"));
                return false;
            }
        return true;
    },

    deleteRecordCallback: function (buttonPressed) {
        if (buttonPressed != "yes")
            return;

        var pks = [];
        if (this instanceof DbNetEdit)
            pks.push(this.primaryKey);
        else if (this instanceof DbNetGrid) {
            var selectedRows = this.selectedRows();
            if (selectedRows.length == 0) {
                this.showMessage(this.translate("ARowMustBeSelected"));
                return;
            }

            for (var i = 0; i < selectedRows.length; i++)
                pks.push(this.rowPrimaryKey(selectedRows[i]));
        }

        var data = {};
        data.pks = pks;

        this.callServer("delete-record", this.deleteApplied, data);
    },

    deleteApplied: function (response) {
        var args = { message: response.message };

        if (!response.ok) {
            this.fireEvent("onRecordDeleteError", args);
            this.showMessage(args.message);
            return;
        }

        args.deleted = this.deserializeRecord(response.records);
        this.fireEvent("onRecordDeleted", args);
        if (args.message != "") {
            this.showMessage(args.message, 1);
        }

        if (this instanceof DbNetEdit) {
            this.refreshParentControls("delete");
            this.getRecordSet();
        }
        else {
            this.reload();
        }
    },

    reload: function () {
        this.rowIndex = parseInt(this.selectedRows()[0].getAttribute("dataRowIndex"));
        this.loadData();
    },

    columnFromKey: function (key) {
        for (var i = 0; i < this.columns.length; i++)
            if (this.columns[i].columnKey == key)
                return this.columns[i];
        return null;
    },

    getInputControl: function (columnExpression, row) {
        var idx = columnExpression;
        if (isNaN(idx))
            idx = this.columnIndex(columnExpression);

        if (idx == -1) {
            alert("Input control [" + columnExpression + "] not found");
            return null;
        }

        var col = this.columns[idx];
        if (!col)
            return null;
        if (row && this.typeName() == "DbNetGrid") {
            row = this.parentDataRow(row);
            var idx = row.attr("dataRowIndex");
            return this.inputControls[idx][col.columnName];
        }
        return this.inputControls[col.columnName];
    },

    customValidation: function (parameters, currentRecord, row) {
        for (var key in parameters) {
            var col = this.columns[this.columnIndex(key)];
            var args = {};
            args.columnName = col.columnName;
            args.columnValue = this.deserialize(parameters[key]);
            args.cancel = false;
            args.message = "Column validation failed";

            this.fireEvent("onColumnValidate", args);

            if (args.cancel) {
                this.highlightField(this.getInputControl(col.columnName, row), args.message);
                return false;
            }

            if (args.columnValue != this.deserialize(parameters[key]))
                parameters[key] = args.columnValue;
        }

        this.deserializeRecord(currentRecord);

        //	for (var key in currentRecord )
        //		if (currentRecord[key].toUTCString)
        //			currentRecord[key] = this.getLocalDateObject(currentRecord[key]);

        var args = {};
        args.parameters = parameters;
        args.currentRecord = currentRecord;
        args.cancel = false;
        args.message = "Row validation failed";
        args.columnToHighlight = "";
        this.fireEvent("onRecordValidate", args);

        if (args.cancel) {
            if (args.columnToHighlight != "")
                this.highlightField(this.getInputControl(args.columnToHighlight, row), args.message);
            else
                this.showMessage(args.message);
            return false;
        }

        return true;
    },

    getLocalDateObject: function (d) {
        var offset = (0 + d.getTimezoneOffset()) * 60000;
        return new Date(d.valueOf() + offset);
    },

    getSuggestedItems: function (suggestLookup) {
        var data = {};
        var e = suggestLookup.element;
        if (e.val() == "" && e.attr("autosuggest") == "true") {
            data.token = "%";
            e.attr("autosuggest", "");
        }
        else
            data.token = e.val();
        data.columnIndex = e.attr("columnIndex");
        this.addParentParameter(e, data);
        this.callServer("get-suggested-items", { method: this.getSuggestedItemsCallback, params: [suggestLookup] }, data);
    },

    addParentParameter: function (field, data) {
        data.params = {};

        if (field.attr("parentColumn")) {
            var e = this.getInputControl(field.attr("parentColumn"), field);
            data.params[e.attr("columnName")] = this.getEditFieldValue(e).toString();
        }
    },

    parentColumnChanged: function (e) {
        var a = this.$(e.target).attr("dependentColumnIndex").split(",");
        for (var i = 0; i < a.length; i++) {
            var idx = parseInt(a[i]);
            var te = this.getInputControl(idx, e.target);
            if (te.prop("tagName").toUpperCase() != "SELECT")
                this.clearEditFieldValue(te);
            else
                this.loadDependentDropDown(e.target, idx);
        }
    },

    loadDependentDropDown: function (e, columnIndex) {
        var data = {};
        data.value = this.getEditFieldValue(jQuery(e));
        data.targetColumnIndex = parseInt(columnIndex);
        data.dialog = false;
        var te = this.siblingControl(e, data.targetColumnIndex);

        if (te.prop("tagName").toUpperCase() != "SELECT")
            return;

        this.callServer("get-options", { method: this.loadDependentDropDownCallback, params: [te] }, data);
    },

    siblingControl: function (e, idx) {
        if (this.typeName() == "DbNetGrid")
            return this.siblingInputControls(e, idx);
        else
            return this.inputControls[this.columns[idx].columnName];
    },

    loadDependentDropDownCallback: function (response, targetElement) {
        this.assignOptions(targetElement, response.html);
        this.fireEvent("onDependentListLoaded", { target: targetElement });
    },

    refreshListOptions: function (columnName, options, row) {
        var e = this.getInputControl(columnName, row);

        if (!e)
            if (this instanceof DbNetGrid && this.editDialog.editControl) {
                this.editDialog.editControl.refreshListOptions(columnName, options);
                return;
            }

        if (options)
            if (jQuery.isArray(options)) {
                this.assignOptions(e, options);
                return;
            }

        var target = e;

        if (e.attr("parentColumn"))
            target = this.getInputControl(e.attr("parentColumn"));

        if (typeof options == "string")
            if (options.toLowerCase().indexOf("select ") == 0)
                this.columns[parseInt(e.attr("columnIndex"))].lookup = options;

        this.loadDependentDropDown(target, parseInt(e.attr("columnIndex")));
    },

    assignOptions: function (editField, options) {
        if (editField.prop("tagName").toUpperCase() != "SELECT")
            return;

        var v = editField.attr("fieldValue");
        editField.html("");

        if (jQuery.isArray(options)) {
            for (var i = 0; i < options.length; i++) {
                var item = options[i]
                var o = document.createElement("option")

                if (jQuery.isArray(item)) {
                    o.text = item[0];
                    o.value = item[item.length - 1];
                }
                else {
                    o.text = item;
                    o.value = item;
                }
                editField[0].options.add(o)
            }
        }
        else {
            var buf = this.addDOMElement("dbnetsuite_option_buffer");
            var c = buf.html(options).children()[0];

            for (var i = 0; i < c.options.length; i++) {
                var o = document.createElement("option")
                o.text = c.options[i].text
                o.value = c.options[i].value

                editField[0].options.add(o)
            }
        }
        editField.val(v);

    },

    clearEditFieldValue: function (editField) {
        editField.attr("fieldValue", "");
        var ef = editField[0];

        switch (this.getEditFieldType(editField)) {
            case "CheckBox":
                ef.checked = false;
                editField.attr("fieldValue", ef.checked.toString());
                break;
            case "RadioButtonList":
                this.clearRadioButtonListValue(editField);
                break;
            case "HtmlPreview":
                editField.html("");
                break;
            case "Html":
                if (window.tinyMCE) {
                    if (tinymce.EditorManager.get(ef.id))
                        tinymce.EditorManager.get(ef.id).setContent("");
                }
                else {
                    nicEditors.findEditor(ef.id).setContent("");
                }
                break;
            case "Upload":
                this.clearUpload({ target: ef });
                break;
            case "TextBoxLookup":
            case "TextBoxSearchLookup":
            case "SuggestLookup":
            case "AutoCompleteLookup":
                editField.val("");
                editField.attr("displayValue", "");
            case "Upload":
                editField.attr("guid", "");
                break;
            default:
                editField.val("");
                break;
        }

        if (editField.attr("parentColumn") != null)
            if (ef.tagName == "SELECT")
                editField.html("");

    },

    openColumnSortDialog: function () {
        this.openDialog(this.columnSortDialog, "column-sort-dialog");
    },

    addLinkedControl: function (ctrl, oneToOne) {
        if (ctrl.fromPart == "") {
            alert("Linked control [" + ctrl.id + "] must be configured before being added")
            return;
        }

        if (ctrl.connectionString == "") {
            ctrl.connectionString = this.connectionString;
            ctrl.commandTimeout = this.commandTimeout;
            ctrl.caseInsensitiveSearch = this.caseInsensitiveSearch;
        }

        if (typeof oneToOne == "undefined")
            oneToOne = (this.fromPart.toLowerCase() == ctrl.fromPart.toLowerCase());

        //if (ctrl.search == null)
        //	ctrl.search = false

        if (oneToOne) {
            ctrl.deleteRow = false

            if (ctrl instanceof DbNetEdit) {
                this.updateRow = false;
                this.insertRow = false;
            }
            else {
                ctrl.insertRow = false;
                ctrl.search = false;
                ctrl.navigation = false;
            }
        }

        ctrl.parentControls.push({ ctrl: this, oneToOne: oneToOne });
        ctrl.parentControl = this;
        this.childControls.push({ ctrl: ctrl, oneToOne: oneToOne });

        ctrl.parentFilterSql = ["1=2"];
        //4.4ctrl.initialize(this);
    },

    getSuggestedItemsCallback: function (response, suggestLookup) {
        suggestLookup.getDataCallback(response);
    },

    showLoadedImage: function (event) {
        this.$(event.target).css({ "visibility": "visible" }).parent().show();
        this.fireEvent("onImageLoaded");
    },

    bindToParent: function (parentControl) {
        /*
        parentControl.bind("onPageLoaded", this.createDelegate(this.parentControlLoaded));
        var evtName = ""
        if (parentControl instanceof DbNetEdit)
        evtName = "onRecordSelected"
        else
        evtName = "onRowSelected"			
        parentControl.bind(evtName, this.createDelegate(this.parentRowSelected));
        */
        this.parentFilterSql = ["1=2"]
    },

    notifyChildControlsParentPageLoaded: function () {
        //debug.log(this, "notifyChildControlsParentPageLoaded:" + this.childControls.length.toString());
        for (var i = 0; i < this.childControls.length; i++)
            this.childControls[i].ctrl.parentControlLoaded(this);
    },

    notifyChildControlsParentRowSelected: function () {
        //debug.log(this, "notifyChildControlsParentRowSelected:" + this.childControls.length.toString());
        for (var i = 0; i < this.childControls.length; i++)
            this.childControls[i].ctrl.parentRowSelected(this);
    },

    parentControlLoaded: function (parentControl) {
        //debug.log(this, "parentControlLoaded");
        if (parentControl.totalPages == 0) {
            this.clear();
            return;
        }

        if (this.parentControls[0].oneToOne && this instanceof DbNetEdit) {
            this.primaryKeyList = parentControl.primaryKeyList;
            this.totalPages = this.primaryKeyList.length;
            this.totalRows = this.totalPages;
        }
    },

    clear: function (p) {
        this.parentFilterSql = ["1=2"];
        this.parentFilterParams = {};
        this.getRecordSet();
    },

    parentRowSelected: function (parentControl) {
        //debug.log(this, "parentControlLoaded");

        /* 4.4
        if (this instanceof DbNetEdit)
        if (parentControl instanceof DbNetGrid)
        if (!this.initialized)
        return;
        */

        if (this.parentControls[0].oneToOne && this instanceof DbNetEdit) {
            if (this.dialog) {
                this.selectRecord(parentControl.rowPrimaryKey());
            }
            else {
                var rowIndex = parseInt(parentControl.selectedRows()[0].getAttribute("dataRowIndex")) + 1;
                if (!this.initialized) {
                    this.primaryKeys = parentControl.primaryKeyList;
                    this.parentFilterSql = [];

                    if (rowIndex > 1) {
                        this.primaryKey = this.primaryKeyList[rowIndex-1];
                    }

                    this.initialize();
                }
                else {
                    this.currentPage = rowIndex;
                    this.selectRecord();
                }
            }
        }
        else
            this.loadChildRecordset(parentControl)
    },

    loadChildRecordset: function (parentControl) {
        this.assignParentFilter(parentControl, parentControl.rowPrimaryKey(), false);

        this.currentPage = 1;
        this.getRecordSet();
    },

    checkSpelling: function () {
        if (this.dbNetSpell.elements.length > 0)
            this.dbNetSpell.checkSpelling();
    },

    saveUserProfile: function (data) {
        var profileProperties = this.getProfileProperties()
        var args = {};
        args.profile = profileProperties;
        this.fireEvent("onBeforeUserProfileSaved", args);
        data.profileProperties = profileProperties;
        this.callServer("save-user-profile", { method: this.userProfileDialog.profileSaved, context: this.userProfileDialog }, data);
    },

    getProfileProperties: function () {
        var pp = this.profileProperties().concat(this.customProfileProperties);
        var profileProperties = {};
        for (var i = 0; i < pp.length; i++) {
            var pn = pp[i];
            if (typeof this[pn] == "undefined") {
                if (jQuery(pn).length > 0)
                    profileProperties[pn] = this.getInputValue(jQuery(pn));
                else
                    profileProperties[pn] = window[pn];
            }
            else if (typeof this[pn] == "function")
                profileProperties[pn] = this[pn].apply(this, []);
            else
                profileProperties[pn] = this[pn];
        }
        return profileProperties;
    },

    loadUserProfiles: function () {
        var data = {};
        this.callServer("load-user-profiles", { method: this.userProfileDialog.profilesLoaded, context: this.userProfileDialog }, data);
    },

    getInputValue: function (e) {
        if (e.length)
            if (e.attr("type") == "checkbox")
                return e[0].checked
            else
                return e.val();
        return null;
    },

    setInputValue: function (e, v) {
        if (e.length)
            if (e.attr("type") == "checkbox")
                e[0].checked = eval(v);
            else
                e.val(v);
    },

    deleteUserProfile: function (profileID) {
        var data = {};
        data.profileID = profileID;
        this.callServer("delete-user-profile", { method: this.userProfileDialog.profilesLoaded, context: this.userProfileDialog }, data);
    },

    userProfileSelected: function (event) {
        this.selectUserProfile(jQuery(event.target).val());
    },

    selectUserProfile: function (id) {
        this.restoreUserProfile(id);
        if (this.userProfileDialog.profileSelect) {
            this.userProfileDialog.profileSelect.val(id);
            this.userProfileDialog.selectProfile(false);
        }
    },

    selectDefaultUserProfile: function (id) {
        this.$("userProfileSelect").val(id);
        this.userProfileDialog.defaultUserProfileId = id;
        this.selectUserProfile(id);
    },

    restoreUserProfile: function (profileID) {
        if (profileID == "") {
            this.applyProfileProperties(this.profileOnLoad);
            return;
        }
        var data = {};
        data.profileID = profileID;
        this.callServer("select-user-profile", this.restoreUserProfileCallback, data);
    },

    restoreUserProfileCallback: function (response) {
        if (response.profileProperties == "")
            return;

        var properties = JSON.parse(response.profileProperties);

        var args = {};
        args.profile = properties;

        this.fireEvent("onBeforeUserProfileRestored", args);

        this.applyProfileProperties(properties);
    },

    applyProfileProperties: function (properties) {
        for (var pn in properties) {
            if (typeof this[pn] == "undefined") {
                if (jQuery(pn).length > 0)
                    this.setInputValue(jQuery(pn), properties[pn]);
                else
                    window[pn] = properties[pn];
            }
            else if (typeof this[pn] == "function")
                this[pn].apply(this, [properties[pn]]);
            else
                this[pn] = properties[pn];
        }

        if (typeof (this.searchCriteriaStore) == "string") {
            this.searchDialogMode = "simple";
            this.$(".quick-search").val(this.searchCriteriaStore);
        }
        var sd;
        switch (this.searchDialogMode.toLowerCase()) {
            case "simple":
                sd = this.simpleSearchDialog;
                break;
            case "advanced":
                sd = this.advancedSearchDialog;

                if (!this.searchDialog.configured) {
                    this.searchDialog.autoOpen = false;
                    var r = this.callServer("search-dialog", null, this.searchDialog.data);
                    this.searchDialog.build(r);
                }

                break;
            default:
                sd = this.searchDialog;
                break;
        }
        if (this.groupBy)
            this.orderBy = "";

        if (this instanceof DbNetGrid)
            this.chartConfigDialog.restore();

        sd.component = this;
        sd.restore(this.searchCriteriaStore);
        var args = {};
        args.profile = properties;
        this.fireEvent("onUserProfileRestored", args);

        if (this.searchDialogMode.toLowerCase() == "simple")
            this.runSimpleSearch(this.searchCriteriaStore);
        else
            this.applySearch(sd);
    },


    downloadDocument: function (url, fileName) {
        url += "&download=true";

        if (fileName)
            url += "&filename=" + fileName;

        window.location.href = url;
    },

    handleUpdateError: function (response) {
        var ic;
        if (this instanceof DbNetGrid)
            ic = this.inputControls[response.rowIndex];
        else
            ic = this.inputControls;

        if (response.uniqueConstraintViolated) {
            this.fireEvent("onUniqueConstraintViolated", response);
            for (var i = 0; i < this.columns.length; i++)
                if (this.columns[i].unique)
                    this.highlightField(ic[this.columns[i].columnName], response.message);
        }
        else {
            if (this.mode == "insert" && this instanceof DbNetEdit)
                this.fireEvent("onRecordInsertError", response);
            else
                this.fireEvent("onRecordUpdateError", response);
            try {
                if (response.columnName)
                    this.highlightField(ic[response.columnName], response.message);
                else
                    this.showMessage(response.message);
            }
            catch (ex) {
            }
        }
    },

    searchDialogFilter: function () {
        this.beforeGetRecordSet()
        var filter = this.callServer("search-dialog-filter");
        for (var p in filter.params) {
            filter.params[p] = this.deserialize(filter.params[p]);
            if (filter.params[p] instanceof Date)
                filter.params[p] = this.adjustServerTime(filter.params[p]);
        }
        return filter;
    },

    adjustServerTime: function (v) {
        var offset = (dbnetsuite.serverTimeZoneOffset + v.getTimezoneOffset()) * 60 * 1000;
        return new Date(v.getTime() + offset);
    },

    reAdjustTimeZone: function (v) {
        var offset = (v.getTimezoneOffset()) * 60 * 1000;
        return new Date(v.getTime() + offset);
    },

    adjustDatePrimaryKeyTimeZone: function () {
        for (var i = 0; i < this.primaryKeyList.length; i++) {
            this.primaryKeyList[i] = this.adjustDateTimeZone(this.primaryKeyList[i]);
        }
    },

    adjustDateTimeZone: function (primaryKey) {
        for (p in primaryKey) {
            var pk = this.deserialize(primaryKey[p]);
            if (pk instanceof Date) {
                primaryKey[p] = pk.toJSON();
            }
        }
        return primaryKey;
    },

    configureSearchPanel: function (response) {
        var searchPanel = jQuery("#" + this.searchPanelId);

        if (searchPanel.length == 0)
            searchPanel = jQuery("." + this.searchPanelId);

        if (searchPanel.length == 0)
            return;

        this.searchDialog = new SearchPanel(this);
        this.searchDialog.dialogContainer = searchPanel;
        this.searchDialog.dialogContainer.show().html(response.searchPanel);
        this.searchDialog.configure();
    },

    doDataUpload: function (event) {
        if (this.uploadDataFolder == "")
            this.showMessage("<b>UploadDataFolder</b> property has not been specified");
        else
            this.openUploadDialog({});
    },

    assignParentFilter: function (parentControl, primaryKey, oneToOne) {
        if (this instanceof DbNetGrid) {
            this.columnFilterSql = [];
            this.columnFilterParams = {};
        }

        if (parentControl instanceof DbNetCombo) {
            if (parentControl.combo.children().length == 0) {
                this.parentFilterSql = ["1=2"];
                return;
            }
        }
        else if (parentControl.totalPages == 0) {
            this.parentFilterSql = ["1=2"];
            return;
        }

        this.parentFilterSql = [];
        this.parentFilterParams = {};

        var keyType = (oneToOne ? "primaryKey" : "foreignKey");

        for (var col in primaryKey) {
            if (this.columns.length == 0) {
                for (var cn in this.columnProperties) {
                    var properties = this.columnProperties[cn];
                    for (var p in properties) {
                        if (p.toLowerCase() == keyType.toLowerCase()) {
                            var pname = cn.replace(/[^A-Za-z]/g, "");
                            this.parentFilterSql.push(cn + " = {@}" + pname);
                            this.parentFilterParams[pname] = primaryKey[col];
                        }
                    }
                }
            }
            else {
                for (var i = 0; i < this.columns.length; i++) {
                    if (this.columns[i][keyType]) {
                        var ck = this.columns[i].columnKey;
                        if (!ck)
                            ck = i;
                        var paramName = "detailCol" + ck + "Param0";
                        if (this.parentFilterParams[paramName])
                            continue;
                        this.parentFilterSql.push("{col" + ck + "} = " + paramName);
                        this.parentFilterParams[paramName] = primaryKey[col];
                        break;
                    }
                }
            }
        }

        var args = { parentControl: parentControl };
        this.fireEvent("onParentFilterAssigned", args);
    }
});
