function overrideValues() {
    var overrideValues = Xrm.Page.getAttribute("wowc_overridevalues").getValue();
    var overrideFields = ["wowc_name", "wowc_epvalue", "wowc_classspecmodifier", "wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_rarity", "wowc_slot", "wowc_category", "wowc_dropsfrom"];

    if (overrideValues == 1) {
        for (var i = 0; i < overrideFields.length; i++) {
            Xrm.Page.getControl(overrideFields[i]).setDisabled(false);
        }
    }
    else {
        for (var i = 0; i < overrideFields.length; i++) {
            Xrm.Page.getControl(overrideFields[i]).setDisabled(true);
        }
    }
}
function fieldLockUnlock() {
    var name = Xrm.Page.getAttribute("wowc_name").getValue();
    var lockUnlockFields = ["wowc_name", "wowc_epvalue", "wowc_classspecmodifier", "wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_rarity", "wowc_slot", "wowc_category","wowc_dropsfrom"];

    if (name == null) {
        for (var i = 0; i < lockUnlockFields.length; i++) {
            Xrm.Page.getControl(lockUnlockFields[i]).setDisabled(false);
        }
    }
    else {
        for (var i = 0; i < lockUnlockFields.length; i++) {
            Xrm.Page.getControl(lockUnlockFields[i]).setDisabled(true);
        }
        Xrm.Page.getAttribute("wowc_overridevalues").setValue(false);
    }
}
function showHideFields() {
    var recordType = Xrm.Page.getAttribute("wowc_type").getValue();
    var setVisableFields = ["wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_classspecmodifier", "wowc_rarity", "wowc_slot", "wowc_defaultgp", "wowc_huntergpvalue", "wowc_tankgpvalue", "wowc_efforttype", "wowc_dropsfrom"];
    var setRequiredFields = ["wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_rarity", "wowc_slot", "wowc_efforttype"];
    var slotType = Xrm.Page.getAttribute("wowc_slot").getValue();

    if (recordType == 257260001) {
        
        for (var i = 0; i < setVisableFields.length; i++) {
            Xrm.Page.getControl(setVisableFields[i]).setVisible(false);
        }
        for (var i = 0; i < setRequiredFields.length; i++) {
            Xrm.Page.getAttribute(setRequiredFields[i]).setRequiredLevel("none");
        }
    }
    else {

        for (var i = 0; i < setVisableFields.length; i++) {
            Xrm.Page.getControl(setVisableFields[i]).setVisible(true);
        }
        for (var i = 0; i < setRequiredFields.length; i++) {
            Xrm.Page.getAttribute(setRequiredFields[i]).setRequiredLevel("required");
        }
        if (slotType != 257260008 && slotType != 257260009 && slotType != 257260007) {
            Xrm.Page.getAttribute("wowc_efforttype").setRequiredLevel("none");
        }
    }
}
