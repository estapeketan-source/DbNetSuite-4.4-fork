var UserProfileDialog = Dialog.extend({
	init: function(parentControl){
		this._super(parentControl,"user-profile");
		this.profileSelect = null;
		this.profileName = null;
		this.initialLoad = true;
		this.defaultUserProfileId = null;
	},
	
	configure : function() {
		this._super();
		this.dialog.find(".close-button").bind("click",this.createDelegate(this.close));
		this.dialog.find(".save-button").bind("click",this.createDelegate(this.saveProfile));
		this.dialog.find(".select-button").bind("click",this.createDelegate(this.profileSelected));
		this.dialog.find(".delete-button").bind("click",this.createDelegate(this.deleteProfile));
		this.profileSelect = this.dialog.find(".user-profile-select");
		this.profileName = this.dialog.find(".user-profile-name");

		this.parentControl.loadUserProfiles();

		this.open();
	},

	saveProfile : function() {
	    var data = {};
	    data.profileTitle = this.dialog.find(".user-profile-name").val();
	    data.defaultProfile = this.dialog.find(".user-profile-default").prop('checked');

	    if (data.profileTitle != "")
			this.parentControl.saveUserProfile(data);
	},	
	
	deleteProfile : function() {
		this.parentControl.deleteUserProfile(this.profileSelect.children(":selected").val());
	},

    
	profileSelected: function () {
	    this.selectProfile(true);
	},	
	
	selectProfile : function(restore) {
	    var opt = this.profileSelect.children(":selected");
	    if (!opt.length)
	        return;

		this.dialog.find(".user-profile-name").val(opt.text());
		if (this.dialog.find(".user-profile-default").length)
    		this.dialog.find(".user-profile-default").prop('checked', opt.attr("default_profile").toString() == "True");

		if (!restore)
		    return;

		this.parentControl.restoreUserProfile(opt.val());
		this.parentControl.$("userProfileSelect").val(opt.val());
	},		
	
	profileSaved : function(response) {
		this.showMessage("Profile saved");
		if (response.items)
		    this.profilesLoaded(response);
		else {
		    if (this.dialog.find(".user-profile-default").length) {
		        this.profileSelect.children().attr("default_profile", "False")
		        this.profileSelect.children(":selected").attr("default_profile", this.dialog.find(".user-profile-default").prop('checked').toString().toProperCase());
		    }
		}
	},
		
	profilesLoaded: function (response) {
		this.loadCombo(this.profileSelect, response.items, false, "");

		this.dialog.find(".delete-button")[0].disabled = (this.profileSelect.children().length == 0);
		this.dialog.find(".select-button")[0].disabled = (this.profileSelect.children().length == 0);
		
		var v = this.profileSelect.find("option:contains('" + this.profileName.val() + "')").val();
		
		this.profileSelect.val( v );
		
		if (this.initialLoad) {
		    if (this.parentControl.userProfileSelect)
		        this.profileSelect.val(this.parentControl.$("userProfileSelect").val());
		    else
		        this.profileSelect.val(this.defaultUserProfileId);
		    this.selectProfile(false);
		}
		else if (this.parentControl.userProfileSelect) {
		    var combo = jQuery(this.parentControl.$("userProfileSelect"));
		    this.loadCombo(combo, response.items, false, "");
		    combo.val(v);
		}

		this.initialLoad = false;
	},
			
	apply : function(event) {
	}	
});
